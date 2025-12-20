namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ValidateTokenDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string? DeviceId { get; set; }
        public string? UserId { get; set; }
        public string? AccountType { get; set; }
        public string Token { get; set; }
    }
}
