namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ValidateTokenErrorDto
    {                      
        public string? ClientId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
        public string? AccountType { get; set; } = string.Empty;
        public string? DeviceId { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;

        public string? Status { get; set; } = string.Empty;
    }
}
