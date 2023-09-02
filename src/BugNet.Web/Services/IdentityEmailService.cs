using System.Net.Mail;

namespace BugNet.Web.Services
{
	public class IdentityEmailService : IEmailSender
	{
		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var message = new MailMessage
			{
				From = new MailAddress("no-reply@bugnet.com"),
				Body = $"<html><body>{htmlMessage}</body></html>",
				IsBodyHtml = true,
				Priority = MailPriority.Normal,
				Subject = subject
			};

			message.To.Add(new MailAddress(email));

			//todo: need to wire up the settings and create a factory for creating dynamic email service
			var smtpClient = new SmtpClient
			{
				DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
				PickupDirectoryLocation = @"D:\Temp\Email\"
			};

			await Task.CompletedTask;
			smtpClient.SendAsync(message, null);
		}
	}
}
