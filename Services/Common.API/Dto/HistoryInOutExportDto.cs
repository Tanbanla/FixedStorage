
namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class HistoryInOutExportDto
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

        public List<string> Types { get; set; } = new();
        public List<string> Factories { get; set; } = new();
        public List<string> Layouts { get; set; } = new();
        public List<string> Departments { get; set; } = new();

    }
    
}
