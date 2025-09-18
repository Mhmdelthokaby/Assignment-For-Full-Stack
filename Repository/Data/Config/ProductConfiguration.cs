using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Config
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ProductCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(p => p.ProductCode)
                   .IsUnique();

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Price)
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.DiscountRate)
                   .HasColumnType("decimal(5,2)");

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .IsRequired(false);

            // Configure the relationship with User
            builder.HasOne(p => p.Owner)
                   .WithMany()
                   .HasForeignKey(p => p.CreatedBy) // Use CreatedBy as the foreign key
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
