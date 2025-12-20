namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class UserInfoFromLoginDto
    {
        public UserInfoFromLoginDto()
        {

        }
        public UserInfoFromLoginDto(string deviceId)
        {
            DeviceId = deviceId;
        }
#nullable enable
        public AppUser? User { get; set; }
        public string? DeviceId { get; set; } = string.Empty;
        public string? RoleId { get; set; } = string.Empty;
        public string? RoleName { get; set; } = string.Empty;
        //public string DepartmentId { get; set; } = string.Empty;
        public string? DepartmentName { get; set; } = string.Empty;
        public string? AccountType { get; set; } = string.Empty;
        //public string RoleIds { get; set; }        
        public string? ExpiredPasswordNotification { get; set; }
        public List<RoleClaimDto?> RoleClaims { get; set; } = new List<RoleClaimDto?>();
#nullable disable

    }
}
