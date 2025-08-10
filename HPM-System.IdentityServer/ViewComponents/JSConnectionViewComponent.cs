using Microsoft.AspNetCore.Mvc;

namespace HPM_System.IdentityServer.ViewComponents
{
    public class JSConnectionViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Views/Shared/Components/JSConnection/Index.cshtml");
        }
    }
}
