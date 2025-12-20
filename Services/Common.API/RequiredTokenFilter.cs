using Swashbuckle.AspNetCore.SwaggerGen;

namespace BIVN.FixedStorage.Services.Common.API
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AddHeaderParameterAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Required { get; set; }
    }

    public class AddHeaderParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var addHeaders = context.MethodInfo.GetCustomAttributes(typeof(AddHeaderParameterAttribute), true).Cast<AddHeaderParameterAttribute>();
            foreach (var header in addHeaders)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = "string" },
                    Name = header.Name,
                    Required = header.Required
                });
            }
        }
    }
}
