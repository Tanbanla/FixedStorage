namespace WebApp.Components
{
    public class CreateUserViewComponent : ViewComponent
    {        
        public CreateUserViewComponent()
        {           
        }

        public async Task<IViewComponentResult> InvokeAsync(CreateUserDto data)
        {
            return View(data);                              
        }
    }
}
