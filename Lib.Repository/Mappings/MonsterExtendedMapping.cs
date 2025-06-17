using Lib.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lib.Repository.Mappings;

public class MonsterExtendedMapping: IEntityTypeConfiguration<Monster>
{
    public void Configure(EntityTypeBuilder<Monster> builder)
    {
        builder.ToTable("Monster");

        builder.Property(p => p.Id).HasColumnType("INTEGER").IsRequired().ValueGeneratedOnAdd();
        builder.Property(p => p.Attack).HasColumnType("INTEGER").IsRequired();
        builder.Property(p => p.Defense).HasColumnType("INTEGER").IsRequired();
        builder.Property(p => p.Hp).HasColumnType("INTEGER").IsRequired();
        builder.Property(p => p.Name).HasColumnType("TEXT").IsRequired();
        builder.Property(p => p.ImageUrl).HasColumnType("TEXT").IsRequired();
        builder.Property(p => p.Speed).HasColumnType("INTEGER").IsRequired();

        builder.HasKey(p => p.Id);
    }
}
