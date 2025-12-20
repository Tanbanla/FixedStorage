using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.QRCode
{
    public class TreeGroupQRCodeFilterDto
    {
        public List<string> MachineModels { get; set; } = new List<string>();
        public List<string> MachineTypes { get; set; } = new List<string>();
        public List<string> LineNames { get; set; } = new List<string>();
    }
}
