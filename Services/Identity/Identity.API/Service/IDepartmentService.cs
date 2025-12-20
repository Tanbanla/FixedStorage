namespace BIVN.FixedStorage.Identity.Service
{
    public interface IDepartmentService
    {
        Task<ResponseModel<IEnumerable<DepartmentDto>>> GetAllDepartmentAsync();
        Task<ResponseModel<IEnumerable<SelectUserDepartmentViewModel>>> UserListAsync();
        Task<ResponseModel<DepartmentDto>> GetDepartmentInfo(Guid Id);

        Task<ResponseModel<bool>> CheckEmptyDepartmentAsync(string departmentId);
        Task<ResponseModel<bool>> CheckExistDepartmentNameAsync(string name);
        Task<ResponseModel<bool>> CheckExistEditNameAsync(string departmentId, string name);

        Task<ResponseModel<bool>> CreateAsync(string departmentName, string userId, string creatorId);
        Task<ResponseModel<bool>> EditAsync(string departmentId, string departmentName, string creatorId, string userId);
        Task<ResponseModel<bool>> DeleteAsync(string departmentId, string userId);

        Task<ResponseModel<bool>> AssignUserToDepartmentAsync(string departmentId, string userId, string updatedBy);
    }
}
