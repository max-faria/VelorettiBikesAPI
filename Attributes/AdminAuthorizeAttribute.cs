using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VelorettiAPI.Attributes
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return; 
            }

            var isADminClaim = user.Claims.FirstOrDefault(c => c.Type == "IsAdmin");
            if(isADminClaim == null || !bool.Parse(isADminClaim.Value))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}