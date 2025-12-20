namespace Storage.API.Service.Dto
{
    public class UpdateInputDetailDto
    {
        public string InputId { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public string UserId { get; set; }
    }
}
