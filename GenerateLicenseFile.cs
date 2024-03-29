using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace OrderEmailNotification
{
    public static class GenerateLicenseFile
    {
        [FunctionName("GenerateLicenseFile")]
        public static async Task Run(
            [QueueTrigger("orders", Connection = "AzureWebJobsStorage")]Order order,
            IBinder binder, ILogger log)
        {
            var outputBlob = await binder.BindAsync<TextWriter>(new BlobAttribute($"licenses/{order.OrderId}.lic")
            {
                Connection = "AzureWebJobsStorage"
            });

            outputBlob.WriteLine(($"OrderId:  {order.OrderId}"));
            outputBlob.WriteLine(($"Email: {order.Email}"));
            outputBlob.WriteLine(($"ProductId: {order.ProductId}"));
            outputBlob.WriteLine(($"PurchasedDate: {DateTime.UtcNow}"));
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes((order.Email + "secret")));
            outputBlob.WriteLine($"SecretCode: {BitConverter.ToString((hash)).Replace("-", "")}");

            log.LogInformation($"C# Queue trigger function processed: {order.Email}");
        }
    }
} 
