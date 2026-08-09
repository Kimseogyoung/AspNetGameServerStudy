using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Proto;
using ServerCore;

namespace ServerCore.Extension
{
    public static class DapperExtension
    {
        private static readonly ConcurrentDictionary<Type, string> ModelNameDict = new();
        private static readonly ConcurrentDictionary<Type, QueryParam> QueryParamDict = new();
        private static readonly ConcurrentDictionary<Type, string> PkWhereClauseDict = new();

        // 조건 타입 기준 SQL 캐시 — GetProperties() + SQL 빌딩을 최초 1회만 실행
        private static readonly ConcurrentDictionary<(Type entity, Type cond), string> CondSingleSqlCache = new();
        private static readonly ConcurrentDictionary<(Type entity, Type cond), string> CondListSqlCache = new();

        // Insert 시 Id 자동증가 여부 판단용 PropertyInfo 캐시
        private static readonly ConcurrentDictionary<Type, PropertyInfo> IdPropCache = new();

        // 여러 필드를 기본 키로 설정하는 메서드


        public static void Init<T>(params string[] keyFields)
        {
            var type = typeof(T);
            var tableName = type.Name;
            if (tableName.EndsWith("Model"))
            {
                tableName = tableName[..^5];
            }
            ModelNameDict[type] = tableName;
            SetPKWhereClause<T>(keyFields);
            SetQueryParameter<T>(keyFields);
        }

        public static async Task<T> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction)
        {
            var queryParam = GetQueryParameter<T>();

            var tableName = GetTableName<T>();
            // `Id` 속성 존재 여부 확인 (PropertyInfo 캐싱)
            var idProp = IdPropCache.GetOrAdd(typeof(T), t => t.GetProperty("Id"));
            var hasAutoIncreaseProperty = tableName != "Player" && idProp != null;

            var insertSql = $@"
                INSERT INTO {queryParam.TableName} ({queryParam.Fields})
                VALUES ({queryParam.Parameters});";

            // Id가 있는 경우 추가적으로 SELECT 실행
            if (hasAutoIncreaseProperty)
            {
                insertSql += $@"
                SELECT * FROM {queryParam.TableName} WHERE Id = CONVERT(LAST_INSERT_ID(), UNSIGNED);";
                var mdl = await connection.QuerySingleOrDefaultAsync<T>(insertSql, entity, transaction);
                if (mdl == null)
                {
                    throw new GameException(EErrorCode.PARAM, "INSERT_FAIL", null);
                }
                return mdl;
            }
            else
            {
                // Id가 없으면 INSERT만 수행
                await connection.ExecuteAsync(insertSql, entity, transaction);
                return entity;
            }
        }

        public static async Task UpdateAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();
            var queryParam = GetQueryParameter<T>();
            var whereClause = GetWhereClause<T>();

            // Build UPDATE SQL
            var updateSql = $@"
            UPDATE {tableName}
            SET {queryParam.UpdateSet}
            WHERE {whereClause};";

            await connection.ExecuteAsync(updateSql, entity, transaction);
        }

        public static Task<T> SelectByPkAsync<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            var tableName = GetTableName<T>();

            _ = GetQueryParameter<T>();
            var whereClause = GetWhereClause<T>();

            var selectSql = $@"SELECT * FROM {tableName} WHERE {whereClause};";

            return connection.QuerySingleOrDefaultAsync<T>(selectSql, keyValues, transaction);
        }

        public static Task<T> SelectByConditionsAsync<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            if (keyValues == null)
            {
                return connection.QuerySingleOrDefaultAsync<T>(
                    $"SELECT * FROM {GetTableName<T>()}", transaction: transaction);
            }

            var sql = CondSingleSqlCache.GetOrAdd(
                (typeof(T), keyValues.GetType()),
                k =>
                {
                    var props = k.cond.GetProperties();
                    var where = string.Join(" AND ", props.Select(p => $"`{p.Name}` = @{p.Name}"));
                    return $"SELECT * FROM {GetTableName<T>()} WHERE {where}";
                });

            return connection.QuerySingleOrDefaultAsync<T>(sql, keyValues, transaction);
        }

        public static async Task<IEnumerable<T>> SelectListByConditionsAsync<T>(this IDbConnection connection, object keyValues, IDbTransaction transaction)
        {
            if (keyValues == null)
            {
                return await connection.QueryAsync<T>(
                    $"SELECT * FROM {GetTableName<T>()}", transaction: transaction);
            }

            var sql = CondListSqlCache.GetOrAdd(
                (typeof(T), keyValues.GetType()),
                k =>
                {
                    var props = k.cond.GetProperties();
                    var where = string.Join(" AND ", props.Select(p =>
                        typeof(IList).IsAssignableFrom(p.PropertyType)
                            ? $"`{p.Name}` IN @{p.Name}"   // Dapper가 IEnumerable 파라미터를 자동 확장
                            : $"`{p.Name}` = @{p.Name}"));
                    return $"SELECT * FROM {GetTableName<T>()} WHERE {where}";
                });

            return await connection.QueryAsync<T>(sql, keyValues, transaction);
        }

        private static void SetPKWhereClause<T>(params string[] keyFields)
        {
            var tableName = GetTableName<T>();

            if (keyFields == null || keyFields.Length == 0)
            {
                throw new ArgumentException($"ZERO_KEY_FILED Name({tableName})");
            }

            var whereClause = string.Join(" AND ", keyFields.Select(k => $"`{k}` = @{k}"));
            PkWhereClauseDict[typeof(T)] = whereClause;
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
            QueryParamDict[typeof(T)] = queryParam;
        }

        private static string GetWhereClause<T>()
        {
            var tableName = GetTableName<T>();
            if (!PkWhereClauseDict.TryGetValue(typeof(T), out var outWhereClause))
            {
                throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NOT_FOUND_WHERE_CLAUSE", new { TableName = tableName });
            }

            return outWhereClause;
        }

        private static QueryParam GetQueryParameter<T>()
        {
            var tableName = GetTableName<T>();
            if (!QueryParamDict.TryGetValue(typeof(T), out var outQueryParam))
            {
                throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NOT_FOUND_QUERY_PARAM", new { TableName = tableName });
            }

            return outQueryParam;
        }

        private static string GetTableName<T>()
        {
            var typeName = typeof(T).Name;
            if (!ModelNameDict.TryGetValue(typeof(T), out var name))
            {
                throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NOT_FOUND_QUERY_PARAM", new { TableName = typeName });
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
