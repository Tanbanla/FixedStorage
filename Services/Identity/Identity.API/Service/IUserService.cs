namespace BIVN.FixedStorage.Identity.API.Service
{
    public interface IUserService
    {
        Task<ResponseModel> UpdateUserInfo(UpdateUserInfoDto updateUserInfo);
        Task<ResponseModel> GetUserInfo(string userId);

        Task<ResponseModel<CreateUserDto>> CreateUserAsync();

        Task<ValidateDto<CreateUserErrorDto>> ValidateCreateUserInputAsync(CreateUserDto model);
       
        Task<ResponseModel<CreateUserResultDto>> CreateUserAsync(CreateUserDto model);
        Task<ResponseModel<IEnumerable<FilterListUserDto>>> FilterUser(FilterUseModel filterUseModel);
        Task<ResponseModel<IList<string>>>StatusUser();

        Task<ResponseModel<UserDetailDto>> GetUserDetailAsync(string id);

        Task<ValidateDto<UpdateUserErrorDto>> ValidateUpdateUserInputAsync(UpdateUserDto model);

        Task<ResponseModel<UpdateUserDto>> UpdateUserAsync(UpdateUserDto model);
        Task<ResponseModel> UpdateAccountTypeUser(string userId, AccountType accountType, string updateBy);
        Task<ResponseModel> UpdateStatusUser(string userId, UserStatus status, string updateBy);

        bool IsValidStatus(int status);
        Task<ResponseModel> GetUserInfoMobileAfterLogin(string userId);

        Task<ResponseModel<LockUserListDto>> LockUsersByExpiredPasswordOrUnactiveAsync();

        Task<ResponseModel<IEnumerable<FilterListUserDto>>> GetFilterUserListExport(UserListExportFilterDto filterModel);        
    }
}
