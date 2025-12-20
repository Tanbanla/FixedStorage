using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ImportErrorInvestigationUpdateModel
    {
        public string InventoryName { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public double ErrorQuantity { get; set; }
        public string ErrorCategory { get; set; }
        public string ErrorDetail { get; set; }
        public ModelStateDictionary Error { get; set; } = new();
    }

    public class ErrorInvestigationQuantityImportDto
    {
        public Guid InventoryId { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public string Plant { get; set; }
        public string WHloc { get; set; }
        public double? ErrorQuantity { get; set; }
    }
    public class InventoryErrorInvestigationQuantityImportDto
    {
        public Guid InventoryId { get; set; }
        public string InventoryName { get; set; }
    }

    public class MSLDataUpdateImportDto
    {
        public string MovementType { get; set; }
        public string ComponentCode { get; set; }
        public string Plant { get; set; }
        public string WHloc { get; set; }
        public string Quantity { get; set; }
        public ModelStateDictionary Error { get; set; } = new();
    }

    public class ImportErrorInvestigationUpdatePivotModel
    {
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string AccountQuantity { get; set; }
        public ModelStateDictionary Error { get; set; } = new();
    }

    public class ImportErrorInvestigationUpdatePivotDto
    {
        public string Plant { get; set; }
        public double TotalErrorMoney { get; set; }
        public double TotalAccountQuantity { get; set; }
        public double ErrorPercent { get; set; }
    }

    public class InventoryDocTypeADto
    {
        public string ComponentCode { get; set; }
        public string Plant { get; set; }
        public string WHloc { get; set; }
    }

}
