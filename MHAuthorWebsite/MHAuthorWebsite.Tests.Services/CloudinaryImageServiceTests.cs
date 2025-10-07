using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MHAuthorWebsite.Tests.Services;

public class CloudinaryImageServiceTests
{
    private IImageService _imageService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Mock<ICloudinaryService> _cloudinaryMock = null!;

    private Image _defaultImage = null!;
    private Product _defaultProduct = null!;

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductTestDb")
            .Options;

        _cloudinaryMock = new Mock<ICloudinaryService>();

        _dbContext = new ApplicationDbContext(options);
        _imageService = new CloudinaryImageService(_cloudinaryMock.Object, new ApplicationRepository(_dbContext));

        // Arrange
        _defaultImage = await SeedImageAsync();
        _defaultProduct = await SeedProductAsync(_defaultImage);
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

    /* ----- UploadImageWithPreviewAsync ----- */

    [Test]
    public async Task UploadImageWithPreviewAsync_ReturnsOk_WhenAllDataValid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>()
        {
            CreateInMemoryFormFile("image-1.jpeg", "image/jpeg", "Fake image data"),
            CreateInMemoryFormFile("image-2.jpeg", "image/jpeg", "Another fake image data")
        };

        int callCount = 1;
        _cloudinaryMock
            .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(() => new ImageUploadResult
            {
                PublicId = $"publicId{callCount}",
                SecureUrl = new Uri($"https://example.com/image{callCount++}.jpg")
            });

        // Act
        ServiceResult<ICollection<ProductImageUploadResultDto>> sr = await
            _imageService.UploadImageWithPreviewAsync(formFiles, 1);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.IsNotNull(sr.Result);
        Assert.IsNotEmpty(sr.Result!);
        Assert.That(sr.Result!.Count, Is.EqualTo(2));
        Assert.That(sr.Result!.ElementAt(1).PublicId, Is.EqualTo("publicId2"));
        Assert.That(sr.Result!.ElementAt(0).OriginalUrl, Is.EqualTo("https://example.com/image1.jpg"));
    }

    [Test]
    public async Task UploadImageWithPreviewAsync_ReturnsFailure_WhenImageCollectionIsInvalid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>(); // Empty collection

        // Act
        ServiceResult<ICollection<ProductImageUploadResultDto>> sr = await
            _imageService.UploadImageWithPreviewAsync(formFiles, 1);

        // Assert
        Assert.IsFalse(sr.Success);
    }

    [Test]
    public async Task UploadImageWithPreviewAsync_ReturnsFailure_WhenTitleImageIndexIsInvalid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>()
        {
            CreateInMemoryFormFile("image-1.jpeg", "image/jpeg", "Fake image data"),
            CreateInMemoryFormFile("image-2.jpeg", "image/jpeg", "Another fake image data")
        };

        // Act
        ServiceResult<ICollection<ProductImageUploadResultDto>> sr1 = await
            _imageService.UploadImageWithPreviewAsync(formFiles, -1); // Negative index

        ServiceResult<ICollection<ProductImageUploadResultDto>> sr2 = await
            _imageService.UploadImageWithPreviewAsync(formFiles, 100); // Index out of range

        // Assert
        Assert.IsFalse(sr1.Success);
        Assert.IsFalse(sr2.Success);
    }

    /* ----- LinkImagesToProductAsync ----- */

    [Test]
    public async Task LinkImagesToProductAsync_ReturnsFailure_WhenImageCollectionIsInvalid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>(); // Empty collection

        // Act
        ServiceResult<Guid?> sr = await _imageService.LinkImagesToProductAsync(formFiles, 0, Guid.NewGuid());

        // Assert
        Assert.IsFalse(sr.Success);
    }

    [Test]
    public async Task LinkImagesToProductAsync_ReturnsFailure_WhenTitleImageIndexIsInvalid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>()
        {
            CreateInMemoryFormFile("image-1.jpeg", "image/jpeg", "Fake image data"),
            CreateInMemoryFormFile("image-2.jpeg", "image/jpeg", "Another fake image data")
        };

        // Act
        ServiceResult<Guid?> sr1 = await
            _imageService.LinkImagesToProductAsync(formFiles, -1, Guid.NewGuid()); // Negative index

        ServiceResult<Guid?> sr2 = await
            _imageService.LinkImagesToProductAsync(formFiles, 100, Guid.NewGuid()); // Index out of range

        // Assert
        Assert.IsFalse(sr1.Success);
        Assert.IsFalse(sr2.Success);
    }

    [Test]
    public async Task LinkImagesToProductAsync_ReturnsFailure_WhenProductIdIsInvalid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>()
        {
            CreateInMemoryFormFile("image-1.jpeg", "image/jpeg", "Fake image data"),
            CreateInMemoryFormFile("image-2.jpeg", "image/jpeg", "Another fake image data")
        };

        // Act
        ServiceResult<Guid?> sr = await
            _imageService.LinkImagesToProductAsync(formFiles, 1, Guid.NewGuid());

        // Assert
        Assert.IsFalse(sr.Success);
    }

    [Test]
    public async Task LinkImagesToProductAsync_ReturnsOk_WhenAllDataValid()
    {
        // Arrange
        ICollection<IFormFile> formFiles = new HashSet<IFormFile>
        {
            CreateInMemoryFormFile("image-1.jpeg", "image/jpeg", "Fake image data"),
            CreateInMemoryFormFile("image-2.jpeg", "image/jpeg", "Another fake image data")
        };

        int callCount = 1;
        _cloudinaryMock
            .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(() => new ImageUploadResult
            {
                PublicId = $"publicId{callCount}",
                SecureUrl = new Uri($"https://example.com/image{callCount++}.jpg")
            });

        int titleImageIndex = 1;

        // Act
        ServiceResult<Guid?> sr = await
            _imageService.LinkImagesToProductAsync(formFiles, titleImageIndex, _defaultProduct.Id);

        // Assert
        Image titleImage = _dbContext.Images
                .AsNoTracking()
                .First(i => i.PublicId == $"publicId{titleImageIndex + 1}"); // The publicId indexer starts at 1

        Assert.IsTrue(sr.Success);
        Assert.IsNotNull(sr.Result);
        Assert.That(sr.Result!.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.That(_dbContext.Images.Count, Is.EqualTo(3));
        Assert.That(_defaultProduct.Images.Count, Is.EqualTo(3));
        Assert.That(sr.Result.Value, Is.EqualTo(titleImage.Id));
    }

    /* ----- DeleteImageAsync ----- */

    [Test]
    public async Task DeleteImageAsync_ReturnsOk_WhenImageExists()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        // Act
        ServiceResult result = await _imageService.DeleteImageAsync(_defaultImage.PublicId);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task DeleteImageAsync_ReturnsFailure_OnError()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "error" });

        // Act
        ServiceResult result = await _imageService.DeleteImageAsync(_defaultImage.PublicId);

        // Assert
        Assert.IsFalse(result.Success);
    }

    /* ----- DeleteProductImageByIdAsync ----- */
    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsNotFound_OnInvalidImageId()
    {
        // Act
        ServiceResult result = await _imageService.DeleteProductImageByIdAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsOk_WhenAllOperationsSucceed()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        // Act
        ServiceResult result = await _imageService.DeleteProductImageByIdAsync(_defaultImage.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(_dbContext.Images.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsOk_WhenImageHasThumbnail()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        _defaultImage.IsThumbnail = true;
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _imageService.DeleteProductImageByIdAsync(_defaultImage.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(_dbContext.Images.Count, Is.EqualTo(0));
    }


    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsFailure_OnDeleteThumbnailError_ButImagesAreDeletedFromDb()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "error" });

        _defaultImage.IsThumbnail = true;
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _imageService.DeleteProductImageByIdAsync(_defaultImage.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(_dbContext.Images.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsFailure_OnDeleteImageAsyncError_ButImagesAreDeletedFromDb()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "error" });

        // Act
        ServiceResult result = await _imageService.DeleteProductImageByIdAsync(_defaultImage.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(_dbContext.Images.Count, Is.EqualTo(0));
    }

    /* ----- UpdateProductTitleImageAsync ----- */

    [Test]
    public async Task UpdateProductTitleImageAsync_ReturnsFailure_WhenImageIdIsInvalid()
    {
        // Act
        ServiceResult result = await _imageService.UpdateProductTitleImageAsync(_defaultProduct.Id, new Guid());

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UpdateProductTitleImageAsync_ReturnsOk_WhenCurrentTitleImageIsNotFound()
    {
        // Arrange 
        _cloudinaryMock
            .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(() => new ImageUploadResult
            {
                PublicId = "thumbnailId",
                SecureUrl = new Uri("https://example.com/thumbnail.jpg")
            });

        // Act
        ServiceResult result = await _imageService.UpdateProductTitleImageAsync(_defaultProduct.Id, _defaultImage.Id);

        // Assert
        Image? updatedImage = await _dbContext.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == _defaultImage.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(updatedImage);
        Assert.That(updatedImage!.IsThumbnail, Is.True);
        Assert.That(updatedImage.ThumbnailPublicId, Is.EqualTo("thumbnailId"));
        Assert.That(updatedImage.ThumbnailUrl, Is.EqualTo("https://example.com/thumbnail.jpg"));
    }

    [Test]
    public async Task UpdateProductTitleImageAsync_ReturnsOk_WhenCurrentTitleImageIsFound()
    {
        // Arrange 
        _cloudinaryMock
            .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(() => new ImageUploadResult
            {
                PublicId = "thumbnailId",
                SecureUrl = new Uri("https://example.com/thumbnail.jpg")
            });

        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        _defaultImage.IsThumbnail = true;
        _defaultImage.ThumbnailPublicId = "oldThumbnailId";
        _defaultImage.ThumbnailUrl = "https://example.com/old-thumbnail.jpg";

        Image newImage = new()
        {
            ImageUrl = "https://example.com/new-image.jpg",
            PublicId = "new-image-id",
            AltText = "New Image",
            ProductId = _defaultProduct.Id,
        };

        _dbContext.Images.Add(newImage);
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _imageService.UpdateProductTitleImageAsync(_defaultProduct.Id, newImage.Id);

        // Assert
        Image? oldImage = await _dbContext.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == _defaultImage.Id);

        Image? newImageUpdated = await _dbContext.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == newImage.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(oldImage);
        Assert.IsNotNull(newImageUpdated);
        Assert.That(oldImage!.IsThumbnail, Is.False);
        Assert.That(oldImage.ThumbnailPublicId, Is.Null);
        Assert.That(oldImage.ThumbnailUrl, Is.Null);
        Assert.That(newImageUpdated!.IsThumbnail, Is.True);
        Assert.That(newImageUpdated.ThumbnailPublicId, Is.Not.Null);
        Assert.That(newImageUpdated.ThumbnailUrl, Is.Not.Null);
    }

    /* ----- Helper methods ----- */

    private async Task<Image> SeedImageAsync()
    {
        Image image = new()
        {
            Id = Guid.NewGuid(),
            ImageUrl = "https://example.com/test-image.jpg",
            PublicId = "test-image-id",
            AltText = "Test Image"
        };

        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();
        return image;
    }

    private async Task<Product> SeedProductAsync(Image image)
    {
        ProductType productType = new()
        {
            Id = 1,
            Name = "Books"
        };

        Product product = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            StockQuantity = 10,
            IsDeleted = false,
            IsPublic = true,
            ProductType = productType,
            Price = 10.99m,
            Images = new List<Image> { image },
            Attributes = new List<ProductAttribute>
            {
                new ()
                {
                    Id = 1,
                    Key = "Author",
                    Value = "John Doe"
                }
            },
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }

    private IFormFile CreateInMemoryFormFile(string fileName, string contentType, string content)
    {
        byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        MemoryStream stream = new MemoryStream(contentBytes);

        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}