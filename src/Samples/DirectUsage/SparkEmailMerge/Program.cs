using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark;
using System.Net.Mail;
using System.Reflection;
using System.IO;

namespace SparkEmailMerge {
	class Program {

		private static string GetLocalDirectory() {
			var localPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
			return (Path.GetDirectoryName(localPath));
		}

		static void Main(string[] args) {
			const string htmlBodyTemplate = "thankyou_html.spark";
			const string textBodyTemplate = "thankyou_text.spark";
			var templateFolder = Path.Combine(GetLocalDirectory(), "Templates");
			var mailDropDirectory = Path.Combine(GetLocalDirectory(), "MailDrop");
			if (!Directory.Exists(mailDropDirectory)) Directory.CreateDirectory(mailDropDirectory);
			var smtp = new SmtpMailSender(mailDropDirectory);
			var templater = new Templater(templateFolder);
			
			var mailCount = 0;

			foreach (var order in Database.RetrieveOrders()) {
				var htmlBody = templater.Populate(htmlBodyTemplate, order);
				var textBody = templater.Populate(textBodyTemplate, order);
				smtp.SendMail("orders@sparkstore.example", order.CustomerEmail, "Your Order from SparkStore", textBody, htmlBody);
				mailCount++;
			}
			Console.WriteLine("Generated {0} e-mails in drop folder {1}", mailCount, mailDropDirectory);
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey(false);
		}
	}
}
