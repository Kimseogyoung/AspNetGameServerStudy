using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Server.Helper
{
    public static class OpenApiHelper
    {
        public static List<OpenApiParameter> CreateParameters(Type inType)
        {
            var paramList = new List<OpenApiParameter>();
            var typeProps = inType.GetProperties();
            foreach (var property in typeProps)
            {
                var openApiParam = new OpenApiParameter
                {
                    Name = property.Name,
                    In = ParameterLocation.Query,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = property.PropertyType.Name
                    }
                };
                paramList.Add(openApiParam);
            }

            return paramList;
        }

        public static OpenApiRequestBody CreateRequestBody(Type inType)
        {
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3187 버그 Swashbuckle.AspNetCore 7.2.0 해결 아오 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            var openApiRequestBody = new OpenApiRequestBody(new OpenApiRequestBody
            {
                Required = true,
                Description = "",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>()
                        }
                    }
                }
            });

            //// Iterate through properties and add them to the request body schema
            var typeProps = inType.GetProperties();
            var schema = openApiRequestBody.Content["application/json"].Schema;

            foreach (var property in typeProps)
            {
                schema.Properties[property.Name] = new OpenApiSchema
                {
                    Type = GetOpenApiType(property.PropertyType, out var innerSchema)
                };

                // 만약 property가 복잡한 객체일 경우 그 내부 속성들을 처리
                if (innerSchema != null)
                {
                    schema.Properties[property.Name].Properties = innerSchema.Properties;
                }
            }

            return openApiRequestBody;
        }

        public static OpenApiResponses CreateResponse(Type inType)
        {
            var res =  new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = inType.Name, 

                            }
                        }
                    }
                }
            };

            var typeProps = inType.GetProperties();
            var schema = res["200"].Content["application/json"].Schema;

            foreach (var property in typeProps)
            {
                schema.Properties[property.Name] = new OpenApiSchema
                {
                    Type = property.PropertyType.Name
                };
            }

            return res;
        }

        private static string GetOpenApiType(Type propertyType, out OpenApiSchema innerSchema)
        {
            innerSchema = null;

            if (propertyType == typeof(string) || propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
                return "string";
            if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(short) || propertyType == typeof(ulong) || propertyType == typeof(ushort))
                return "integer";
            if (propertyType == typeof(double) || propertyType == typeof(float))
                return "number";
            if (propertyType == typeof(bool))
                return "boolean";
            if (propertyType.IsEnum)
                return "integer";
            if (propertyType.IsClass && propertyType != typeof(string))
            {
                // 만약 propertyType이 클래스라면, 내부 속성들을 처리하기 위해 재귀 호출
                innerSchema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = GetPropertiesForComplexType(propertyType)
                };
                return "object";
            }

            return "string"; 
        }

        // Class 타입인 경우 재귀 호출
        private static Dictionary<string, OpenApiSchema> GetPropertiesForComplexType(Type type)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var typeProps = type.GetProperties();

            foreach (var property in typeProps)
            {
                properties[property.Name] = new OpenApiSchema
                {
                    Type = GetOpenApiType(property.PropertyType, out var innerSchema)
                };

                // 내부 속성이 있다면 추가
                if (innerSchema != null)
                {
                    properties[property.Name].Properties = innerSchema.Properties;
                }
            }

            return properties;
        }
    }

}
