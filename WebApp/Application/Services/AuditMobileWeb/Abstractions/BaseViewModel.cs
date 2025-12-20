namespace WebApp.Application.Services.AuditMobileWeb.Abstractions
{
    public abstract class BaseViewModel
    {
        public string Title { get; set; }

        public readonly AuditMobileComponent GoBackViewModel = new AuditMobileComponent(Constants.AuditMobileSettings.DefaultGoBackViewPath);
        public BaseViewModel()
        {

        }
    }
}
