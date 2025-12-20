namespace WebApp.Application.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RouteDisplayValueAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] displayValues;
        /// <summary>
        /// Passing route display value withrelationship by index base
        /// </summary>
        /// <param name="values">The value before comma will be parent of the value after comma</param>
        public RouteDisplayValueAttribute(params string[] values)
        {
            displayValues = values;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var addtionalRouteValue = new KeyValuePair<object, object>("RouteDisplayValue", displayValues.AsEnumerable());
            var existed = context.ActionDescriptor.Properties.Any(p => p.Key == addtionalRouteValue.Key);
            if (!existed)
            {
                context.ActionDescriptor.Properties.Add(addtionalRouteValue);
            }

            return;
        }
    }
}
