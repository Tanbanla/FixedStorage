namespace BIVN.FixedStorage.Identity.API.Service
{
    public interface IRoleService
    {
        Task<ResponseModel<bool>> ExistRoleNameAsync(string roleName);

        Task<ResponseModel<IEnumerable<RoleInfoModel>>> GetRolesAsync();

        Task<ResponseModel<bool>> CreateAsync(CreateRoleModel createRoleModel);
        Task<ResponseModel<bool>> EditAsync(EditRoleModel editRoleModel);
        Task<ResponseModel<bool>> PrepareDeleteAsync(string roleId);
        Task<ResponseModel<bool>> DeleteAsync(string roleId, string userId);
        Task<ResponseModel<bool>> AssignUserRoleAsync(string userId, string roleId,string updatedBy);

    }
}
