using System.Diagnostics;
using API.Controllers;
using API.Test.Fixtures;
using FluentAssertions;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Test;

public class MonsterExtendedControllerTests
{
    private readonly Mock<IBattleOfMonstersRepository> _repository;

    [Fact]
    public async Task Post_OkRequest_ImportCsv_With_CorrectFile()
    {
        // Arrange
        var filePath = @"C:\Users\dpena\Documents\Projects\FullStackLabs\assessment-cc-dotnet-sr-01\API.Test\Files\monsters-correct.csv";
        var fileMock = new Mock<IFormFile>();
        var content = await File.ReadAllBytesAsync(filePath);
        var stream = new MemoryStream(content);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns("monsters-correct.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        var repositoryMock = new Mock<IBattleOfMonstersRepository>();
        repositoryMock.Setup(repo => repo.Monsters.AddAsync(It.IsAny<IEnumerable<Monster>>()))
                      .Returns(Task.CompletedTask);

        var controller = new MonsterController(repositoryMock.Object);

        // Act
        var result = await controller.ImportCsv(fileMock.Object);

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    


    [Fact]
    public async Task Post_BadRequest_ImportCsv_With_Nonexistent_Monster()
    {
        var filePath = @"C:\Users\dpena\Documents\Projects\FullStackLabs\assessment-cc-dotnet-sr-01\API.Test\Files\monsters-wrong-column.csv";
        var fileMock = new Mock<IFormFile>();
        var content = await File.ReadAllBytesAsync(filePath);
        var stream = new MemoryStream(content);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns("monsters-empty-monster.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        var repositoryMock = new Mock<IBattleOfMonstersRepository>();
        var controller = new MonsterController(repositoryMock.Object);

        // Act
        var result = await controller.ImportCsv(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Wrong data mapping.");
    }

    [Fact]
    public async Task Post_BadRequest_ImportCsv_With_WrongDataMapping()
    {
        // Arrange
        var filePath = @"C:\Users\dpena\Documents\Projects\FullStackLabs\assessment-cc-dotnet-sr-01\API.Test\Files\monsters-empty-monster.csv";
        var fileMock = new Mock<IFormFile>();
        var content = await File.ReadAllBytesAsync(filePath);
        var stream = new MemoryStream(content);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns("monsters-empty-monster.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        var repositoryMock = new Mock<IBattleOfMonstersRepository>();
        var controller = new MonsterController(repositoryMock.Object);

        // Act
        var result = await controller.ImportCsv(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Wrong data mapping.");
    }
}
