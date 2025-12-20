namespace BIVN.FixedStorage.WebApp.Components
{
    [ViewComponent]
    public class MessageViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string message)
        {
            return View(model: message);
        }
    }
}
