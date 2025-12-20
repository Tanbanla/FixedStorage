namespace Storage.API.Service.Dto
{
    public class HistoryFilterModel : IQueryAllForExport
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public int QuantityFrom { get; set; }
        public int QuantityTo { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }

        public bool isAllType { get; set; }
        public bool isAllFactories { get; set; }
        public bool isAllLayouts { get; set; }
        public bool isAllDepartments { get; set; }

        public List<string> Types { get; set; }
        public List<string> Factories { get; set; }
        public List<string> Layouts { get; set; }
        public List<string> Departments { get; set; }


        public int Skip { get; set; }
        public int PageSize { get; set; }

        public bool IsGetAll { get; set; } = false;
    }
}
