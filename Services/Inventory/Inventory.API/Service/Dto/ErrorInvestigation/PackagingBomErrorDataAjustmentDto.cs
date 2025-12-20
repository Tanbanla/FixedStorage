using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace Inventory.API.Service.Dto.ErrorInvestigation
{
    public class PackagingBomErrorDataAjustmentDto
    {
        public int No { get; set; } = 0;
        public Guid ErrorInvestigationId { get; set; }
        public string Plant { get; set; }
        public string WareHouseLocation { get; set; }
        public string ComponentCode { get; set; }
        public double? AdjustQuantity { get; set; }
        public int? ErrorCategory { get; set; }
    }

    public class InventoryMisuseErrorDataAdjustmentDto
    {
        public int No { get; set; } = 0;
        public Guid ErrorInvestigationId { get; set; }
        public string DocCode { get; set; }
        public string ComponentCode { get; set; }
        public string Note { get; set; }
        public double? ErrQuantity { get; set; }
        public double? AdjustQuantity { get; set; }
        public string BOMxQuantity { get; set; }
        public int? ErrorCategory { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }

    }

    public class ExportDataAdjustmentDto
    {
        public IQueryable<InventoryMisuseErrorDataAdjustmentDto> InventoryMisuse { get; set; }
        public IQueryable<PackagingBomErrorDataAjustmentDto> PackagingBom { get; set; }
    }
}
