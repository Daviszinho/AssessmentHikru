using API.Controllers;
using API.Test.Fixtures;
using FluentAssertions;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Test;

public class BattleExtendedControllerTests
{
    private readonly Mock<IBattleOfMonstersRepository> _repository;
    
    [Fact]
    public async Task Post_OnNoMonsterFound_When_StartBattle_With_NonexistentMonster()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var battle = new Battle { Id = 1, MonsterA = null, MonsterB = null };

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("MonsterA and MonsterB cannot be null.");
    }
    

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var monsterA = new Monster { Id = 1, Name = "MonsterA", Attack = 10, Defense = 5, Speed = 5, Hp = 10 };
        var monsterB = new Monster { Id = 2, Name = "MonsterB", Attack = 2, Defense = 5, Speed = 5, Hp = 10 };
        var battle = new Battle { Id = 1, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterA.Id)).ReturnsAsync(monsterA);
        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterB.Id)).ReturnsAsync(monsterB);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var battleResult = okResult!.Value as Battle;
        battleResult.Should().NotBeNull();
        battleResult!.Winner.Should().Be(1);
    }
   

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterBWinning()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var monsterA = new Monster { Id = 1, Name = "MonsterA", Attack = 2, Defense = 5, Speed = 5, Hp = 10 };
        var monsterB = new Monster { Id = 2, Name = "MonsterB", Attack = 10, Defense = 5, Speed = 5, Hp = 10 };
        var battle = new Battle { Id = 1, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterA.Id)).ReturnsAsync(monsterA);
        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterB.Id)).ReturnsAsync(monsterB);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var battleResult = okResult!.Value as Battle;
        battleResult.Should().NotBeNull();
        battleResult!.Winner.Should().Be(2);
    }
   
    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning_When_TheirSpeedsSame_And_MonsterA_Has_Higher_Attack()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var monsterA = new Monster { Id = 1, Name = "MonsterA", Attack = 10, Defense = 5, Speed = 5, Hp = 10 };
        var monsterB = new Monster { Id = 2, Name = "MonsterB", Attack = 2, Defense = 5, Speed = 5, Hp = 10 };
        var battle = new Battle { Id = 1, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterA.Id)).ReturnsAsync(monsterA);
        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterB.Id)).ReturnsAsync(monsterB);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var battleResult = okResult!.Value as Battle;
        battleResult.Should().NotBeNull();
        battleResult!.Winner.Should().Be(1);
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterBWinning_When_TheirSpeedsSame_And_MonsterB_Has_Higher_Attack()
    {
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var monsterA = new Monster { Id = 1, Name = "MonsterA", Attack = 2, Defense = 5, Speed = 5, Hp = 10 };
        var monsterB = new Monster { Id = 2, Name = "MonsterB", Attack = 10, Defense = 5, Speed = 5, Hp = 10 };
        var battle = new Battle { Id = 1, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterA.Id)).ReturnsAsync(monsterA);
        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterB.Id)).ReturnsAsync(monsterB);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var battleResult = okResult!.Value as Battle;
        battleResult.Should().NotBeNull();
        battleResult!.Winner.Should().Be(2);
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning_When_TheirDefensesSame_And_MonsterA_Has_Higher_Speed()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var monsterA = new Monster { Id = 1, Name = "MonsterA", Attack = 10, Defense = 5, Speed = 7, Hp = 10 };
        var monsterB = new Monster { Id = 2, Name = "MonsterB", Attack = 2, Defense = 5, Speed = 5, Hp = 10 };
        var battle = new Battle { Id = 1, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterA.Id)).ReturnsAsync(monsterA);
        mockRepository.Setup(repo => repo.Monsters.FindAsync(monsterB.Id)).ReturnsAsync(monsterB);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.StartBattle(battle);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var battleResult = okResult!.Value as Battle;
        battleResult.Should().NotBeNull();
        battleResult!.Winner.Should().Be(1);
    }
    
    [Fact]
    public async Task Delete_OnSuccess_RemoveBattle()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var battleId = 1;
        var battle = new Battle { Id = battleId, MonsterA = 1, MonsterB = 2 };

        mockRepository.Setup(repo => repo.Battles.FindAsync(battleId)).ReturnsAsync(battle);
        mockRepository.Setup(repo => repo.Save()).ReturnsAsync(1);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.Remove(battleId);

        // Assert
        var okResult = result as OkResult;
        okResult.Should().NotBeNull();
        mockRepository.Verify(repo => repo.Battles.RemoveAsync(battleId), Times.Once);
        mockRepository.Verify(repo => repo.Save(), Times.Once);
    }
    
    [Fact]
    public async Task Delete_OnNoBattleFound_Returns404()
    {
        // Arrange
        var mockRepository = new Mock<IBattleOfMonstersRepository>();
        var nonExistentBattleId = 999;

        mockRepository.Setup(repo => repo.Battles.FindAsync(nonExistentBattleId)).ReturnsAsync((Battle?)null);

        var controller = new BattleExtendedController(mockRepository.Object);

        // Act
        var result = await controller.Remove(nonExistentBattleId);

        // Assert
        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }
}
