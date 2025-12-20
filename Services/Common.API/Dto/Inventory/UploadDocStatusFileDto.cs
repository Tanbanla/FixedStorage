using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class UploadDocStatusFileDto
    {
        public string LocationName { get; set; }
        public string DocCode { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string Plant { get; set; }
        public string WareHouseLocation { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
    }
}
