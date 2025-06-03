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

        string GetClientIp(HttpContext context)
        {
            // Check for X-Forwarded-For header first (used when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // In case of multiple IPs, take the first one
                return forwardedFor.Split(',')[0];
            }

            // Fallback to remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

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
            var queriesCount = Request.Cookies["eql"] ?? "0";

            if (int.Parse(queriesCount) > 2)
            {
                var ip = GetClientIp(HttpContext);
                await emailService.SendEmailAsync(configuration.GetSection("Smtp")["To"],
                      $"Queries exceeded for IP: {ip} !", $"IP {ip} exceeded queries limit!");
                return BadRequest();
            }

            StudentResultVM? result = default;

            var xlsFilesPath = Path.Combine(env.WebRootPath, "results");

            foreach (var file in Directory.GetFiles(xlsFilesPath, "*.xlsx"))
            {
                result = await resultService.SearchBySeatNo(seatNo.ToString(), file);
                if (result != null)
                {
                    break;
                }
            }

            if (result != null)
            {
                var s1 = "‰ ÌÃ… «·ÿ«·»";
                var s2 = "—ﬁ„ «·Ã·Ê”";
                var s3 = "ÿ·» ‰ ÌÃ…";
                try
                {
                    string body = "<!DOCTYPE html>" +
                                   "<html lang='ar' dir='rtl'>" +
                                   "<head>" +
                                   "<meta charset='utf-8' />" +
                                   "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />" +
                                   "</head>" +
                                   "<body>" +
                                   $"<h1>{s1}: {result.Name}</h1>" +
                                   $"<p>{s2}: {result.SeatNo}</p>" +
                                   "<table border='1'>" +
                                   "<tr><th>«·„«œ…</th><th>«·œ—Ã…</th></tr>" +
                                   string.Join("",
                                       result.Marks
                                           .Where(q => !string.IsNullOrWhiteSpace(q.Value))
                                           .Select(m => $"<tr><td>{m.Key}</td><td>{m.Value}</td></tr>")) +
                                   "</table>" +
                                   "</body>" +
                                   "</html>";

                    Response.Cookies.Append("eql", (int.Parse(queriesCount) + 1).ToString(), new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    if (!env.IsDevelopment())
                    {
                        await emailService.SendEmailAsync(configuration.GetSection("Smtp")["To"],
                            $"{s3}: ({result.SeatNo}) | [{result.Name}]",
                            body);
                    }
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
