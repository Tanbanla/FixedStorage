using WebApp.Application.Services.AuditMobileWeb.Abstractions;

namespace WebApp.Application.Services.AuditMobileWeb
{
    public class AuditMobileTopBarViewModel : BaseViewModel
    {
        /// <summary>
        /// Hiển thị bộ lọc danh sách linh kiện giám sát
        /// </summary>
        public readonly AuditMobileComponent SideBarViewModel = new AuditMobileComponent(Constants.AuditMobileSettings.DefaultSideBarViewPath);
        
        /// <summary>
        /// Bộ lọc màn danh sách giám sát
        /// </summary>
        public readonly AuditMobileComponent FilterViewModel = new AuditMobileComponent(Constants.AuditMobileSettings.DefaultAuditFilterViewPath);
    }
}
