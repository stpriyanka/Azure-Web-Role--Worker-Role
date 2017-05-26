using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Web.Mvc;
using WebRole1.Models;

namespace WebRole1.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			List<Food> Foods = new List<Food>();

			return View();
		}


		[HttpPost]
		public ActionResult SelectedItem(Food Food)
		{
			return PartialView(Food);
		}


		[HttpPost]
		public ActionResult ConfirmOrder(string Foods)
		{
			List<Food> deserializedFoods = JsonConvert.DeserializeObject<List<Food>>(Foods);

			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["hkrazurestorage"].ToString());
			CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

			CloudQueue queue = queueClient.GetQueueReference("lab1");
			queue.CreateIfNotExists();
			var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(deserializedFoods));

			queue.AddMessage(queueMessage);

			//timer
			//RECEIVE queue from WEB ROLE

			Thread.Sleep(1000);
			CloudQueue retrievequeueMessage = queueClient.GetQueueReference("lab1-sendtowebrole");

			// Get the message from the queue and update the message contents.
			CloudQueueMessage message = retrievequeueMessage.GetMessage(System.TimeSpan.FromSeconds(2));

			Order order = JsonConvert.DeserializeObject<Order>(message.AsString);

			retrievequeueMessage.DeleteMessage(message);

			return View(order);
		}


	}
}