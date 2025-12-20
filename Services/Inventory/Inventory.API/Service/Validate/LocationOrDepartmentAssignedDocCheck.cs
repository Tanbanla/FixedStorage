using BIVN.FixedStorage.Services.Common.API.Helpers;

namespace Inventory.API.Service.Validate
{
    public class LocationOrDepartmentAssignedDocCheck : IApplicationValidation
    {
        private readonly Guid _locationId;

        public LocationOrDepartmentAssignedDocCheck(Guid locationId)
        {
            _locationId = locationId;
        }

        public async Task<ResponseModel<bool>> Validate(HttpContext httpContext)
        {
            var inventoryContext = httpContext.RequestServices.GetService<InventoryContext>();

            var location = await inventoryContext.InventoryLocations.FindAsync(_locationId);
            var anyLocationNameAssigned = await inventoryContext.InventoryDocs.AnyAsync(x => x.LocationName == location.Name || x.DepartmentName == location.DepartmentName);
            if (anyLocationNameAssigned)
            {
                return new ResponseModel<bool>
                {
                    Code = (int)HttpStatusCodes.LocationAssignedToDocument,
                    Data = true,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.LocationAssignedToDocument)
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = false
            };
        }
    }
}
