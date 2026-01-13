using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public abstract class BaseAdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

          
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var controllerName = context.ActionDescriptor.RouteValues["controller"];

            if (controllerName == "Account" && (actionName == "Login" || actionName == "Logout"))
            {
                return;
            }

           
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "Admin", returnUrl = context.HttpContext.Request.Path });
                return;
            }

           
            var roleName = HttpContext.Session.GetString("RoleName");
            var isAdmin = !string.IsNullOrEmpty(roleName) && 
                         (roleName.ToLower() == "admin" || roleName.ToLower() == "administrator");

            if (!isAdmin)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Bạn không có quyền truy cập trang quản trị";
                context.Result = RedirectToAction("Login", "Account", new { area = "Admin" });
                return;
            }
        }

        protected bool IsAdmin()
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            return !string.IsNullOrEmpty(roleName) && 
                   (roleName.ToLower() == "admin" || roleName.ToLower() == "administrator");
        }
    }
}









