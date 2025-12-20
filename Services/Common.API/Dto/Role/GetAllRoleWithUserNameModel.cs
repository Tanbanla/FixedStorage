using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Role
{
    public class GetAllRoleWithUserNameModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string Department { get; set; }
    }
}
