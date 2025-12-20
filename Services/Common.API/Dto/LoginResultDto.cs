namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class LoginResultDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string DeviceId { get; set; }
        public string RoleId { get; set; }
        //public string RoleIds { get; set; }
        public string RoleName { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Avatar { get; set; }
        public string UserCode { get; set; }
        public string AccountType { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? SecuriryStamp { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string? NotificationBeforePasswordExpire { get; set; }
        public int? Status { get; set; }
        public string MobileAccess { get; set; } = string.Empty;
        public List<RoleClaimDto> RoleClaims { get; set; } = new List<RoleClaimDto>();
        public InventoryLoggedInfo InventoryLoggedInfo { get; set; }
    }
}
