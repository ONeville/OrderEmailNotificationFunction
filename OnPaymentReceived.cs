using System.Xml;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace OrderEmailNotification
{
    public static class OnPaymentReceived
    {
        [FunctionName("OnPaymentReceived")]
       public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, 
            [Queue("orders")] IAsyncCollector<Order> orderQueue,
            [Table("orders")] IAsyncCollector<OrderItem> orderTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);
            await orderQueue.AddAsync(order);
            
            var orderSerialized = JsonConvert.SerializeObject(order);
            var orderItem = JsonConvert.DeserializeObject<OrderItem>(orderSerialized);
            orderItem.PartitionKey = "orders";
            orderItem.RowKey = order.OrderId;
            await orderTable.AddAsync(orderItem);
            
            log.LogInformation($"Order PartitionKey:({orderItem.PartitionKey}) {orderItem.OrderId} received from {orderItem.Email} for product {orderItem.ProductId}");
            var orders = new List<Order>();
            orders.Add(order);
            return (ActionResult)new OkObjectResult(orders);
        }
    }
}
