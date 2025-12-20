namespace WebApp.Application.Services.AuditMobileWeb.Abstractions
{
    public abstract class BaseViewModelBuilder<TViewModel, TBuilder>
            where TViewModel : BaseViewModel, new()
            where TBuilder : BaseViewModelBuilder<TViewModel, TBuilder>
    {
        protected readonly TViewModel _viewModel;
        public BaseViewModelBuilder()
        {
            _viewModel = new TViewModel();
        }

        public virtual TBuilder CreateTitle(string title)
        {
            _viewModel.Title = title;
            return (TBuilder)this;
        }

        public virtual TBuilder DisplayGoBackButton(bool state = false)
        {
            _viewModel.GoBackViewModel.IsShow = state;
            return (TBuilder)this;
        }

        public virtual TBuilder ChangeGoBackView(string viewPath)
        {
            _viewModel.GoBackViewModel.ViewPath = viewPath;
            return (TBuilder)this;
        }

        public virtual TViewModel Build()
        {
            return _viewModel;
        }
    }
}
