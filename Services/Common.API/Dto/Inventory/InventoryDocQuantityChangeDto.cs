using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryDocQuantityChangeDto
    {
        public double OldQuantity { get; set; }
        public double NewQuantity { get; set; }
        public Guid InventoryDocId { get; set; }

    }
}
