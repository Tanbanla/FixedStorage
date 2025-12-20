namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ChangePasswordDto
    {
        public string UserId { get; set; }
        public string CurrentUserId { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }
    }
}
