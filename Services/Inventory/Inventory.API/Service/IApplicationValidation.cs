namespace Inventory.API.Service
{
    public interface IApplicationValidation
    {
        Task<ResponseModel<bool>> Validate(HttpContext httpContext);
    }
}
