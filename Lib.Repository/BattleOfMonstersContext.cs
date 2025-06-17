using Lib.Repository.Entities;
using Lib.Repository.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Lib.Repository;

public sealed class BattleOfMonstersContext : DbContext
{
    public DbSet<Battle> Battle { get; set; } = null!;
    public DbSet<Monster> Monster { get; set; } = null!;


    public BattleOfMonstersContext(DbContextOptions<BattleOfMonstersContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Battle>().Ignore(b => b.MonsterARelation);
        modelBuilder.Entity<Battle>().Ignore(b => b.MonsterBRelation);
        modelBuilder.Entity<Battle>().Ignore(b => b.WinnerRelation);
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfiguration(new BattleMapping());
        modelBuilder.ApplyConfiguration(new MonsterMapping());
        modelBuilder.ApplyConfiguration(new MonsterExtendedMapping());
    }
}
