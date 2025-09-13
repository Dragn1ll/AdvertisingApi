using Application.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using Persistence.DataAccess;
using Persistence.DataAccess.Repositories;

namespace AdvertisingTests;

public class AdvertisingPlatformRepositoryTests
{
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<ILogger<AdvertisingPlatformRepository>> _loggerMock;
    private readonly AdvertisingPlatformRepository _repository;
    private readonly Mock<DbSet<AdvertisingPlatformEntity>> _dbSetMock;

    public AdvertisingPlatformRepositoryTests()
    {
        _dbContextMock = new Mock<AppDbContext>();
        _loggerMock = new Mock<ILogger<AdvertisingPlatformRepository>>();
        _dbSetMock = new Mock<DbSet<AdvertisingPlatformEntity>>();
        
        _repository = new AdvertisingPlatformRepository(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Add_ShouldReturnSuccess_WhenEntityIsAdded()
    {
        // Arrange
        var entity = new AdvertisingPlatformEntity { Name = "Test", Locations = ["/test"] };

        _dbContextMock
            .Setup(x => x.AddAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EntityEntry<AdvertisingPlatformEntity>>(
                Task.FromResult((EntityEntry<AdvertisingPlatformEntity>)null!)));

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.Add(entity);

        // Assert
        Assert.True(result.IsSuccess);
        _dbContextMock.Verify(x => x.AddAsync(entity, It.IsAny<CancellationToken>()), 
            Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Add_ShouldReturnFailure_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var entity = new AdvertisingPlatformEntity { Name = "Test", Locations = ["/test"] };
        var exception = new DbUpdateException("Database error");
        
        _dbContextMock
            .Setup(x => x.AddAsync(entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _repository.Add(entity);

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(ErrorType.ServerError, result.Error!.ErrorType);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database error")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public async Task Clear_ShouldReturnSuccess_WhenDatabaseIsCleared()
    {
        // Arrange
        var entities = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Test1", Locations = ["/test1"] },
            new() { Name = "Test2", Locations = ["/test2"] }
        }.AsQueryable();

        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.Provider)
            .Returns(entities.Provider);
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.Expression)
            .Returns(entities.Expression);
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.ElementType)
            .Returns(entities.ElementType);
        using var enumerator = entities.GetEnumerator();
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.GetEnumerator())
            .Returns(enumerator);

        _dbContextMock.Setup(x => x.AdvertisingPlatforms).Returns(_dbSetMock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _repository.Clear();

        // Assert
        Assert.True(result.IsSuccess);
        _dbSetMock.Verify(x
            => x.RemoveRange(It.IsAny<IEnumerable<AdvertisingPlatformEntity>>()), Times.Once);
        _dbContextMock.Verify(x 
            => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Clear_ShouldReturnFailure_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var entities = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Test1", Locations = ["/test1"] }
        }.AsQueryable();

        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.Provider)
            .Returns(entities.Provider);
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.Expression)
            .Returns(entities.Expression);
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.ElementType)
            .Returns(entities.ElementType);
        using var enumerator = entities.GetEnumerator();
        _dbSetMock.As<IQueryable<AdvertisingPlatformEntity>>()
            .Setup(m => m.GetEnumerator())
            .Returns(enumerator);

        _dbContextMock.Setup(x => x.AdvertisingPlatforms).Returns(_dbSetMock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Clear error"));

        // Act
        var result = await _repository.Clear();

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(ErrorType.ServerError, result.Error!.ErrorType);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Clear error")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllEntities_WhenDatabaseHasData()
    {
        // Arrange
        var entities = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Test1", Locations = ["/test1"] },
            new() { Name = "Test2", Locations = ["/test2"] }
        };

        _dbContextMock
            .Setup(x => x.AdvertisingPlatforms)
            .ReturnsDbSet(entities);

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count());
        _dbContextMock.Verify(x => x.AdvertisingPlatforms, Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnFailure_WhenDbExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(x => x.AdvertisingPlatforms)
            .Throws(new Exception("Database connection error"));

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(ErrorType.ServerError, result.Error!.ErrorType);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database connection error")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
    {
        // Arrange
        _dbContextMock.Setup(x => x.AdvertisingPlatforms)
            .ReturnsDbSet(new List<AdvertisingPlatformEntity>());

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}