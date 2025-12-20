using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ErrorCategoryManagementDto
    {
        public Guid Id { get; set; }
        public string ErrorCategoryKey { get; set; }
        public string ErrorCategoryName { get; set; }
    }

    
    public class ErrorCategoryModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên phân loại.")]
        public string Name { get; set; }
    }
}
