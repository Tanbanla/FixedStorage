namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListAuditViewModel
    {
        public List<AuditInfoModel> AuditInfoModels { get; set; } = new();
        //Số phiếu giám sát
        public int FinishCount { get; set; }
        //Tổng số phiếu
        public int TotalCount { get; set; }
    }

    public class AuditInfoModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid? AccountId { get; set; }
        public int Status { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public int OrderByStatus { get; set; }
    }
}
