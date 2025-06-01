using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Natiga.Models;
using Natiga.Services;

namespace Natiga.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ResultService resultService, IWebHostEnvironment env) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost("/get-result/{seatNo}")]
        public async Task<IActionResult> GetResult(string seatNo)
        {
            StudentResultVM? result = default;

            var xlsFilesPath = Path.Combine(env.WebRootPath, "results");

            foreach (var file in Directory.GetFiles(xlsFilesPath, "*.xlsx"))
            {
                result = await resultService.SearchBySeatNo(seatNo.ToString(),file);
                if (result != null)
                {
                    break;
                }
            }

            return PartialView("_ResultPartial", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
