namespace Storage.API.Service.Dto
{
    public class InputStorageListQueryModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool isAllFactories { get; set; }
        public bool isAllStatuses { get; set; }
        public List<string> Factories { get; set; }
        public List<string> Statuses { get; set; }


        public int Skip { get; set; }
        public int PageSize { get; set; }
    }
}
