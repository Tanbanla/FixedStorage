namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class UpdateUserInfoDto
    {
        
        public IFormFile file
        {
            get; set;
        }
        public string userId { get; set; }

    }
}
