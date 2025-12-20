using BIVN.FixedStorage.Services.Common.API.Dto.Location;

namespace Inventory.API.Service
{
    public interface ILocationService
    {
        Task<ResponseModel<IEnumerable<LocationViewModel>>> GetLocations(string departmentName);
        Task<ResponseModel<LocationViewModel>> LocationDetail(Guid locationId);
        Task<ResponseModel<bool>> AddLocation(CreateLocationDto createLocationDto); 
        Task<ResponseModel<bool>> UpdateLocation(UpdateLocationDto updateLocationDto); 
        Task<ResponseModel<bool>> DeleteLocation(Guid locationId, Guid userId);

        Task<ResponseModel<ResultSet<IEnumerable<InventoryActorInfoViewModel>>>> AssignmentActorList();
        Task<ResponseModel<bool>> ChangeRole(Guid userId, int? roleType, Guid actorId);
        Task<ResponseModel<bool>> ChangeLocation(Guid userId, List<Guid> locationsIds, Guid actorId);

        Task<ResponseModel<byte[]>> ExportAssignment();
        Task<ResponseModel> GetDeparments();
        Task<ResponseModel> GetLocationByDepartments(LocationByDepartmentDto departments);
        Task<ResponseModel<bool>> CanEditLocation(Guid locationId);
        Task<ResponseModel<IEnumerable<AuditorByLocationsDto>>> GetAuditorByLocations(AuditorByLocationModel locations);
    }
}
