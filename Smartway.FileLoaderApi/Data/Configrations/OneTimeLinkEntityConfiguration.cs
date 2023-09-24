using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Data.Configrations;

public class OneTimeLinkEntityConfiguration : IEntityTypeConfiguration<OneTimeLink>
{
    public void Configure(EntityTypeBuilder<OneTimeLink> builder)
    {
        builder.Property(x => x.Token)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.GroupId)
            .IsRequired();

        builder.Property(x => x.Expiry)
            .IsRequired();

        builder.Property(x => x.WasUsed)
            .IsRequired();
    }
}
