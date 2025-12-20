namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class RoleClaimDto
    {
        public required string RoleId { get; set; }
        public required string RoleName { get; set; }
        //public required string RoleIds { get; set; }
        public required string ClaimType { get; set; }


#nullable enable
        public string? ClaimValue { get; set; }

    }
}
