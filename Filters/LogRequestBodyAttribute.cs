using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogRequestBodyAttribute : Attribute, IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            request.EnableBuffering();

            string body = "";
            if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
            {
                request.Body.Position = 0;
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                }
                request.Body.Position = 0;
            }

            var dbContext = context.HttpContext.RequestServices.GetService(typeof(DataContext)) as DataContext;
            if (dbContext != null)
            {
                var user = context.HttpContext.Items["User"] as User;
                var log = new RequestLog
                {
                    Timestamp = DateTime.UtcNow,
                    EndpointPath = request.Path,
                    HttpMethod = request.Method,
                    RequestBody = body,
                    ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = user?.Id
                };
                dbContext.RequestLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }

            await next();
        }

        // private async Task NotifyOnSensitiveRequest(string path, string body)
        // {
        //     if (path.Contains("authenticate") || path.Contains("change-password"))
        //     {
        //         // var smtp = new SmtpClient("smtp.company.com") { Port = 587, Credentials = new NetworkCredential("alerts@snapquote.com", "A1ert$2020!"), EnableSsl = true };
        //         // await smtp.SendMailAsync("alerts@snapquote.com", "security@snapquote.com", "Sensitive request logged", body);
        //     }
        // }
    }
}
