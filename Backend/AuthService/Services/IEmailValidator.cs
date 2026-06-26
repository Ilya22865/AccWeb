namespace AuthService.Services {
    public interface IEmailValidator {
        Task<bool> IsValid(string email, string apiKey);
    }
}