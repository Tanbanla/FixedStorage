using System.Text.Json;
using System.Text.Json.Serialization;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace Inventory.API.Service.Dto.ErrorInvestigation
{
    
    public class ErrorInvestigationDocumentListDto
    {
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public ErrorInvestigationStatusType Status { get; set; }
        public double? ErrorQuantity { get; set; }
        public double? ErrorMonyAbs { get; set; }
        public string Position { get; set; }
        public IEnumerable<DocumentList> DocumentList{ get; set; }
    }

    public class DocumentList
    {
        public Guid DocId { get; set; }
        public double? AccountQuantity { get; set; }
        public string DocCode { get; set; }
        public double? BOM { get; set; }
    }

    public class DoubleConverter : System.Text.Json.Serialization.JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
       => reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
       => writer.WriteStringValue(value.ToString("F3", CultureInfo.InvariantCulture ));
    }

    public class NullableDoubleConverter : System.Text.Json.Serialization.JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null ? (double?)null : reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
            => writer.WriteStringValue(value?.ToString("F3"));
    }

}
