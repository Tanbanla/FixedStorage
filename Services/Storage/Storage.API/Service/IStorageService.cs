namespace BIVN.FixedStorage.Services.Storage.API.Service
{
    public interface IStorageService
    {
        Task<ResponseModel> InOutStorageActivityAsync(int typeOfActivity, int typeOfBusiness, string positionCode, string supplierCode, Guid userId, double quantity, string reason, string? employeeCode);
        Task<ResponseModel<bool>> AllowImport();
        Task<ResponseModel<ImportStorageResultModel>> Import(string userId, IFormFile file);

        /// <summary>
        /// Danh sách nhập kho
        /// </summary>
        /// <returns></returns>
        Task<ResponseModel<ResultSet<IEnumerable<InputStorageListModel>>>> GetInputStorageList(InputStorageListQueryModel queryModel);
        Task<ResponseModel<IEnumerable<InputStorageDetailDto>>> GetInputDetails(FilterInputDetailModel filterModel);

        /// <summary>
        /// Cập nhật số lượng tạm thời trước khi xác nhận nhập kho
        /// </summary>
        /// <param name="inputId"></param>
        /// <param name="updateInputDetailDto"></param>
        /// <returns></returns>
        Task<ResponseModel<bool>> UpdateInputDetail(Guid inputDetailId, UpdateInputDetailDto updateInputDetailDto);

        Task<ResponseModel<bool>> ChangeInputDetailStatus(Guid inputDetailId, RemainingHanle remainingHanle);

        /// <summary>
        /// Xác nhận nhập kho
        /// </summary>
        /// <returns></returns>
        Task<ResponseModel<ImportStorageResultModel>> ConfirmImport(Guid userId, Guid inputId);

        /// <summary>
        /// Xóa lần nhập kho 
        /// </summary>
        /// <param name="inputId"></param>
        /// <returns></returns>
        Task<ResponseModel<bool>> DeleteBwinImport(Guid inputId);
        Task<ResponseModel> Export(string userId, InputStorageListQueryModel queryModel);
    }
}
