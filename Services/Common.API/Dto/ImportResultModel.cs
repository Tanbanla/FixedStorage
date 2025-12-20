namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ImportResponseModel : ResponseModel
    {
        public byte[] Bytes { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public string FileType { get; set; } = Constants.FileResponse.StreamType;
        public string FileName { get; set; }
    }


    public class ImportResponseModel<TResult> : ResponseModel<TResult>
    {
        public object Bytes { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public string FileType { get; set; } = Constants.FileResponse.StreamType;
        public string FileName { get; set; }

    }
    public class ImportResponseWithDataModel<TResult, TData> : ImportResponseModel<TResult>
    {
        public TData Data { get; set; }
    }

}
