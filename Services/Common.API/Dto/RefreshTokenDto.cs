namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class RefreshTokenDto
    {
        public string? DeviceId { get; set; }
        public string OldToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
