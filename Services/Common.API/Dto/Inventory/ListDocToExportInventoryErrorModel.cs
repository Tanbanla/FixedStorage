using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocToExportInventoryErrorModel
    {
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public double ErrorQuantity { get; set; }
        public double? ErrorMoney { get; set; }
        public double TotalQuantity { get; set; }
        public double AccountQuantity { get; set; }
        public List<DetailDocument> detailDocuments { get; set; } = new();
    }

    public class DetailDocument
    {
        public string DocCode { get; set; }
        public string Location { get; set; }
        public double Quantity { get; set; }
        public List<DetailDocOutput> detailDocOutputs { get; set; } = new();
    }
    public class DetailDocOutput
    {
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }
}
