using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class TreeGroupInventoryErrorFilterDto
    {
        public List<string> Plants { get; set; } = new();
        public List<AssigneeAccount> AssigneeAccounts { get; set; } = new();
    }

    public class AssigneeAccount {
        public Guid AssigneeAccountId { get; set; }
        public string UserName { get; set; }
    }

}
