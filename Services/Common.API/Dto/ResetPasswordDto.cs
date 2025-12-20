namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ResetPasswordDto
    {
        public string UserId { get; set; }

        public string CurrentUserId { get; set; }

        public string NewPassword { get; set; }        
    }
}
