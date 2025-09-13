using Application.Enums;
using Application.Interfaces;
using Application.Services;
using Application.Utils;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AdvertisingTests;

public class AdvertisingServiceTests
{
    private readonly Mock<IAdvertisingPlatformRepository> _repositoryMock;
    private readonly AdvertisingService _advertisingService;

    public AdvertisingServiceTests()
    {
        _repositoryMock = new Mock<IAdvertisingPlatformRepository>();
        _advertisingService = new AdvertisingService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnEmptyList_WhenLocationIsEmpty()
    {
        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnFailure_WhenRepositoryFails()
    {
        // Arrange
        var expectedError = new Error(ErrorType.ServerError, "Database error");
        _repositoryMock.Setup(r => r.GetAll())
            .ReturnsAsync(Result<IEnumerable<AdvertisingPlatformEntity>>.Failure(expectedError));

        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("/ru/msk");

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnCorrectPlatforms_ForMoscow()
    {
        // Arrange
        var platforms = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Яндекс.Директ", Locations = ["/ru"] },
            new() { Name = "Ревдинский рабочий", Locations = ["/ru/svrd/revda", "/ru/svrd/pervik"] },
            new() { Name = "Газета уральских москвичей", Locations = ["/ru/msk", "/ru/permobl", "/ru/chelobl"] },
            new() { Name = "Крутая реклама", Locations = ["/ru/svrd"] }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .ReturnsAsync(Result<IEnumerable<AdvertisingPlatformEntity>>.Success(platforms));

        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("/ru/msk");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Газета уральских москвичей", result.Value!);
        Assert.Contains("Яндекс.Директ", result.Value!);
        Assert.Equal(2, result.Value!.Count());
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnCorrectPlatforms_ForSverdlovskRegion()
    {
        // Arrange
        var platforms = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Яндекс.Директ", Locations = ["/ru"] },
            new() { Name = "Ревдинский рабочий", Locations = ["/ru/svrd/revda", "/ru/svrd/pervik"] },
            new() { Name = "Газета уральских москвичей", Locations = ["/ru/msk", "/ru/permobl", "/ru/chelobl"] },
            new() { Name = "Крутая реклама", Locations = ["/ru/svrd"] }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .ReturnsAsync(Result<IEnumerable<AdvertisingPlatformEntity>>.Success(platforms));

        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("/ru/svrd");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Яндекс.Директ", result.Value!);
        Assert.Contains("Крутая реклама", result.Value!);
        Assert.Equal(2, result.Value!.Count());
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnCorrectPlatforms_ForRevda()
    {
        // Arrange
        var platforms = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Яндекс.Директ", Locations = ["/ru"] },
            new() { Name = "Ревдинский рабочий", Locations = ["/ru/svrd/revda", "/ru/svrd/pervik"] },
            new() { Name = "Газета уральских москвичей", Locations = ["/ru/msk", "/ru/permobl", "/ru/chelobl"] },
            new() { Name = "Крутая реклама", Locations = ["/ru/svrd"] }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .ReturnsAsync(Result<IEnumerable<AdvertisingPlatformEntity>>.Success(platforms));

        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("/ru/svrd/revda");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Яндекс.Директ", result.Value!);
        Assert.Contains("Ревдинский рабочий", result.Value!);
        Assert.Contains("Крутая реклама", result.Value!);
        Assert.Equal(3, result.Value!.Count());
    }

    [Fact]
    public async Task GetAdvertisingByLocation_ShouldReturnCorrectPlatforms_ForRoot()
    {
        // Arrange
        var platforms = new List<AdvertisingPlatformEntity>
        {
            new() { Name = "Яндекс.Директ", Locations = ["/ru"] },
            new() { Name = "Ревдинский рабочий", Locations = ["/ru/svrd/revda", "/ru/svrd/pervik"] },
            new() { Name = "Газета уральских москвичей", Locations = ["/ru/msk", "/ru/permobl", "/ru/chelobl"] },
            new() { Name = "Крутая реклама", Locations = ["/ru/svrd"] }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .ReturnsAsync(Result<IEnumerable<AdvertisingPlatformEntity>>.Success(platforms));

        // Act
        var result = await _advertisingService.GetAdvertisingByLocation("/ru");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Яндекс.Директ", result.Value!);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task UploadAdvertising_ShouldReturnFailure_WhenFileIsNull()
    {
        // Act
        var result = await _advertisingService.UploadAdvertising(null!);

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.Error!.ErrorType);
    }

    [Fact]
    public async Task UploadAdvertising_ShouldReturnFailure_WhenFileIsEmpty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _advertisingService.UploadAdvertising(fileMock.Object);

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.Error!.ErrorType);
    }

    [Fact]
    public async Task UploadAdvertising_ShouldAddPlatforms_WhenFileIsValid()
    {
        // Arrange
        var fileMock = CreateFormFileMock("Яндекс.Директ:/ru\nКрутая реклама:/ru/svrd");

        _repositoryMock.Setup(r => r.Clear()).ReturnsAsync(Result.Success());
        _repositoryMock.Setup(r => r.Add(It.IsAny<AdvertisingPlatformEntity>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _advertisingService.UploadAdvertising(fileMock.Object);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r 
            => r.Add(It.Is<AdvertisingPlatformEntity>(p 
                => p.Name == "Яндекс.Директ")), Times.Once);
        _repositoryMock.Verify(r 
            => r.Add(It.Is<AdvertisingPlatformEntity>(p 
                => p.Name == "Крутая реклама")), Times.Once);
    }

    [Fact]
    public async Task UploadAdvertising_ShouldSkipInvalidLines_WhenFileContainsThem()
    {
        // Arrange
        var fileMock = CreateFormFileMock("Яндекс.Директ:/ru" +
                                          "\nInvalidLine" +
                                          "\nКрутая реклама:/ru/svrd" +
                                          "\nAnotherInvalidLine:");

        _repositoryMock.Setup(r => r.Clear()).ReturnsAsync(Result.Success());
        _repositoryMock.Setup(r => r.Add(It.IsAny<AdvertisingPlatformEntity>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _advertisingService.UploadAdvertising(fileMock.Object);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r 
            => r.Add(It.IsAny<AdvertisingPlatformEntity>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UploadAdvertising_ShouldReturnFailure_WhenClearFails()
    {
        // Arrange
        var fileMock = CreateFormFileMock("Яндекс.Директ:/ru");
        var expectedError = new Error(ErrorType.ServerError, "Clear failed");

        _repositoryMock.Setup(r => r.Clear())
            .ReturnsAsync(Result.Failure(expectedError));

        // Act
        var result = await _advertisingService.UploadAdvertising(fileMock.Object);

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task UploadAdvertising_ShouldReturnFailure_WhenAddFails()
    {
        // Arrange
        var fileMock = CreateFormFileMock("Яндекс.Директ:/ru");
        var expectedError = new Error(ErrorType.ServerError, "Add failed");

        _repositoryMock.Setup(r => r.Clear()).ReturnsAsync(Result.Success());
        _repositoryMock.Setup(r => r.Add(It.IsAny<AdvertisingPlatformEntity>()))
            .ReturnsAsync(Result.Failure(expectedError));

        // Act
        var result = await _advertisingService.UploadAdvertising(fileMock.Object);

        // Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
    }

    private Mock<IFormFile> CreateFormFileMock(string content)
    {
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        
        writer.Write(content);
        writer.Flush();
        
        ms.Position = 0;
        
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        
        return fileMock;
    }
}