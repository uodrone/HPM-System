using Microsoft.AspNetCore.Mvc;

namespace HPM_System.IdentityServer.ViewComponents
{
    public class CSSConnectionViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Views/Shared/Components/CSSConnection/Index.cshtml");
        }
    }
}
