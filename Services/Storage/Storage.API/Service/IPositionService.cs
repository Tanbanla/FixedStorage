namespace BIVN.FixedStorage.Services.Storage.API.Service
{
    public interface IPositionService
    {
        //Task<ResponseModel> UpdateStatus(StorageUpdateDto updateDto);
        Task<ResponseModel> GetComponentInfoAndListPosition(ComponentDto componentDto);
        Task<ResponseModel> GetListStorage();
        Task<ResponseModel<List<LayoutDto>>> GetLayoutList();
        Task<ResponseModel> AddNewComponent(CreateComponentDto createComponentDto, string userId);
        Task<ResponseModel> GetDetailComponent(string id);

        Task<ValidateFilterDto<ComponentsFilterErrorDto>> ValidateFilterModelGetFilterComponents(ComponentsFilterDto filterModel);

        Task<ResponseModel<PagedList<ComponentFilterItemResultDto>>> GetAllComponentsPaging(ComponentsFilterDto filterModel);

        Task<ResponseModel<List<DropDownListItemDto>>> GetLayoutDropDownList();
        Task<ResponseModel> UpdateComponent(UpdateComponentDto updateComponentDto, string componentId,string userId);
        Task<ResponseModel> DeleteComponents(List<string> ids);
        Task<ResponseModel<List<ComponentFilterItemResultDto>>> GetAllComponentsToExport(ComponentsFilterToExportDto model);
        Task<ResponseModel> DeleteLayout(string layout);
        Task<ResponseModel<ComponentItemImportResultDto>> ImportExcelComponentListAsync([FromForm] IFormFile file);

        Task<ResponseModel<List<DropDownListItemDto>>> GetInventoryStatusDropDownList();
    }
}
