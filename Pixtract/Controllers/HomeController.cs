using Microsoft.AspNetCore.Mvc;

namespace Pixtract.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
