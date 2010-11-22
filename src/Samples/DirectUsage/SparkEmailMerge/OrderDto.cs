using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkEmailMerge {
	public class OrderLineDto {
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
	}
	public class OrderDto {
		public string CustomerEmail { get; set; }
		public string CustomerName { get; set; }
		public string OrderReference { get; set; }
		public DateTime OrderDate { get; set; }
		private IList<OrderLineDto> lines = new List<OrderLineDto>();
		public IList<OrderLineDto> Lines { get { return lines; } set { lines = value; } }
	}
}
