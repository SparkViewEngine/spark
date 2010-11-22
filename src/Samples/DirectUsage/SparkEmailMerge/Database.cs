using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkEmailMerge {
	public class Database {
		public static IEnumerable<OrderDto> RetrieveOrders() {
			yield return order1;
			yield return order2;
			yield return order3;
		}

		private static OrderDto order1 = new OrderDto() {
			CustomerName = "Billy Gibbons",
			CustomerEmail = "the_reverend@zztop.example",
			OrderDate = new DateTime(1987, 8, 5),
			OrderReference = "ZZTOP956",
			Lines = new List<OrderLineDto>() { 
				new OrderLineDto() { ProductName = "Beard Shampoo", Quantity = 10, UnitPrice = 12.00m },
				new OrderLineDto() { ProductName = "Cheap Sunglasses", Quantity = 10, UnitPrice = 0.25m },
				new OrderLineDto() { ProductName = "Cadillac Eldorado", Quantity = 2, UnitPrice = 25000m },
				new OrderLineDto() { ProductName = "Chilli Bean Burrito", Quantity = 5, UnitPrice = 3.99m }
			}
		};

		private static OrderDto order2 = new OrderDto() {
			CustomerEmail = "eddie@vanhalen.example",
			CustomerName = "Edward Van Halen",
			OrderDate = new DateTime(1977, 6, 2),
			OrderReference = "EVH00123",
			Lines = new List<OrderLineDto>() { 
				new OrderLineDto() { ProductName = "Kramer Superstrat Guitar", Quantity = 1, UnitPrice = 499.00m },
				new OrderLineDto() { ProductName = "Marshall JCM800 Head", Quantity = 1, UnitPrice = 1495.00m },
				new OrderLineDto() { ProductName = "Marshall 4x12 Cabinet", Quantity = 2, UnitPrice = 1200.00m },
				new OrderLineDto() { ProductName = "M&M Candy", Quantity = 25, UnitPrice = 0.49m }
			}
		};

		private static OrderDto order3 = new OrderDto() {
			CustomerName = "Slash",
			CustomerEmail = "slash@gnr.example",
			OrderDate = new DateTime(1988, 05, 02),
			OrderReference = "GNR00456",
			Lines = new List<OrderLineDto>() {
				new OrderLineDto() { ProductName = "Gibson Les Paul 1959", Quantity = 1, UnitPrice = 2599.00m },
				new OrderLineDto() { ProductName = "Top Hat", Quantity = 1, UnitPrice = 59.99m },
				new OrderLineDto() { ProductName = "Silver Medal Hatband", Quantity = 1, UnitPrice = 12.99m },
				new OrderLineDto() { ProductName = "Bottle of Jack Daniels Whisky", Quantity = 25, UnitPrice = 22.99m }
			}
		};
	}
}
