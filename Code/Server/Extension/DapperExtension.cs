using Dapper;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Extension
{
    public static class DapperExtension
    {
        private static readonly ConcurrentDictionary<Type, string> s_modelNameDict = new();
        private static readonly ConcurrentDictionary<Type, QueryParam> s_queryParamDict = new();
        private static readonly ConcurrentDictionary<Type, string> s_pkWhereClauseDict= new();

        // 여러 필드를 기본 키로 설정하는 메서드


        public static void Init<T>(params string[] keyFields)
        {
            var type = typeof(T);
            var tableName = type.Name;
            if (tableName.EndsWith("Model"))
            {
                tableName = tableName.Substring(0, tableName.Length - 5);
            }
            s_modelNameDict[type] = tableName;
            SetPKWhereClause<T>(keyFields);
            SetQueryParameter<T>(keyFields);
        }

        public static T Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction)
        {
            var queryParam = GetQueryParameter<T>();

            var tableName = GetTableName<T>();
            // `Id` 속성 존재 여부 확인
            var hasAutoIncreaseProperty = tableName != "Player" && typeof(T).GetProperty("Id") != null;

            string insertSql = $@"
                INSERT INTO {queryParam.TableName} ({queryParam.Fields}) 
                VALUES ({queryParam.Parameters});";

            // Id가 있는 경우 추가적으로 SELECT 실행
            if (hasAutoIncreaseProperty)
            {
                insertSql += $@"
                SELECT * FROM {queryParam.TableName} WHERE Id = CONVERT(LAST_INSERT_ID(), UNSIGNED);";
                var mdl =  connection.QuerySingleOrDefault<T>(insertSql, entity, transaction);
                ReqHelper.ValidParam(mdl != null, "INSERT_FAIL");
                return mdl;
            }
            else
            {
                // Id가 없으면 INSERT만 수행
                connection.Execute(insertSql, entity, transaction);
                return entity;
            }
        }

        public static void Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryParam = GetQueryParameter<T>();
            var whereClause = GetWhereClause<T>();

            // Build UPDATE SQL
            string updateSql = $@"
            UPDATE {tableName} 
            SET {queryParam.UpdateSet} 
            WHERE {whereClause};";

            connection.Execute(updateSql, entity, transaction);
        }

        public static T SelectByPk<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryParam = GetQueryParameter<T>();
            var whereClause = GetWhereClause<T>();

            string selectSql = $@"SELECT * FROM {tableName} WHERE {whereClause};";

            return connection.QuerySingleOrDefault<T>(selectSql, keyValues, transaction);
        }

        public static T SelectByConditions<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryParam = GetQueryParameter<T>();
            var queryBuilder = new StringBuilder();

            // 기본 쿼리
            queryBuilder.Append($"SELECT * FROM {tableName}");

            // 조건이 있을 경우 WHERE 절 추가
            if (keyValues != null)
            {
                var whereClauses = new List<string>();
                var properties = keyValues.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var key = property.Name;
                    var value = property.GetValue(keyValues);

                    // 조건이 null이 아니면 WHERE 절에 추가
                    if (value != null)
                    {
                        whereClauses.Add($"`{key}` = @{key}");
                    }
                }

                if (whereClauses.Any())
                {
                    queryBuilder.Append(" WHERE ");
                    queryBuilder.Append(string.Join(" AND ", whereClauses));
                }
            }

            return connection.QuerySingleOrDefault<T>(queryBuilder.ToString(), keyValues, transaction);
        }

        public static IEnumerable<T> SelectListByPlayerId<T>(this IDbConnection connection, ulong playerId, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryParam = GetQueryParameter<T>();
            var whereClause = GetWhereClause<T>();

            string selectSql = $@"SELECT * FROM {tableName} WHERE PlayerId = @PlayerId;";

            return connection.Query<T>(selectSql, new {PlayerId = playerId }, transaction);
        }

        public static IEnumerable<T> SelectListByConditions<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryBuilder = new StringBuilder();

            // 기본 쿼리
            queryBuilder.Append($"SELECT * FROM {tableName}");

            // 조건이 있을 경우 WHERE 절 추가
            if (keyValues != null)
            {
                var whereClauses = new List<string>();
                var properties = keyValues.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var key = property.Name;
                    var value = property.GetValue(keyValues);

                    // 조건이 null이 아니면 WHERE 절에 추가
                    if (value != null)
                    {
                        if (value is IList valueList && valueList.Count > 0)
                        {
                            // IN 절 처리
                            var inClause = string.Join(", ", valueList.Cast<object>().Select((v, i) => $"@{key}_{i}"));
                            whereClauses.Add($"{key} IN ({inClause})");
                        }
                        else
                        {
                            whereClauses.Add($"{key} = @{key}");
                        }
                    }
                }

                if (whereClauses.Any())
                {
                    queryBuilder.Append(" WHERE ");
                    queryBuilder.Append(string.Join(" AND ", whereClauses));
                }
            }

            string selectSql = queryBuilder.ToString();

            // Dapper 실행
            return connection.Query<T>(selectSql, keyValues, transaction);
        }

        private static void SetPKWhereClause<T>(params string[] keyFields)
        {
            var tableName = GetTableName<T>();

            if (keyFields == null || keyFields.Length == 0)
                throw new ArgumentException($"ZERO_KEY_FILED Name({tableName})");

            var whereClause = string.Join(" AND ", keyFields.Select(k => $"`{k}` = @{k}"));
            s_pkWhereClauseDict[typeof(T)] = whereClause;
        }

        private static void SetQueryParameter<T>(params string[] keyFields)
        {
            var type = typeof(T);
            var tableName = GetTableName<T>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var fields = string.Join(", ", properties.Select(p => $"`{p.Name}`"));

            var parameters = string.Join(", ", properties.Select(p => "@" + p.Name));

            var updateSet = string.Join(", ", properties
                .Where(p => !keyFields.Contains(p.Name))
                .Select(p => $"`{p.Name}` = @{p.Name}"));

            var queryParam = new QueryParam(tableName, fields, parameters, updateSet);

            // 캐시된 필드와 파라미터 정보 가져오기 또는 새로 생성
            s_queryParamDict[typeof(T)] = queryParam;
        }

        private static string GetWhereClause<T>()
        {
            var tableName = GetTableName<T>();
            if (!s_pkWhereClauseDict.TryGetValue(typeof(T), out var outWhereClause))
            {
                throw new GameException($"NOT_FOUND_WHERE_CLAUSE", new { TableName = tableName });
            }

            return outWhereClause;
        }

        private static QueryParam GetQueryParameter<T>()
        {
            var tableName = GetTableName<T>();
            if (!s_queryParamDict.TryGetValue(typeof(T), out var outQueryParam))
            {
                throw new GameException($"NOT_FOUND_QUERY_PARAM", new { TableName = tableName });
            }
           
            return outQueryParam;
        }

        private static string GetTableName<T>()
        {
            var typeName = typeof(T).Name;
            if (!s_modelNameDict.TryGetValue(typeof(T), out var name))
            {
                throw new GameException($"NOT_FOUND_QUERY_PARAM", new { TableName = typeName });
            }

            return name;
        }

        private class QueryParam
        {
            public string TableName { get; private set; }
            public string Fields { get; private set; }
            public string Parameters { get; private set; }
            public string UpdateSet { get; private set; }

            public QueryParam(string tableName, string fields, string parameters, string updateSet)
            {
                TableName = tableName;
                Fields = fields;
                Parameters = parameters;
                UpdateSet = updateSet;
            }
        }
    }
}
