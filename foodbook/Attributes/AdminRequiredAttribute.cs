using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using foodbook.Helpers;

namespace foodbook.Attributes
{
    public class AdminRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            if (!session.IsLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            if (!session.IsAdmin())
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}
