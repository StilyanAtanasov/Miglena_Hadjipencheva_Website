using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MHAuthorWebsite.Tests.Services;

public class CloudinaryImageServiceTests
{
    private IAdminProductImageService _adminProductImageService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Mock<ICloudinaryService> _cloudinaryMock = null!;
    private Mock<IImageService> _imageServiceMock = null!;

    private ProductImage _defaultImage = null!;
    private Product _defaultProduct = null!;

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductTestDb")
            .Options;

        _cloudinaryMock = new Mock<ICloudinaryService>();
        _imageServiceMock = new Mock<IImageService>();

        _dbContext = new ApplicationDbContext(options);
        _adminProductImageService = new CloudinaryAdminProductImageService(new ApplicationRepository(_dbContext), _imageServiceMock.Object, _cloudinaryMock.Object);

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
            _adminProductImageService.UploadImageWithPreviewAsync(formFiles, 1);

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
            _adminProductImageService.UploadImageWithPreviewAsync(formFiles, 1);

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
            _adminProductImageService.UploadImageWithPreviewAsync(formFiles, -1); // Negative index

        ServiceResult<ICollection<ProductImageUploadResultDto>> sr2 = await
            _adminProductImageService.UploadImageWithPreviewAsync(formFiles, 100); // Index out of range

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
        ServiceResult<Guid?> sr = await _adminProductImageService.LinkImagesToProductAsync(formFiles, 0, Guid.NewGuid());

        // Assert
        Assert.That(sr.Success, Is.False);
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
            _adminProductImageService.LinkImagesToProductAsync(formFiles, -1, Guid.NewGuid()); // Negative index

        ServiceResult<Guid?> sr2 = await
            _adminProductImageService.LinkImagesToProductAsync(formFiles, 100, Guid.NewGuid()); // Index out of range

        // Assert
        Assert.That(sr1.Success, Is.False);
        Assert.That(sr2.Success, Is.False);
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
            _adminProductImageService.LinkImagesToProductAsync(formFiles, 1, Guid.NewGuid());

        // Assert
        Assert.That(sr.Success, Is.False);
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
            _adminProductImageService.LinkImagesToProductAsync(formFiles, titleImageIndex, _defaultProduct.Id);

        // Assert
        ProductImage titleImage = _dbContext.ProductsImages
                .AsNoTracking()
                .First(i => i.PublicId == $"publicId{titleImageIndex + 1}"); // The publicId indexer starts at 1

        Assert.That(sr.Success, Is.True);
        Assert.That(sr.Result, Is.Not.Null);
        Assert.That(sr.Result!.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.That(_dbContext.ProductsImages.Count, Is.EqualTo(3));
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
        ServiceResult result = await _adminProductImageService.DeleteImageAsync(_defaultImage.PublicId);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task DeleteImageAsync_ReturnsFailure_OnError()
    {
        // Arrange
        _cloudinaryMock
            .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "error" });

        // Act
        ServiceResult result = await _adminProductImageService.DeleteImageAsync(_defaultImage.PublicId);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    /* ----- DeleteProductImageByIdAsync ----- */
    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsNotFound_OnInvalidImageId()
    {
        // Act
        ServiceResult result = await _adminProductImageService.DeleteProductImageByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsOk_WhenAllOperationsSucceed()
    {
        // Arrange
        _imageServiceMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Ok());

        ProductImage newImage = new()
        {
            ImageUrl = "https://example.com/new-image.jpg",
            PublicId = "new-image-id",
            AltText = "New Image",
            ProductId = _defaultProduct.Id,
        };

        _dbContext.ProductsImages.Add(newImage);
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _adminProductImageService.DeleteProductImageByIdAsync(newImage.Id);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(_dbContext.ProductsImages.IgnoreQueryFilters().Count, Is.EqualTo(2)); // Only the default images - product image + thumbnail
        Assert.That(!_dbContext.ProductsImages.Any(pi => pi.Id == newImage.Id));
    }

    [Test]
    public async Task DeleteProductImageByIdAsync_ReturnsFailure_OnDeleteImageAsyncError_ButImagesAreDeletedFromDb()
    {
        // Arrange
        _imageServiceMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Failure());

        ProductImage newImage = new()
        {
            ImageUrl = "https://example.com/new-image.jpg",
            PublicId = "new-image-id",
            AltText = "New Image",
            ProductId = _defaultProduct.Id,
        };

        _dbContext.ProductsImages.Add(newImage);
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _adminProductImageService.DeleteProductImageByIdAsync(newImage.Id);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(_dbContext.ProductsImages.IgnoreQueryFilters().Count, Is.EqualTo(2)); // Only the default images - product image + thumbnail
        Assert.That(!_dbContext.ProductsImages.Any(pi => pi.Id == newImage.Id));
    }

    /* ----- UpdateProductTitleImageAsync ----- */

    [Test]
    public async Task UpdateProductTitleImageAsync_ReturnsFailure_WhenImageIdIsInvalid()
    {
        // Act
        ServiceResult result = await _adminProductImageService.UpdateProductTitleImageAsync(_defaultProduct.Id, new Guid());

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task UpdateProductTitleImageAsync_ReturnsOk_OnCorrectData()
    {
        // Arrange 
        _cloudinaryMock
            .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(() => new ImageUploadResult
            {
                PublicId = "thumbnail-public-id",
                SecureUrl = new Uri("https://example.com/thumbnail.jpg")
            });

        _imageServiceMock
            .Setup(i => i.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Ok());

        ProductImage newImage = new()
        {
            ImageUrl = "https://example.com/new-image.jpg",
            PublicId = "new-image-id",
            AltText = "New Image",
            ProductId = _defaultProduct.Id,
        };

        _dbContext.ProductsImages.Add(newImage);
        await _dbContext.SaveChangesAsync();

        Guid oldThumbnailId = _defaultProduct.Thumbnail.ImageId;

        // Act
        ServiceResult result = await _adminProductImageService.UpdateProductTitleImageAsync(_defaultProduct.Id, newImage.Id);

        // Assert
        ProductImage? oldThumbnail = await _dbContext.ProductsImages
             .AsNoTracking()
             .FirstOrDefaultAsync(i => i.Id == oldThumbnailId);

        Assert.That(result.Success, Is.True);
        Assert.That(oldThumbnail, Is.Null);
        Assert.That(_dbContext.ProductsImages.Count, Is.EqualTo(3)); // Two original images(_defaultImage & newImage) + only one new thumbnail
        Assert.That(_defaultProduct.Thumbnail.ImageId, Is.Not.EqualTo(oldThumbnailId));
    }

    /* ----- Helper methods ----- */

    private async Task<ProductImage> SeedImageAsync()
    {
        ProductImage image = new()
        {
            Id = Guid.NewGuid(),
            ImageUrl = "https://example.com/test-image.jpg",
            PublicId = "test-image-id",
            AltText = "Test Image"
        };

        _dbContext.ProductsImages.Add(image);
        await _dbContext.SaveChangesAsync();
        return image;
    }

    private async Task<Product> SeedProductAsync(ProductImage image)
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
            Images = new List<ProductImage> { image },
            Attributes = new List<ProductAttribute>
            {
                new ()
                {
                    Id = 1,
                    Key = "Author",
                    Value = "John Doe"
                }
            },
            Thumbnail = new ProductThumbnail
            {
                Image = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = "https://example.com/thumbnail.jpg",
                    PublicId = "thumbnail-id",
                    AltText = "Thumbnail Image"
                },
                ImageOriginalId = image.Id
            },
            Weight = 0.5m
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }

    private IFormFile CreateInMemoryFormFile(string fileName, string contentType, string content)
    {
        byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        MemoryStream stream = new(contentBytes);

        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}