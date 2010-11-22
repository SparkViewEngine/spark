using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;

namespace SparkEmailMerge {
	public class SmtpMailSender {
		private SmtpClient smtp;

		public void SendMail(string from, string to, string subject, string textBody, string htmlBody) 
		{
			
			MailMessage message = new MailMessage(from, to);
			// message.Bcc.Add("rosemary@spotlight.com");

			message.Subject = subject;
			message.IsBodyHtml = false;

			AlternateView textBodyView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.ASCII, "text/plain");
			textBodyView.TransferEncoding = TransferEncoding.SevenBit;
			message.AlternateViews.Add(textBodyView);

			var htmlBodyView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
			htmlBodyView.TransferEncoding = TransferEncoding.Base64;
			message.AlternateViews.Add(htmlBodyView);
			smtp.Send(message);
		}
		
		public SmtpMailSender(string pickupDirectoryLocation) 
		{
			this.smtp = new SmtpClient();
			this.smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			this.smtp.PickupDirectoryLocation = pickupDirectoryLocation;
		}
	}
}
