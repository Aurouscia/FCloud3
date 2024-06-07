using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Sys
{
    public class LagacyCompatController : Controller
    {
        [Route("/w")]
        public IActionResult WikiHomePage() => Redirect("/");
        [Route("/w/{id}")]
        public IActionResult Wiki(string id) => Redirect($"/#/w/{id}");
        [Route("/Wiki/ViewDir")]
        [Route("/FileStorage/ViewDir")]
        [Route("/FileStorage")]
        public IActionResult Dir() => Redirect($"/#/d");
    }
}