using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
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
                    .WhereReadonly<Order>(o => o.Status == Data.Models.Enums.OrderStatus.Shipped
                                                 || o.Status == Data.Models.Enums.OrderStatus.Accepted)
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
                    if (shipmentInfo.TrackingEvents.Count > order.Shipment.Events.Count)
                    {
                        repository.Attach(order.Shipment);

                        order.Shipment.Events = shipmentInfo.TrackingEvents
                            .Select(eventInfo => new ShipmentEvent
                            {
                                DestinationType = eventInfo.DestinationType,
                                DestinationDetails = eventInfo.DestinationDetails,
                                CityName = eventInfo.CityName,
                                OfficeName = eventInfo.OfficeName,
                                Time = DateTime.Parse(eventInfo.Time!),
                            })
                            .ToList();

                        order.Shipment.ExpectedDeliveryDate = shipmentInfo.ExpectedDeliveryDate is not null ?
                            DateTimeOffset.FromUnixTimeMilliseconds(shipmentInfo.ExpectedDeliveryDate!.Value).UtcDateTime : null;

                        if (shipmentInfo.SendTime != null) order.Status = Data.Models.Enums.OrderStatus.Shipped;
                        if (shipmentInfo.DeliveryTime != null) order.Status = Data.Models.Enums.OrderStatus.Delivered;

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