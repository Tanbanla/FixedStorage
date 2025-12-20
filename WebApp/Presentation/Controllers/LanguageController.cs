using Irony;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Resources;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections;

namespace WebApp.Presentation.Controllers
{
    public class LanguageController : ControllerBase
    {
        [AllowAnonymous]
        public  IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [AllowAnonymous]
        public IActionResult GetAllTranslatedKeys()
        {
            var cultures = new List<string> { "vi-VN", "en-US" };
            var resourceManager = new ResourceManager("WebApp.Resources.Resource", typeof(LanguageController).Assembly);
            var translatedKeys = new Dictionary<string, Dictionary<string, string>>();

            foreach (var culture in cultures)
            {
                var cultureInfo = new CultureInfo(culture);
                var resourceSet = resourceManager.GetResourceSet(cultureInfo, true, true);
                var keyValues = resourceSet.Cast<DictionaryEntry>()
                .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.ToString());

                translatedKeys.Add(culture, keyValues);
            }

            return Ok(translatedKeys);
        }



    }
}
