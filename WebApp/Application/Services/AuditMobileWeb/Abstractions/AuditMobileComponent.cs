namespace WebApp.Application.Services.AuditMobileWeb.Abstractions
{
    public class AuditMobileComponent
    {
        public virtual bool IsShow { get; set; } = false;
        /// <summary>
        /// Đường dẫn partial view hoặc component
        /// </summary>
        public virtual string ViewPath { get; set; }
        public AuditMobileComponent(string viewPath)
        {
            ViewPath = viewPath;
        }
    }
}
