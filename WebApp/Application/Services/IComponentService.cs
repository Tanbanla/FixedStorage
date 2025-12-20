using BIVN.FixedStorage.Services.Common.API.Dto.Component;

namespace WebApp.Application.Services
{
    public interface IComponentService
    {
        Task<ResponseModel> ExportFilteredComponentListAsync(List<ComponentFilterItemResultDto> model, string templateName);

        Task<bool> ImportComponentListFromExcel(List<ComponentCellDto> model, string resultFileName);
    }
}
