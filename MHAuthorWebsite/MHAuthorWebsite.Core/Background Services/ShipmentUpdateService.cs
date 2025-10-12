using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MHAuthorWebsite.Core.Background_Services;

public class ShipmentUpdateService : BackgroundService
{
    private readonly IServiceProvider _services;

    public ShipmentUpdateService(IServiceProvider services) => _services = services;

    private TimeSpan DelayInterval => TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _services.CreateScope();
                IApplicationRepository repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                IEcontService econtService = scope.ServiceProvider.GetRequiredService<IEcontService>();

                Order[] acceptedOrders = await repository
                    .WhereReadonly<Order>(o => o.Status == OrderStatus.Shipped
                                                 || o.Status == OrderStatus.Accepted)
                    .Include(o => o.Shipment)
                        .ThenInclude(s => s.Events)
                    .ToArrayAsync(cancellationToken);

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

                        ShipmentEvent[] newEvents = shipmentInfo.TrackingEvents
                            .Where(te => !order.Shipment.Events
                                .Any(se => se.Source == ShipmentEventSource.Econt
                                           && se.Time == DateTime.Parse(te.Time!)
                                           && se.DestinationType == te.DestinationType
                                           && se.DestinationDetails == te.DestinationDetails
                                           && se.CityName == te.CityName
                                           && se.OfficeName == te.OfficeName))
                            .Select(eventInfo => new ShipmentEvent
                            {
                                DestinationType = eventInfo.DestinationType,
                                DestinationDetails = eventInfo.DestinationDetails,
                                CityName = eventInfo.CityName,
                                OfficeName = eventInfo.OfficeName,
                                Time = DateTime.Parse(eventInfo.Time!),
                                Source = ShipmentEventSource.Econt,
                                ShipmentId = order.Shipment.Id
                            })
                            .ToArray();

                        await repository.AddRangeAsync(newEvents);

                        order.Shipment.ExpectedDeliveryDate = shipmentInfo.ExpectedDeliveryDate is not null ?
                            DateTimeOffset.FromUnixTimeMilliseconds(shipmentInfo.ExpectedDeliveryDate!.Value).UtcDateTime : null;

                        if (shipmentInfo.SendTime != null) order.Status = OrderStatus.Shipped;
                        if (shipmentInfo.DeliveryTime != null) order.Status = OrderStatus.Delivered;

                        await repository.SaveChangesAsync();
                    }
                }

                Console.WriteLine("Shipment status was updated successfully to all orders!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}