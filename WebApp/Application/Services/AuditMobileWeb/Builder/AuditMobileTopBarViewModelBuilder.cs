using WebApp.Application.Services.AuditMobileWeb.Abstractions;

namespace WebApp.Application.Services.AuditMobileWeb.Builder
{
    public class AuditMobileTopBarViewModelBuilder : BaseViewModelBuilder<AuditMobileTopBarViewModel, AuditMobileTopBarViewModelBuilder>
    {
        public AuditMobileTopBarViewModelBuilder()
        {

        }
        public virtual AuditMobileTopBarViewModelBuilder DisplayToggleSideBar(bool state = false)
        {
            _viewModel.SideBarViewModel.IsShow = state;
            return this;
        }
        public virtual AuditMobileTopBarViewModelBuilder EnableDropDownSettings(bool state = false)
        {
            _viewModel.FilterViewModel.IsShow = state;
            return this;
        }

        public virtual AuditMobileTopBarViewModelBuilder ChangeSideBarView(string viewPath)
        {
            _viewModel.SideBarViewModel.ViewPath = viewPath;
            return this;
        }
    }
}
