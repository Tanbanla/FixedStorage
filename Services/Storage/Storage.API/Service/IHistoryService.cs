namespace Storage.API.Service
{
    public interface IHistoryService
    {
        Task<ResponseModel<HistoryPagedList>> GetHistories(HistoryFilterModel historyFilterModel);
        Task<ResponseModel<HistoryDetailModel>> GetHistoryDetail(string userId, string historyId);
        Task<ResponseModel<ResultSet<IEnumerable<HistoryInOutExportResultDto>>>> GetHistoriesToExportExcel(HistoryInOutExportDto historyFilterModel);
    }
}
