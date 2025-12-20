namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class RefreshTokenResultDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string DeviceId { get; set; }

    }
}
