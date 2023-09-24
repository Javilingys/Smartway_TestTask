using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Data.Configrations;

public class AppFileEntityConfiguration : IEntityTypeConfiguration<AppFile>
{
    public void Configure(EntityTypeBuilder<AppFile> builder)
    {
        builder.Property(x => x.SizeInBytes)
            .IsRequired();

        builder.Property(x => x.GroupId)
            .IsRequired();

        builder.Property(x => x.Path).IsRequired()
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Name).IsRequired()
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Extension)
            .IsUnicode(false)
            .IsRequired();
    }
}
