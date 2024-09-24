using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class ErrorController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    [Route("Error/404")]
    public IActionResult PageNotFound()
    {
        return View();
    }

    public IActionResult Test()
    {
        throw new Exception();
    }
  }
}