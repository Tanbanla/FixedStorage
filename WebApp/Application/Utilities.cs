using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace WebApp.Application
{
    public static class Utilities
    {


        public static ActionResult RedirectPreventAccessResult()
        {
            return new RedirectToActionResult("PreventAccessPage", "Home", null);
        }

        public static string RemoveNewLineAndCarriageReturn(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return Regex.Replace(input.Trim(), "[\r\n\v]", "");
        }

    }
    public static class HtmlExtensions
    {
        public static string Translate(this IHtmlHelper helper, string key)
        {
            IServiceProvider services = helper.ViewContext.HttpContext.RequestServices;
            SharedViewLocalizer localizer = services.GetRequiredService<SharedViewLocalizer>();
            string result = localizer[key];
            return result;
        }

        private static readonly HtmlContentBuilder _emptyBuilder = new HtmlContentBuilder();

        public static IHtmlContent BuildBreadcrumbNavigation(this IHtmlHelper helper)
        {
            if (helper.ViewContext.RouteData.Values["controller"].ToString() == "Home" ||
                helper.ViewContext.RouteData.Values["controller"].ToString() == "Account")
            {
                return _emptyBuilder;
            }

            var controllerName = helper.ViewContext.RouteData.Values["controller"].ToString();
            var actionName = helper.ViewContext.RouteData.Values["action"].ToString();
            var addValue = helper.ViewContext.ActionDescriptor.Properties.FirstOrDefault(x => x.Key.ToString() == "RouteDisplayValue");
            IEnumerable<string> displayValue = null;
            if (addValue.Value != null)
            {
                displayValue = addValue.Value as IEnumerable<string>;
            }
            else
            {
                displayValue = displayValue.Append(controllerName);
            }
            var breadcrumb = new HtmlContentBuilder()
                                .AppendHtml("<ol class='breadcrumb bold-text' style=\"padding:1rem 1rem;margin:0;align-items: center;\"><li>")
                                .AppendHtml("<span><a style=\"color:#6A707E;\" href='/'>" + helper.Translate("Trang chủ") + "</a></span></li>");

            if (controllerName != "InventoryReporting")
            {
                breadcrumb = breadcrumb.AppendHtml("<li><image src='./assets/images/table_icons/arrow-right.svg' width=\"24px\" height=\"24px\" alt='arrow>'/></li>");
            }

            //.AppendHtml("</li><li >")
            //.AppendHtml(helper.ActionLink(displayValue,
            //                          actionName, controllerName))
            //.AppendHtml("</li>")

            if (displayValue != null && displayValue.Count() == 1)
            {
                if (controllerName != "InventoryReporting")
                {
                    breadcrumb.AppendHtml("<li >")
                        .AppendHtml(helper.ActionLink(helper.Translate(displayValue.ElementAt(0)), actionName, controllerName))
                        .AppendHtml("</li>");
                }



                //if (helper.ViewContext.RouteData.Values["action"].ToString() != "Index" && string.IsNullOrEmpty(displayValue))
                //{
                //    breadcrumb.AppendHtml("<li>")
                //              .AppendHtml(helper.ActionLink(actionName, actionName, controllerName))
                //              .AppendHtml("</li>");
                //}


            }
            else if (displayValue != null && displayValue.Count() > 1)
            {
                for (var i = 0; i < displayValue.Count(); i++)
                {
                    if (i < displayValue.Count() - 1)
                    {
                        breadcrumb.AppendHtml("<li ><span>")
                                    .AppendHtml(helper.ActionLink(helper.Translate(displayValue.ElementAt(i)), actionName, controllerName, new { style = "color:#6A707E;" }))
                                    .AppendHtml("</span></li>")
                                    .AppendHtml("<li><image src='./assets/images/table_icons/arrow-right.svg' width=\"24px\" height=\"24px\" alt='arrow>'/></li>");
                    }
                    else
                    {
                        breadcrumb.AppendHtml("<li >")
                                 .AppendHtml(helper.ActionLink(helper.Translate(displayValue.ElementAt(i)), actionName, controllerName))
                                 .AppendHtml("</li>");
                    }

                }
            }

            return breadcrumb.AppendHtml("</ol>");
        }
    }

    public class SharedViewLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public SharedViewLocalizer(IStringLocalizerFactory factory)
        {
            var type = typeof(Program);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("Resource", assemblyName.Name);
        }

        public LocalizedString this[string key] => _localizer[key];

        public LocalizedString GetLocalizedString(string key)
        {
            return _localizer[key];
        }
    }
}
