using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.QRCode
{
    public class ListDocTypeCToExportQRCodeDto
    {
        [Required(ErrorMessage = "Vui lòng nhập InventoryId.")]
        public string InventoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập MachineModel.")]
        public string MachineModel { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập MachineType.")]
        public string MachineType { get; set; }
        public string LineName { get; set; }
    }
}
