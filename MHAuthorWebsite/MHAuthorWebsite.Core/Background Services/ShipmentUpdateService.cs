using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Core.NotificationTemplates.PayloadModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.Econt;

namespace MHAuthorWebsite.Core.Background_Services;

public class ShipmentUpdateService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ShipmentUpdateService> _logger;

    public ShipmentUpdateService(IServiceProvider services, ILogger<ShipmentUpdateService> logger)
    {
        _services = services;
        _logger = logger;
    }

    private TimeSpan DelayInterval => TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _services.CreateScope();
                IApplicationRepository repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                IShipmentUpdateDataService dataService = scope.ServiceProvider.GetRequiredService<IShipmentUpdateDataService>();
                IEcontService econtService = scope.ServiceProvider.GetRequiredService<IEcontService>();

                Order[] acceptedOrders = await dataService.GetOrdersOnTheWayReadonlyAsync(cancellationToken);

                if (acceptedOrders.Length == 0)
                {
                    await Task.Delay(DelayInterval, cancellationToken);
                    continue;
                }

                foreach (Order order in acceptedOrders)
                {
                    EcontOrderDto dto = new()
                    {
                        Id = order.Shipment.CourierShipmentId,
                        OrderNumber = order.Shipment.OrderNumber
                    };

                    ServiceResult<EcontShipmentStatusDto> sr = await econtService.GetTrackingInfo(dto);
                    if (!sr.Success) continue;

                    EcontShipmentStatusDto shipmentInfo = sr.Result!;
                    if (shipmentInfo.TrackingEvents!.Count > order.Shipment.Events.Count(e => e.Source == ShipmentEventSource.Econt))
                    {
                        repository.Attach(order.Shipment);

                        TimeZoneInfo bgTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");

                        ShipmentEvent[] newEvents = shipmentInfo.TrackingEvents
                            .Select(eventInfo =>
                            {
                                DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(
                                    DateTime.SpecifyKind(
                                        DateTime.Parse(eventInfo.Time!),
                                        DateTimeKind.Unspecified
                                    ),
                                    bgTimeZone
                                );

                                return new { eventInfo, utcTime };
                            })
                            .Where(x => !order.Shipment.Events
                                .Any(se => se.Source == ShipmentEventSource.Econt
                                           && se.Time == DateTime.Parse(x.eventInfo.Time!)
                                           && se.DestinationType == x.eventInfo.DestinationType
                                           && se.DestinationDetails == x.eventInfo.DestinationDetails
                                           && se.CityName == x.eventInfo.CityName
                                           && se.OfficeName == x.eventInfo.OfficeName))
                            .Select(x => new ShipmentEvent
                            {
                                DestinationType = x.eventInfo.DestinationType,
                                DestinationDetails = x.eventInfo.DestinationDetails,
                                CityName = x.eventInfo.CityName,
                                OfficeName = x.eventInfo.OfficeName,
                                Time = x.utcTime,
                                Source = ShipmentEventSource.Econt,
                                ShipmentId = order.Shipment.Id
                            })
                            .ToArray();

                        await repository.AddRangeAsync(newEvents);

                        order.Shipment.ExpectedDeliveryDate = shipmentInfo.ExpectedDeliveryDate is not null ?
                            DateTimeOffset.FromUnixTimeMilliseconds(shipmentInfo.ExpectedDeliveryDate!.Value).UtcDateTime : null;

                        if (shipmentInfo.SendTime != null) order.Status = OrderStatus.Shipped;
                        if (shipmentInfo.DeliveryTime != null) order.Status = OrderStatus.Delivered;

                        if (newEvents.Length == 0 || order.User.Name is null) continue;

                        ShipmentEvent latestEvent = newEvents.OrderByDescending(e => e.Time).First();

                        OrderStatusUpdatePayloadModel notificationPayloadModel = new()
                        {
                            ShipmentNumber = order.Shipment.ShipmentNumber,
                            OrderNumber = order.Shipment.OrderNumber,
                            CustomerName = order.User.Name,
                            EventTime = latestEvent.Time.ToString("dd/MM/yyyy HH:mm"),
                            Location = latestEvent.CityName ?? latestEvent.OfficeName ?? "Локацията не е налична!",
                            StatusUpdate = order.Status.GetDisplayName(),
                            TrackingUrl = $"{EcontTrackerUrl}/{order.Shipment.ShipmentNumber}",
                            LocationDetails = latestEvent.DestinationDetails,
                            OrderId = order.Id
                        };

                        ScheduledNotification notification = new()
                        {
                            Subject = $"Актуализация на поръчка {order.Shipment.OrderNumber}",
                            NotificationStatus = ScheduledNotificationStatus.Pending,
                            NotificationTemplate = ScheduledNotificationTemplate.OrderStatusUpdate,
                            NotificationType = ScheduledNotificationType.Email,
                            RecipientId = order.UserId,
                            ScheduledAt = DateTime.UtcNow,
                            Payload = JsonSerializer.Serialize(notificationPayloadModel),
                            TargetDeliveryDetails = order.Shipment.Email,
                            ExpirationDate = DateTime.UtcNow.AddDays(7)
                        };

                        await repository.AddAsync(notification);
                    }

                    await repository.SaveChangesAsync();
                }

                _logger.LogInformation("Shipment status was updated successfully to all orders!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {ServiceName}.", nameof(ShipmentUpdateService));
                await BackgroundServiceErrorReporter.ReportAsync(
                    _services,
                    ex,
                    nameof(ShipmentUpdateService),
                    _logger);
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}
