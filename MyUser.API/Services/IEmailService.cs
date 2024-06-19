namespace MyUser.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string body);

        string GenerateEmailConfirmationToken(string email);
        object GetPrincipalFromToken(string token);
    }
}
