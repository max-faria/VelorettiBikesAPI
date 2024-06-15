namespace VelorettiAPI.Models
{
    public class RecoverPasswordRequest
    {
        public required string Email { get; set; }
    }
    public class ResetPasswordRequest
    {
        public required string Token { get; set; }
        public required string Password { get; set; }
    }
}