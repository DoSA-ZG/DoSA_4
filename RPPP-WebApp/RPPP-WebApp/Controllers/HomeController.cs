using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
        public IActionResult HarvestReport()
        {
            return View();
        }
    }
}
