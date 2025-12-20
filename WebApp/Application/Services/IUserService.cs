namespace WebApp.Application.Services
{
    public interface IUserService
    {
        ValidateDto<CreateUserErrorDto> ValidateCreateUserInput(CreateUserDto model);

        //Task<CreateUserDto> CreateUserDtoAsync();

        Task<ValidateDto<UpdateUserErrorDto>> ValidateUpdateUserInputAsync(UpdateUserDto model);

        bool IsValidStatus(int status);

        Task<ValidateDto<UserListExportErrorResultDto>> ValidateExportUserListFilterAsync(UserListExportFilterDto model);

        Task<byte[]> ExportFilteredUserListAsync(List<FilterListUserDto> dataModel);
    }
}
