using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Http;
using System.Web.UI;
using EventStore.ClientAPI;

namespace WebApplication3.Controllers
{
    [RoutePrefix("finance/payments-paypal/v1")]
    public class DefaultController : ApiController
    {
        [Route("{paymentReference}")]
        public IHttpActionResult Put([FromUri]string paymentReference, [FromBody] PayPalPaymentIntent intent)
        {
            var connection =
            EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("52.164.224.165"), 1113));

            // Don't forget to tell the connection to connect!
            connection.ConnectAsync().Wait();

            

            var myEvent = new EventData(Guid.NewGuid(), "PayPalPaymentIntentAdded", false,
                                        ObjectToByteArray(intent),
                                        Encoding.UTF8.GetBytes("some metadata"));

            connection.AppendToStreamAsync("PayPalStream",
                                           ExpectedVersion.Any, myEvent).Wait();

            var streamEvents =
                connection.ReadStreamEventsForwardAsync("PayPalStream", 0, 1, false).Result;

            var returnedEvent = streamEvents.Events[0].Event;

            Console.WriteLine("Read event with data: {0}, metadata: {1}",
                Encoding.UTF8.GetString(returnedEvent.Data),
                Encoding.UTF8.GetString(returnedEvent.Metadata));

            return Ok();
        }
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }

    [Serializable]
    public class PayPalPaymentIntent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Currency { get; set; }

        public Item[] Items { get; set; }


    }

    public class Item
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}
