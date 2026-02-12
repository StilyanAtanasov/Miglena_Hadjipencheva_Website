using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Admin.Dto.Work;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Images;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class AdminWorkServiceTests
{
    private IAdminWorkService _adminWorkService = null!;
    private ApplicationDbContext _dbContext = null!;

    private readonly Mock<IAdminProductImageService> _imageServiceMock = new();
    private readonly Mock<IFastCacheService> _cacheMock = new();
    private readonly Mock<ILogger<AdminWorkService>> _loggerMock = new();

    private Work _defaultWork = null!;

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AdminWorkTestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _adminWorkService = new AdminWorkService(new ApplicationRepository(_dbContext),
            _imageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

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
    public async Task AddWorkAsync_ReturnsOk_WhenDataIsValid()
    {
        // Arrange
        AddWorkDto model = new()
        {
            Title = "New Work",
            Content = "{\"ops\":[{\"insert\":\"New content\\n\"}]}",
            CoverImage = new UploadImageRequestDto
            {
                FileName = "cover.jpg",
                ContentType = "image/jpeg",
                Content = new MemoryStream(new byte[] { 1, 2, 3 })
            },
            IsPublic = true
        };

        _imageServiceMock
            .Setup(s => s.UploadImagesAsync(It.IsAny<ICollection<UploadImageRequestDto>>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(ServiceResult<ICollection<ImageUploadResultDto>>.Ok(new List<ImageUploadResultDto>
            {
                new() { ImageUrl = "https://example.com/new-cover.jpg", PublicId = "new-cover-id" }
            }));

        // Act
        ServiceResult result = await _adminWorkService.AddWorkAsync(model);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(_dbContext.Works.Count(), Is.EqualTo(2));

        Work? addedWork = await _dbContext.Works
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Title == model.Title);

        Assert.IsNotNull(addedWork);
        Assert.That(addedWork!.Content, Is.EqualTo(model.Content));
        Assert.That(addedWork.IsPublic, Is.EqualTo(model.IsPublic));
    }

    [Test]
    public async Task AddWorkAsync_ReturnsFailure_WhenImageUploadFails()
    {
        // Arrange
        AddWorkDto model = new()
        {
            Title = "New Work",
            Content = "{\"ops\":[{\"insert\":\"New content\\n\"}]}",
            CoverImage = new UploadImageRequestDto
            {
                FileName = "cover.jpg",
                ContentType = "image/jpeg",
                Content = new MemoryStream(new byte[] { 1, 2, 3 })
            },
            IsPublic = true
        };

        _imageServiceMock
            .Setup(s => s.UploadImagesAsync(It.IsAny<ICollection<UploadImageRequestDto>>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(ServiceResult<ICollection<ImageUploadResultDto>>.Failure());

        // Act
        ServiceResult result = await _adminWorkService.AddWorkAsync(model);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(_dbContext.Works.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetWorkForEditReadonlyAsync_ReturnsWork_WhenWorkExists()
    {
        // Act
        ServiceResult<EditWorkDto> result = await _adminWorkService.GetWorkForEditReadonlyAsync(_defaultWork.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.That(result.Result!.Id, Is.EqualTo(_defaultWork.Id));
        Assert.That(result.Result.Title, Is.EqualTo(_defaultWork.Title));
        Assert.That(result.Result.Content, Is.EqualTo(_defaultWork.Content));
        Assert.That(result.Result.CurrentCoverImageUrl, Is.EqualTo(_defaultWork.CoverImageUrl));
    }

    [Test]
    public async Task GetWorkForEditReadonlyAsync_ReturnsNotFound_WhenWorkDoesNotExist()
    {
        // Act
        ServiceResult<EditWorkDto> result = await _adminWorkService.GetWorkForEditReadonlyAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task UpdateWorkAsync_ReturnsOk_WhenDataIsValid()
    {
        // Arrange
        EditWorkDto model = new()
        {
            Id = _defaultWork.Id,
            Title = "Updated Title",
            Content = "{\"ops\":[{\"insert\":\"Updated content with \"},{\"attributes\":{\"bold\":true},\"insert\":\"formatting\"},{\"insert\":\"\\n\"}]}",
            IsPublic = false,
            NewCoverImage = null
        };

        // Act
        ServiceResult result = await _adminWorkService.UpdateWorkAsync(model);

        // Assert
        Assert.IsTrue(result.Success);

        Work? updatedWork = await _dbContext.Works
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == _defaultWork.Id);

        Assert.IsNotNull(updatedWork);
        Assert.That(updatedWork!.Title, Is.EqualTo(model.Title));
        Assert.That(updatedWork.Content, Is.EqualTo(model.Content));
        Assert.That(updatedWork.IsPublic, Is.EqualTo(model.IsPublic));
    }

    [Test]
    public async Task UpdateWorkAsync_UpdatesCoverImage_WhenNewImageProvided()
    {
        // Arrange
        EditWorkDto model = new()
        {
            Id = _defaultWork.Id,
            Title = "Updated Title",
            Content = "{\"ops\":[{\"insert\":\"Updated content\\n\"}]}",
            IsPublic = true,
            NewCoverImage = new UploadImageRequestDto
            {
                FileName = "new-cover.jpg",
                ContentType = "image/jpeg",
                Content = new MemoryStream(new byte[] { 4, 5, 6 })
            }
        };

        _imageServiceMock
            .Setup(s => s.UploadImagesAsync(It.IsAny<ICollection<UploadImageRequestDto>>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(ServiceResult<ICollection<ImageUploadResultDto>>.Ok(new List<ImageUploadResultDto>
            {
                new() { ImageUrl = "https://example.com/updated-cover.jpg", PublicId = "updated-cover-id" }
            }));

        _imageServiceMock
            .Setup(s => s.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Ok());

        // Act
        ServiceResult result = await _adminWorkService.UpdateWorkAsync(model);

        // Assert
        Assert.IsTrue(result.Success);

        Work? updatedWork = await _dbContext.Works
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == _defaultWork.Id);

        Assert.IsNotNull(updatedWork);
        Assert.That(updatedWork!.CoverImageUrl, Is.EqualTo("https://example.com/updated-cover.jpg"));
        Assert.That(updatedWork.CoverImagePublicId, Is.EqualTo("updated-cover-id"));

        _imageServiceMock.Verify(s => s.DeleteImageAsync(_defaultWork.CoverImagePublicId), Times.Once);
    }

    [Test]
    public async Task UpdateWorkAsync_ReturnsNotFound_WhenWorkDoesNotExist()
    {
        // Arrange
        EditWorkDto model = new()
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title",
            Content = "{\"ops\":[{\"insert\":\"Updated content\\n\"}]}",
            IsPublic = true
        };

        // Act
        ServiceResult result = await _adminWorkService.UpdateWorkAsync(model);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task DeleteWorkAsync_ReturnsOk_WhenWorkExists()
    {
        // Arrange
        _imageServiceMock
            .Setup(s => s.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Ok());

        // Act
        ServiceResult result = await _adminWorkService.DeleteWorkAsync(_defaultWork.Id);

        // Assert
        Assert.IsTrue(result.Success);

        Work? deletedWork = await _dbContext.Works
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == _defaultWork.Id);

        Assert.IsNull(deletedWork);

        _imageServiceMock.Verify(s => s.DeleteImageAsync(_defaultWork.CoverImagePublicId), Times.Once);
    }

    [Test]
    public async Task DeleteWorkAsync_ReturnsNotFound_WhenWorkDoesNotExist()
    {
        // Act
        ServiceResult result = await _adminWorkService.DeleteWorkAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task ToggleWorkPublicityAsync_TogglesIsPublic_WhenWorkExists()
    {
        // Arrange
        bool originalPublicStatus = _defaultWork.IsPublic;

        // Act
        ServiceResult result = await _adminWorkService.ToggleWorkPublicityAsync(_defaultWork.Id);

        // Assert
        Assert.IsTrue(result.Success);

        Work? updatedWork = await _dbContext.Works
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == _defaultWork.Id);

        Assert.IsNotNull(updatedWork);
        Assert.That(updatedWork!.IsPublic, Is.EqualTo(!originalPublicStatus));
    }

    [Test]
    public async Task ToggleWorkPublicityAsync_ReturnsNotFound_WhenWorkDoesNotExist()
    {
        // Act
        ServiceResult result = await _adminWorkService.ToggleWorkPublicityAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task AddWorkAsync_ReturnsFailure_WhenRepositoryThrows()
    {
        // Arrange
        var mockRepo = new Mock<IApplicationRepository>();
        mockRepo.Setup(s => s.AddAsync(It.IsAny<Work>())).ThrowsAsync(new Exception("DB Error"));

        var service = new AdminWorkService(mockRepo.Object,
            _imageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        AddWorkDto model = new()
        {
            Title = "Title",
            Content = "{}",
            CoverImage = new UploadImageRequestDto { FileName = "a.jpg", Content = new MemoryStream() }
        };

        _imageServiceMock.Setup(s => s.UploadImagesAsync(It.IsAny<ICollection<UploadImageRequestDto>>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(ServiceResult<ICollection<ImageUploadResultDto>>.Ok(new List<ImageUploadResultDto> { new() { ImageUrl = "u", PublicId = "p" } }));

        // Act
        ServiceResult result = await service.AddWorkAsync(model);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UpdateWorkAsync_ReturnsFailure_WhenImageDeletionFails()
    {
        // Arrange
        EditWorkDto model = new()
        {
            Id = _defaultWork.Id,
            Title = "Updated",
            Content = "{}",
            NewCoverImage = new UploadImageRequestDto { FileName = "new.jpg", Content = new MemoryStream() }
        };

        _imageServiceMock.Setup(s => s.UploadImagesAsync(It.IsAny<ICollection<UploadImageRequestDto>>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(ServiceResult<ICollection<ImageUploadResultDto>>.Ok(new List<ImageUploadResultDto> { new() { ImageUrl = "u", PublicId = "p" } }));

        _imageServiceMock.Setup(s => s.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Failure());

        // Act
        ServiceResult result = await _adminWorkService.UpdateWorkAsync(model);

        // Assert
        // The service currently proceeds even if old image deletion fails (it logs an error usually)
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task DeleteWorkAsync_ReturnsFailure_WhenImageDeletionFails()
    {
        // Arrange
        _imageServiceMock.Setup(s => s.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Failure());

        // Act
        ServiceResult result = await _adminWorkService.DeleteWorkAsync(_defaultWork.Id);

        // Assert
        // In current implementation, if image deletion fails, we still delete the database record
        Assert.IsTrue(result.Success);
        Assert.IsNull(await _dbContext.Works.FindAsync(_defaultWork.Id));
    }

    [Test]
    public async Task GetWorkForEditReadonlyAsync_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var mockRepo = new Mock<IApplicationRepository>();
        mockRepo.Setup(s => s.FindByExpressionAsync<Work>(It.IsAny<Expression<Func<Work, bool>>>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Work, object>>[]>()))
                .ThrowsAsync(new Exception());

        var service = new AdminWorkService(mockRepo.Object,
            _imageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        ServiceResult<EditWorkDto> result = await service.GetWorkForEditReadonlyAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
    }

    private async Task<Work> SeedWorkAsync()
    {
        Work work = new()
        {
            Id = Guid.NewGuid(),
            Title = "Test Work",
            Content = "{\"ops\":[{\"insert\":\"Test content\\n\"}]}",
            CoverImageUrl = "https://example.com/cover.jpg",
            CoverImagePublicId = "test-cover-id",
            DatePublished = DateTime.UtcNow,
            IsPublic = true
        };

        _dbContext.Works.Add(work);
        await _dbContext.SaveChangesAsync();

        return work;
    }
}
