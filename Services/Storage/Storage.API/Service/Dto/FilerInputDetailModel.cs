namespace Storage.API.Service.Dto
{
    public class FilterInputDetailModel
    {
        public string UserId { get; set; }
        public string InputId { get; set; }
        public string IsAllFactories { get; set; }
        public List<string> Factories { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
