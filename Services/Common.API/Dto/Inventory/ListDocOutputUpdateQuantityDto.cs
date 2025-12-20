using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocOutputUpdateQuantityDto
    {
        public List<DocOutput_UpdateQuantity> DocOutputs { get; set; } = new();
        public Guid InventoryDocId { get; set; }

        public ListDocOutputUpdateQuantityDto(Guid invDocId)
        {
            InventoryDocId = invDocId;
        }
    }
    public class DocOutput_UpdateQuantity
    {
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }
}
