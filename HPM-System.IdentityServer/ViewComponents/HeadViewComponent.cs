using Microsoft.AspNetCore.Mvc;

namespace HPM_System.IdentityServer.ViewComponents
{
    public class HeadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Views/Shared/Components/Head/Index.cshtml");
        }
    }
}
