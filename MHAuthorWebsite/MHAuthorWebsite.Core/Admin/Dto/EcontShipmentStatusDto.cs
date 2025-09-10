using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Core.Admin.Dto;

public class EcontShipmentStatusDto
{
    public string ShipmentNumber { get; set; } = null!;

    [JsonPropertyName("pdfURL")]
    public string PdfUrl { get; set; } = null!; // URL for the label PDF
}