using Lib.Repository;
using Lib.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Test;


public class MonsterMappingTests
{
    [Fact]
    public void MonsterMapping_HasManyRelationships_AreConfiguredExplicitly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BattleOfMonstersContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using var context = new BattleOfMonstersContext(options);
        var model = context.Model;

        // Act
        var battleEntity = model.FindEntityType(typeof(Battle));
        var monsterEntity = model.FindEntityType(typeof(Monster));

        // Assert
        Assert.NotNull(battleEntity);
        Assert.NotNull(monsterEntity);

        // Validate the foreign keys and relationships are configured explicitly
        var monsterARelation = battleEntity.FindNavigation(nameof(Battle.MonsterARelation));
        Assert.NotNull(monsterARelation);
        Assert.Equal(monsterEntity, monsterARelation.TargetEntityType);

        var monsterBRelation = battleEntity.FindNavigation(nameof(Battle.MonsterBRelation));
        Assert.NotNull(monsterBRelation);
        Assert.Equal(monsterEntity, monsterBRelation.TargetEntityType);

        var winnerRelation = battleEntity.FindNavigation(nameof(Battle.WinnerRelation));
        Assert.NotNull(winnerRelation);
        Assert.Equal(monsterEntity, winnerRelation.TargetEntityType);

        // Validate the foreign keys are correctly associated with the navigation properties
        Assert.False(monsterARelation.ForeignKey.IsUnique);
        Assert.False(monsterBRelation.ForeignKey.IsUnique);
        Assert.False(winnerRelation.ForeignKey.IsUnique);
    }
}
