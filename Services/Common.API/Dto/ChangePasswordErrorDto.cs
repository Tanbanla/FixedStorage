namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ChangePasswordErrorDto
    {
        //public string UserId { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }
    }
}
