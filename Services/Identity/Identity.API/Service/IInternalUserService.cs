using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;

namespace Identity.API.Service
{
    public interface IInternalService
    {
        ResponseModel<IEnumerable<InternalUserDto>> GetUsers();
        ResponseModel<IEnumerable<InternalDepartmentDto>> GetDepartments();
        ResponseModel<IEnumerable<RoleClaimDto>> GetUserRoles(string userId);
        ResponseModel<IEnumerable<ListUserModel>> ListUser();
        ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>> GetAllRoleWithUserNames();
    }
}
