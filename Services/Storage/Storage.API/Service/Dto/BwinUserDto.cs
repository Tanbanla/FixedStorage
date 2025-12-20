namespace Storage.API.Service.Dto
{
    public class BwinUserDto
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public DateTime CreateAt { get; set; }
        public string CreateBy { get; set; }
    }
}
