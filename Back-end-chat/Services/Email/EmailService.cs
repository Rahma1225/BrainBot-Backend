using System.Net;
using System.Net.Mail;
using System.IO;
using System.Threading.Tasks;

namespace Back_end_chat.Services
{
	public class EmailService
	{
		public async Task SendEmailWithInlineImageAsync(string to, string subject, string htmlTemplateWithPlaceholder, string imagePath, string placeholder = "##IMAGE##")
		{
			// Load image and convert to base64
			var imageBytes = await File.ReadAllBytesAsync(imagePath);
			var base64Image = Convert.ToBase64String(imageBytes);
			var contentType = Path.GetExtension(imagePath).ToLower() == ".png" ? "image/png" : "image/jpeg";

			// Replace placeholder in HTML with base64 image
			var htmlContent = htmlTemplateWithPlaceholder.Replace(
				placeholder,
				$"<img src='data:{contentType};base64,{base64Image}' width='150' alt='Logo' />"
			);

			var message = new MailMessage
			{
				From = new MailAddress("timsoftbot@gmail.com"),
				Subject = subject,
				Body = htmlContent,
				IsBodyHtml = true
			};

			message.To.Add(to);

			using var smtp = new SmtpClient("smtp.gmail.com", 587)
			{
				Credentials = new NetworkCredential("timsoftbot@gmail.com", "kxcgbxnblumerivv"),
				EnableSsl = true
			};

			await smtp.SendMailAsync(message);
		}
	}
}
