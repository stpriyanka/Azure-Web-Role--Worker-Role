using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WorkerRole1
{
	public class WorkerRole : RoleEntryPoint
	{
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

		public object CloudConfigurationManager { get; private set; }


		private double _smallCheese = 20;
		private double _smallHam = 25;
		private double _smallFish = 15;
		private double _smallVeg = 0;

		private double _mediumCheese = 20 * (.5);
		private double _mediumHam = 25 * (.5);
		private double _mediumFish = 15 * (.5);
		private double _mediumVeg = 0 * (.5);

		private double _largeCheese = 20 * 2;
		private double _largeHam = 25 * 2;
		private double _largeFish = 15 * 2;
		private double _largeVeg = 0 * 2;




		public override void Run()
		{
			Trace.TraceInformation("WorkerRole1 is running");

			try
			{
				this.RunAsync(this.cancellationTokenSource.Token).Wait();
			}
			finally
			{
				this.runCompleteEvent.Set();
			}
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections
			ServicePointManager.DefaultConnectionLimit = 12;

			// For information on handling configuration changes
			// see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

			bool result = base.OnStart();

			Trace.TraceInformation("WorkerRole1 has been started");
			//LOGIC
	

			return result;
		}

		public override void OnStop()
		{
			Trace.TraceInformation("WorkerRole1 is stopping");

			this.cancellationTokenSource.Cancel();
			this.runCompleteEvent.WaitOne();

			base.OnStop();

			Trace.TraceInformation("WorkerRole1 has stopped");
		}

		private async Task RunAsync(CancellationToken cancellationToken)
		{
			// TODO: Replace the following with your own logic.
			//while (!cancellationToken.IsCancellationRequested)
			//{
			//	Trace.TraceInformation("Working");
			//	await Task.Delay(1000);
			//}
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["hkrazurestorage"].ToString());
			CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

			CloudQueue queue = queueClient.GetQueueReference("lab1");
			// Get the message from the queue 
			CloudQueueMessage message = queue.GetMessage();

			if (true)
			{
				List<Food> foods = (List<Food>)JsonConvert.DeserializeObject(message.AsString, typeof(List<Food>));

				double totalPrice = 0;
				uint burgerCount = 0;

				foreach (Food food in foods)
				{
					burgerCount += food.Quantity;

					//CHEESE
					if (food.Name == "Cheese Burger")
					{
						if (food.Size == "Small")
						{
							totalPrice += _smallCheese * food.Quantity;
						}

						else if (food.Size == "Medium")
						{
							totalPrice += _mediumCheese * food.Quantity;

						}
						else if (food.Size == "Large")
						{
							totalPrice += _largeCheese * food.Quantity;

						}
					}

					//HAM
					else if (food.Name == "Ham Burger")
					{
						if (food.Size == "Small")
						{
							totalPrice += _smallHam * food.Quantity;
						}
						else if (food.Size == "Medium")
						{
							totalPrice += _mediumHam * food.Quantity;
						}
						else if (food.Size == "Large")
						{
							totalPrice += _largeHam * food.Quantity;
						}
					}

					//FISH
					else if (food.Name == "Fish Burger")
					{

						if (food.Size == "Small")
						{
							totalPrice += _smallFish * food.Quantity;
						}
						else if (food.Size == "Medium")
						{
							totalPrice += _mediumFish * food.Quantity;
						}
						else if (food.Size == "Large")
						{
							totalPrice += _largeFish * food.Quantity;
						}
					}

					//VEG
					else if (food.Name == "Vegetable Burger")
					{
						if (food.Size == "Small")
						{
							totalPrice += _smallVeg * food.Quantity;
						}

						else if (food.Size == "Medium")
						{
							totalPrice += _mediumVeg * food.Quantity;
						}
						else if (food.Size == "Large")
						{
							totalPrice += _largeVeg * food.Quantity;
						}
					}

					//add frend fry price
					if (food.IsFrenchFry)
					{
						totalPrice += 12;
					}
				}
				//including 17% tax
				totalPrice += totalPrice * .17;

				//including 8% discount
				if (burgerCount > 10)
				{
					totalPrice = totalPrice - (totalPrice * .08);
				}

				string s = totalPrice.ToString();
				Trace.TraceInformation(s);

				queue.DeleteMessage(message);


				//Send queue back to WEB ROLE
				CloudQueue sendMsgToqueue = queueClient.GetQueueReference("lab1-sendtowebrole");
				sendMsgToqueue.CreateIfNotExists();

				Order o = new Order()
				{
					Foods = foods,
					TotalPrice = totalPrice
				};

				var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(o));

				sendMsgToqueue.AddMessage(queueMessage);

			}
		}
	}
	public class Food
	{
		public string Name { get; set; }

		public string Size { get; set; }

		public int Price { get; set; }

		public uint Quantity { get; set; }

		public bool IsFrenchFry { get; set; }


	}

	public class Order
	{
		public double TotalPrice { get; set; }

		public List<Food> Foods { get; set; }
	}

}
