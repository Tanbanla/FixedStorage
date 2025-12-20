namespace BIVN.FixedStorage.Services.Common.API.Dto.Role
{
    public class CreateRoleModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm quyền")]
        public string Name { get; set; }
        public List<PermissionModel> Permissions { get; set; }
        public string UserId { get; set; }
    }
}
