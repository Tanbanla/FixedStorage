using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ListErrorInvestigationDocumentsDto
    {
        public string DocCode { get; set; }
        public string ModelCode { get; set; }
        public string ComponentCode { get; set; }
        public double? BOM { get; set; }
        public double? TotalQuantity { get; set; }
        public string AssigneeAccount { get; set; }
        public string Position { get; set; }
        public string AttachModule { get; set; }
        public Guid DocId { get; set; }
    }
}
