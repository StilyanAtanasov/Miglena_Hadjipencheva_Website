using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder
            .Property(s => s.ShipmentNumber)
            .HasComment("Unique identifier for the shipment");

        builder
            .Property(s => s.AwbUrl)
            .HasComment("The URL for the air waybill document");

        builder
            .HasIndex(s => s.ShipmentNumber)
            .IsUnique();
    }
}