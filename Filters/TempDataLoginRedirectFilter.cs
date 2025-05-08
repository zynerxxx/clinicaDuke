using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace clinicaDukeDB.Filters
{
    public class TempDataLoginRedirectFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                var tempDataFactory = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
                var tempData = tempDataFactory?.GetTempData(context.HttpContext);
                if (tempData != null)
                {
                    tempData["LoginError"] = "¡Inicia sesión primero!";
                }
            }
        }
    }
}
