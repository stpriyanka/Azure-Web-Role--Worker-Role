using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1.Controllers
{
	public class Result
	{

		public double GetTotalPice(string totalPrice)
		{
			double x = JsonConvert.DeserializeObject<double>(totalPrice);
			return x;
		}

	}
}