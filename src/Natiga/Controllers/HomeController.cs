using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Natiga.Models;
using Natiga.Services;

namespace Natiga.Controllers
{
    public class HomeController(
        ILogger<HomeController> logger,
        ResultService resultService,
        IWebHostEnvironment env,
        EmailService emailService,
        IConfiguration configuration) : Controller
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

            if (result != null)
            {
                try
                {
                    string body = $"<h1>‰ ÌÃ… «·ÿ«·» {result.Name}</h1>" +
                                  $"<p>—ﬁ„ «·Ã·Ê”: {result.SeatNo}</p>" +
                                  "<table border='1'>" +
                                  "<tr><th>«·„«œ…</th><th>«·œ—Ã…</th></tr>" +
                                  string.Join("",
                                      result.Marks.Where(q => !string.IsNullOrWhiteSpace(q.Value)).Select(m => $"<tr><td>{m.Key}</td><td>{m.Value}</td></tr>")) +
                                  "</table>";

                    await emailService.SendEmailAsync(configuration.GetSection("Smtp")["To"],
                        $"ÿ·» ‰ ÌÃ…: ({result.SeatNo}) | [{result.Name}]",
                        body);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
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
