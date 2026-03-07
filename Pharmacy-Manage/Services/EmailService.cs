using MailKit.Net.Smtp;
using MimeKit;

namespace Pharmacy_Manage.Services
{
	public class EmailService
	{
		private readonly string _email = "rachit1575@gmail.com";
		private readonly string _password = "vwnw mwsz htlu ukry";

		public void SendOtp(string toEmail, string subject, string messageText)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Pharmacy Manage", _email));
			message.To.Add(MailboxAddress.Parse(toEmail));
			message.Subject = subject;

			message.Body = new TextPart("plain")
			{
				Text = messageText
			};

			using var client = new SmtpClient();
			client.Connect("smtp.gmail.com", 587, false);
			client.Authenticate(_email, _password);
			client.Send(message);
			client.Disconnect(true);
		}
	}
}