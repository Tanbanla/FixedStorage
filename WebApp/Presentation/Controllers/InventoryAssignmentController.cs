namespace WebApp.Presentation.Controllers
{
    public class InventoryAssignmentController : Controller
    {
        // GET: InventoryAssignmentController
        [RouteDisplayValue(RouteDisplay.INVENTORY_ASSIGNMENT)]
        [HttpGet("inventory-assignment")]
        public ActionResult Index()
        {
            var currUser = HttpContext.UserFromContext();
            var canAccess = currUser.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                            (User.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) || User.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) || User.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY))
                               || HttpContext.IsPromoter();
            if (!canAccess)
            {
                return RedirectPreventAccessResult();
            }

            return View();
        }
    }
}
