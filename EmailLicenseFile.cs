using System;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace OrderEmailNotification
{
    public static class EmailLicenseFile
    {
        [FunctionName("EmailLicenseFile")]
        public static void Run(
            [BlobTrigger("licenses/{orderId}.lic", Connection = "AzureWebJobsStorage")]string licenseFileContents, 
            [Table("orders", "orders", "{orderId}")] OrderItem order,
            [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage message, string orderId, ILogger log)
        {
            log.LogInformation($"Got order from {order.Email} Order ID: {orderId}");
            message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(order.Email);
            var plainText = Encoding.UTF8.GetBytes(licenseFileContents);
            var base64 = Convert.ToBase64String(plainText);
            message.AddAttachment($"{orderId}.lic", base64, "text/plain");
            message.Subject = "Your license file";
            message.HtmlContent = "Thank you for your order";
        }
    }
}
