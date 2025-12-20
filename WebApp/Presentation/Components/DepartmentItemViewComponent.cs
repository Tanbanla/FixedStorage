namespace BIVN.FixedStorage.WebApp.Components
{
    [ViewComponent]
    public class DepartmentItemViewComponent : ViewComponent
    {
        public DepartmentItemViewComponent()
        {

        }

        public async Task<IViewComponentResult> InvokeAsync(DepartmentDto model)
        {
            ViewBag.Message = nameof(DepartmentItemViewComponent);

            return View(model);
        }
    }
}
