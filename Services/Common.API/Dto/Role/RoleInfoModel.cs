namespace BIVN.FixedStorage.Services.Common.API.Dto.Role
{
    public class RoleInfoModel
    {
        public string Name { get; set; }
        public List<PermissionModel> Permissions { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}
