using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder
            .HasIndex(s => s.ShipmentNumber)
            .IsUnique();
    }
}