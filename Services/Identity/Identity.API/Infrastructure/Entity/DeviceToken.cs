namespace BIVN.FixedStorage.Identity.API
{
    public class DeviceToken
    {
        public required Guid Id { get; set; }
        public required string DeviceId { get; set; }
        public string? UserId { get; set; }
        public required string Token { get; set; }
        public bool Status { get; set; }
        public DateTime TokenExpireTime { get; set; }
        public HttpStatusCodes? ForceLogoutCode { get; set; }
    }
}
