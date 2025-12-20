namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ValidateTokenResultDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;

        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        //public string RoleIds { get; set; } = string.Empty;
        //public string RoleNames { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ExpiredDate { get; set; } = string.Empty;
#nullable enable        
        public List<RoleClaimDto?> RoleClaims { get; set; } = new List<RoleClaimDto?>();


        public string? Avatar { get; set; } = string.Empty;
        public string? SecurityStamp { get; set; } = string.Empty;

#nullable disable
        public string? Status { get; set; } = string.Empty;
        public InventoryLoggedInfo InventoryLoggedInfo { get; set; } = new();
    }
}
