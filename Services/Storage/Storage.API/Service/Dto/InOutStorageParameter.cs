namespace Storage.API.Service.Dto
{
    /// <summary>
    /// Mobile cần truyền dạng key value nên cần model này wrap lại
    /// </summary>
    public class InOutStorageParameterForMobile
    {
        public List<InOutStorageDto> Params { get; set; }
    }
}
