namespace BIVN.FixedStorage.Services.Common.API;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions CamelCasing = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public static readonly JsonSerializerOptions CaseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true   
    };
    
    public static readonly JsonSerializerOptions CamelCaseOtions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
