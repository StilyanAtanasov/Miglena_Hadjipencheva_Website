using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Work;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class WorkServiceTests
{
    private IWorkService _workService = null!;
    private ApplicationDbContext _dbContext = null!;
    private readonly Mock<ILogger<WorkService>> _loggerMock = new();
    private readonly Mock<IFastCacheService> _cacheServiceMock = new();

    private Work _defaultWork = null!;

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("WorkTestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _workService = new WorkService(new ApplicationRepository(_dbContext), _cacheServiceMock.Object, _loggerMock.Object);

        // Arrange
        _defaultWork = await SeedWorkAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void Test_SetUp()
    {
        Assert.Pass();
    }

    [Test]
    public async Task GetWorkDetailsAsync_ReturnsWork_WhenWorkExists()
    {
        // Act
        ServiceResult<WorkDetailsDto> result = await _workService.GetWorkDetailsAsync(_defaultWork.Id, false);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.That(result.Result!.Id, Is.EqualTo(_defaultWork.Id));
        Assert.That(result.Result.Title, Is.EqualTo(_defaultWork.Title));
        Assert.That(result.Result.Content, Is.EqualTo(_defaultWork.Content));
    }

    [Test]
    public async Task GetWorkDetailsAsync_ReturnsNotFound_WhenWorkDoesNotExist()
    {
        // Act
        ServiceResult<WorkDetailsDto> result = await _workService.GetWorkDetailsAsync(Guid.NewGuid(), false);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task GetWorkDetailsAsync_RespectsPublicity()
    {
        // Arrange
        Work privateWork = await SeedWorkAsync("Private", false);

        // Act
        ServiceResult<WorkDetailsDto> publicResult = await _workService.GetWorkDetailsAsync(privateWork.Id, false);
        ServiceResult<WorkDetailsDto> adminResult = await _workService.GetWorkDetailsAsync(privateWork.Id, true);

        // Assert
        Assert.IsFalse(publicResult.Found);
        Assert.IsTrue(adminResult.Success);
    }

    [Test]
    public async Task GetPagedWorksAsync_ReturnsCorrectPage_AndRespectsSearch()
    {
        // Arrange
        await SeedWorkAsync("Title A", true);
        await SeedWorkAsync("Title B", true);
        await SeedWorkAsync("Other", true);

        // Act
        ICollection<WorkCardDto> searchResult = await _workService.GetPagedWorksAsync(false, 1, "Title");
        ICollection<WorkCardDto> allResult = await _workService.GetPagedWorksAsync(false, 1);

        // Assert
        Assert.That(searchResult.Count, Is.EqualTo(2));
        Assert.That(allResult.Count, Is.EqualTo(4)); // 1 from Setup + 3 here
    }

    [Test]
    public async Task GetWorksCountAsync_ReturnsCorrectCount_AndRespectsIsAdmin()
    {
        // Arrange
        await SeedWorkAsync("Public", true);
        await SeedWorkAsync("Private", false);

        // Act
        int publicCount = await _workService.GetWorksCountAsync(false);
        int adminCount = await _workService.GetWorksCountAsync(true);

        // Assert
        Assert.That(publicCount, Is.EqualTo(2)); // Default + 1 Public
        Assert.That(adminCount, Is.EqualTo(3));  // Default + 1 Public + 1 Private
    }

    private async Task<Work> SeedWorkAsync(string title = "Test Work", bool isPublic = true)
    {
        Work work = new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = "{\"ops\":[{\"insert\":\"Test content\\n\"}]}",
            CoverImageUrl = "https://example.com/cover.jpg",
            CoverImagePublicId = "test-cover-id",
            DatePublished = DateTime.UtcNow,
            IsPublic = isPublic
        };

        _dbContext.Works.Add(work);
        await _dbContext.SaveChangesAsync();

        return work;
    }
}
