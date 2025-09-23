using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Core.Admin.Dto;

public class EcontShipmentStatusDto
{
    public string ShipmentNumber { get; set; } = null!;

    public string? StorageOfficeName { get; set; }

    public string? StoragePersonName { get; set; }

    public long? CreatedTime { get; set; }

    public long? SendTime { get; set; }

    public long? DeliveryTime { get; set; }

    public string? ShipmentType { get; set; }

    public int PackCount { get; set; }

    public double Weight { get; set; }

    public string? ShipmentDescription { get; set; }

    public string? SenderDeliveryType { get; set; }

    public string? SenderOfficeCode { get; set; }

    public string? ReceiverDeliveryType { get; set; }

    public string? ReceiverOfficeCode { get; set; }

    public decimal CdCollectedAmount { get; set; }

    public string? CdCollectedCurrency { get; set; }

    public long? CdCollectedTime { get; set; }

    public decimal CdPaidAmount { get; set; }

    public string? CdPaidCurrency { get; set; }

    public long? CdPaidTime { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Currency { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? DiscountDescription { get; set; }

    public decimal? SenderDueAmount { get; set; }

    public decimal? ReceiverDueAmount { get; set; }

    public decimal? OtherDueAmount { get; set; }

    public int? DeliveryAttemptCount { get; set; }

    public ICollection<EcontServiceDto> Services { get; set; } = new HashSet<EcontServiceDto>();

    public ICollection<EcontTrackingEventDto>? TrackingEvents { get; set; }

    [JsonPropertyName("pdfURL")]
    public string PdfUrl { get; set; } = null!;

    public long? ExpectedDeliveryDate { get; set; }

    public bool? PayAfterAccept { get; set; }

    public bool? PayAfterTest { get; set; }

    public bool? PartialDelivery { get; set; }

    public string? AlertRedirected { get; set; }

    [JsonPropertyName("PackingListPDFURL")]
    public string? PackingListPdfUrl { get; set; }

    [JsonPropertyName("ReturnShipmentURL")]
    public string? ReturnShipmentUrl { get; set; }

    public string? NotApplicableServices { get; set; }
}