using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocToExportInventoryErrorDto
    {
        [Required(ErrorMessage = "Vui lòng nhập InventoryId.")]
        public Guid InventoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Plant.")]
        public string Plant { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập AssigneeAccountId.")]
        public Guid AssigneeAccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ErrorMoney.")]
        public double? ErrorMoney { get; set; }
        public double? ErrorQuantity { get; set; }
        public string ComponentCode { get; set; }
    }
}
