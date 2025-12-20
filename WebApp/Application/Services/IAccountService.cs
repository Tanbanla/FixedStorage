namespace WebApp.Application.Services
{
    public interface IAccountService
    {
        ValidateDto<LoginErrorDto> ValidateLoginInput(LoginDto model);

        ValidateDto<ChangePasswordErrorDto> ValidateChangePasswordInput(ChangePasswordDto model);

        void SetUserCookies(LoginResultDto userInfo);

        bool TryAddCookie(string key, string value, CookieOptions options);

        void DeleteUserCookies();

        bool TryDeleteCookie(string key, CookieOptions options);

        ValidateDto<ResetPasswordErrorDto> ValidateResetPasswordInput(ResetPasswordDto model);
    }
}
