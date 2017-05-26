using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebRole1.Models
{
	public class Food
	{
		[Required]
		public string Name { get; set; }

		[Required]
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