namespace BIVN.FixedStorage.Identity.API.Service
{
    public interface IIdentityService
    {
        Task<ValidateDto<LoginErrorDto>> ValidateLoginInput(LoginDto model);

        Task<CheckUserInfoDto> GetUserInfoFromLoginAsync(LoginDto model, bool isForRefreshToken = false);

        void CheckPasswordExpiration(CheckUserInfoDto checkUser);

        Task<ResponseModel<LoginResultDto>> GenerateJwtTokenAsync(UserInfoFromLoginDto user);
        Task<ResponseModel<RefreshTokenResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        ValidateTokenDto GetTokenInfo(HttpContext httpContext);

        ValidateDto<ValidateTokenErrorDto> ValidateTokenInfoInput(ValidateTokenDto model);

        Task<ResponseModel<ValidateTokenResultDto>> ValidateJwtTokenAsync(string jwtToken, bool isForRefreshToken = false);

        ClaimsIdentity CreateClaimsIdentity(ValidateTokenResultDto userInfo);

        ValidateDto<LogoutResultDto> ValidateLogoutInput(LogoutDto userId);

        Task<ValidateDto<LogoutResultDto>> UpdateDeviceTokenLogoutAsync(LogoutDto userId);

        ValidateDto<LogoutResultDto> ValidateLogoutInput(LogoutMultipleDto userIdList);

        Task<ValidateDto<LogoutResultDto>> UpdateDeviceTokenLogoutAsync(LogoutMultipleDto userIdList);

        ValidateDto<ChangePasswordErrorDto> ValidateChangePasswordInput(ChangePasswordDto model);

        Task<ResponseModel<ChangePasswordResultDto>> ChangePasswordAsync(ChangePasswordDto model);

        Task<ResponseModel<bool>> AuthorizePermission(string permission);
        Task RemoveExpiredToken();

        ValidateDto<ResetPasswordErrorDto> ValidateResetPasswordInput(ResetPasswordDto model);

        Task<ResponseModel<ChangePasswordResultDto>> ResetPasswordAsync(ResetPasswordDto model);

        Task<LogoutMultipleDto> GetUserIdListByRoleIdAsync(string roleId);

        Task<ResponseModel<ValidateTokenResultDto>> ValidateJwtRefreshTokenAsync(string jwtToken, string refreshToken, bool isForRefreshToken = false);
    }
}
