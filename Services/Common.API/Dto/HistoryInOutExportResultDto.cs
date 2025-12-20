
namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class HistoryInOutExportResultDto
    {
        public string Id { get; set; }
        public string UserCode { get; set; }
        public string CreateBy { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public string DepartmentName { get; set; }
        public int ActivityType { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public double Quantity { get; set; }
        public double InventoryNumber { get; set; }
        public string Note { get; set; }

        public string FactoryId { get; set; }
        public string DepartmentId { get; set; }
        public string Layout { get; set; }

    }
    
}
