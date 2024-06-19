namespace MyUser.API.Services
{
    using System;
    using System.Text;

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            // Implement email sending logic here
            Console.WriteLine($"Sending email to {email} with subject: {subject}");
            Console.WriteLine($"Body: {body}");
            await Task.CompletedTask;
        }

        public string GenerateEmailConfirmationToken(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null.");
            }

            // Example logic: Convert email to bytes and encode as Base64 string
            byte[] emailBytes = Encoding.UTF8.GetBytes(email);
            return Convert.ToBase64String(emailBytes);
        }

        public object GetPrincipalFromToken(string token)
        {
            throw new NotImplementedException();
        }
    }

}
