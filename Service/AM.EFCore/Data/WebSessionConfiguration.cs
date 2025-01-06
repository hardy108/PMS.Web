using AM.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM.EFCore.Data
{
    public class WebSessionConfiguration : IEntityTypeConfiguration<WebSession>
    {
        void IEntityTypeConfiguration<WebSession>.Configure(EntityTypeBuilder<WebSession> entity)
        {
            entity.HasKey(e => e.Token);

            entity.Property(e => e.Token)
                .IsUnicode(false)
                .ValueGeneratedNever();

            entity.Property(e => e.ExpiredDate)
                .HasColumnType("datetime")
                .HasComputedColumnSql("(dateadd(second,[LifeTimeInSeconds],[LastAccess]))");

            entity.Property(e => e.LastAccess)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        }
    }
}
