namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class CheckUserInfoDto
    {
        public CheckUserInfoDto(ValidateDto<LoginErrorDto> validation, UserInfoFromLoginDto userInfo)
        {
            Validation = validation;
            UserInfo = userInfo;
        }
        public ValidateDto<LoginErrorDto> Validation { get; set; }
        public UserInfoFromLoginDto UserInfo { get; set; }
    }
}
