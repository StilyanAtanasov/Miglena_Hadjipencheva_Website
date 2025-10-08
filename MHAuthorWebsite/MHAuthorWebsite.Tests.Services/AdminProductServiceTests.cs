using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class AdminProductServiceTests
{
    private IAdminProductService _adminProductService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private Product _defaultProduct = null!;

    [SetUp]
    public async Task Setup()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AdminProductTestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _adminProductService = new AdminProductService(new ApplicationRepository(_dbContext), _userManagerMock.Object);

        // Arrange
        _defaultProduct = await SeedProductAsync();
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
    public async Task AddProductAsync_ReturnsOk_WhenImagesAndAttributesArePresent()
    {
        // Arrange
        AddProductDto model = new()
        {
            Name = "New Product",
            Description = "New Description",
            Price = 20.99m,
            ProductTypeId = _defaultProduct.ProductTypeId,
            StockQuantity = 5,
            ImageUrls = new List<ImageUploadResultDto>
            {
                new ()
                {
                    PublicId = "public-id",
                    ImageUrl = "image.jpg",
                }
            },
            Attributes = new List<AttributeValueForm>
            {
                new ()
                {
                    Key = "Author",
                    Value = "Fiction",
                    AttributeDefinitionId = _defaultProduct.ProductType.AttributeDefinitions.First().Id,
                }
            }
        };

        // Act
        ServiceResult result = await _adminProductService.AddProductAsync(model);

        // Assert
        Product? addedProduct = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters() // To include non-public products - added products are private by default
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Name == model.Name);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(addedProduct);
        Assert.AreEqual(model.Name, addedProduct!.Name);
        Assert.That(addedProduct.Images.Count, Is.EqualTo(1));
        Assert.That(addedProduct.Images.First().PublicId, Is.EqualTo("public-id"));
        Assert.That(addedProduct.Attributes.Count, Is.EqualTo(1));
        Assert.That(addedProduct.Attributes.First().Value, Is.EqualTo("Fiction"));
        Assert.IsFalse(addedProduct.IsDeleted);
    }

    [Test]
    public async Task AddProductAsync_ReturnsFailure_OnError()
    {
        // Arrange
        AddProductDto model = new()
        {
            Name = "New Product",
            Description = "New Description",
            Price = 20.99m,
            ProductTypeId = 1,
            StockQuantity = 5
        };

        Mock<IApplicationRepository> repositoryMock = new();
        repositoryMock
                  .Setup(r => r.AddAsync(It.IsAny<Product>()))
                  .Throws(new Exception("Db error"));

        _adminProductService = new AdminProductService(repositoryMock.Object, _userManagerMock.Object);

        // Act
        ServiceResult result = await _adminProductService.AddProductAsync(model);

        // Assert
        Product? addedProduct = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters() // To include non-public products - added products are private by default
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Name == model.Name);

        Assert.IsFalse(result.Success);
        Assert.IsNull(addedProduct);
        Assert.That(_dbContext.Products.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetProductForEditAsync_ReturnsViewModel_OnExistingProduct()
    {
        // Act
        ServiceResult<EditProductFormViewModel> sr = await _adminProductService.GetProductForEditAsync(_defaultProduct.Id);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.IsTrue(sr.HasResult());
        Assert.AreEqual(_defaultProduct.Id, sr.Result!.Id);
        Assert.That(sr.Result.Images.Count, Is.EqualTo(1));
        Assert.That(sr.Result.Attributes.Count, Is.EqualTo(1));
        Assert.That(sr.Result.Images.First().Id, Is.EqualTo(_defaultProduct.Images.First().Id));
        Assert.That(sr.Result.Attributes.First().Key, Is.EqualTo(_defaultProduct.Attributes.First().Key));
    }

    [Test]
    public async Task GetProductForEditAsync_ReturnsViewModel_OnNonExistingProduct()
    {
        // Act
        ServiceResult<EditProductFormViewModel> sr = await _adminProductService.GetProductForEditAsync(new Guid());

        // Assert
        Assert.IsFalse(sr.Success);
        Assert.IsFalse(sr.HasResult());
        Assert.IsFalse(sr.Found);
    }

    [Test]
    public async Task UpdateProductAsync_ReturnsOk_WhenProductIsUpdated()
    {
        // Arrange
        EditProductFormViewModel model = new()
        {
            Id = _defaultProduct.Id,
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 15.99m,
            StockQuantity = 8,
            ProductTypeName = _defaultProduct.ProductType.Name,
            Attributes = _defaultProduct.Attributes.Select(_ => new AttributeValueForm
            {
                Value = "New value",
            }).ToArray()
        };

        // Act
        ServiceResult result = await _adminProductService.UpdateProductAsync(model);

        // Assert
        Product? updatedProduct = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters() // To include non-public products - newly added products are private by default
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(updatedProduct);
        Assert.AreEqual(model.Name, updatedProduct!.Name);
        Assert.AreEqual(model.Description, updatedProduct.Description);
        Assert.AreEqual(model.Price, updatedProduct.Price);
        Assert.AreEqual(model.StockQuantity, updatedProduct.StockQuantity);
        Assert.That(updatedProduct.Attributes.Count, Is.EqualTo(1));
        Assert.That(updatedProduct.Attributes.First().Value, Is.EqualTo("New value"));
    }

    [Test]
    public async Task UpdateProductAsync_Returns404_WhenProductIsNotFound()
    {
        // Arrange
        EditProductFormViewModel model = new()
        {
            Id = new Guid(),
        };

        // Act
        ServiceResult result = await _adminProductService.UpdateProductAsync(model);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task DeleteProductAsync_ReturnsOk_WhenProductExists()
    {
        // Act
        ServiceResult result = await _adminProductService.DeleteProductAsync(_defaultProduct.Id);

        // Assert
        Product? deletedProduct = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _defaultProduct.Id);

        Product? deletedProductNotNull = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == _defaultProduct.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNull(deletedProduct);

        Assert.IsNotNull(deletedProductNotNull);
        Assert.IsTrue(deletedProductNotNull!.IsDeleted);
    }

    [Test]
    public async Task DeleteProductAsync_Returns404_WhenProductDoesNotExist()
    {
        // Act
        ServiceResult result = await _adminProductService.DeleteProductAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task DeleteProductAsync_ReturnsFailure_OnError()
    {
        // Arrange
        Mock<IApplicationRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.All<Product>())
            .Throws(new Exception("Db error"));

        _adminProductService = new AdminProductService(repositoryMock.Object, _userManagerMock.Object);

        // Act
        ServiceResult result = await _adminProductService.DeleteProductAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetProductTypeAttributesAsync_ReturnsAttributes_WhenProductTypeExists()
    {
        // Act
        ICollection<ProductTypeAttributesDto> productTypeAttributes = await _adminProductService
            .GetProductTypeAttributesAsync(_defaultProduct.ProductTypeId);

        // Assert
        Assert.That(productTypeAttributes.Count, Is.EqualTo(1));
        Assert.That(productTypeAttributes.First().Key, Is.EqualTo("Author"));
    }

    [Test]
    public async Task GetProductTypeAttributesAsync_ReturnsEmptyCollection_WhenProductTypeDoesNotExists()
    {
        // Act
        ICollection<ProductTypeAttributesDto> productTypeAttributes = await _adminProductService
            .GetProductTypeAttributesAsync(-1);

        // Assert
        Assert.That(productTypeAttributes.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetProductsListReadonlyAsync_ReturnsList()
    {
        // Act
        ICollection<ProductListViewModel> products = await _adminProductService
            .GetProductsListReadonlyAsync();

        // Assert
        Assert.That(products.Count, Is.EqualTo(1));
        Assert.That(products.First().Name, Is.EqualTo(_defaultProduct.Name));
    }

    [Test]
    public async Task GetProductsListReadonlyAsync_ReturnsEmptyCollection_WhenNoProductsAreFound()
    {
        // Arrange
        _dbContext.RemoveRange(_dbContext.ProductAttributes);
        _dbContext.RemoveRange(_dbContext.Products);
        await _dbContext.SaveChangesAsync();

        // Act
        ICollection<ProductListViewModel> products = await _adminProductService
            .GetProductsListReadonlyAsync();

        // Assert
        Assert.That(products.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ToggleProductPublicityAsync_ReturnsOk_WhenProductExists()
    {
        // Act
        ServiceResult result = await _adminProductService.ToggleProductPublicityAsync(_defaultProduct.Id);

        // Assert
        Product? updatedProduct = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == _defaultProduct.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(updatedProduct);
        Assert.IsFalse(updatedProduct!.IsPublic);
        Assert.IsTrue(updatedProduct.IsPublic == false);
        Assert.IsTrue(updatedProduct.IsDeleted == false);
    }

    [Test]
    public async Task ToggleProductPublicityAsync_Returns404_WhenProductIsNotFound()
    {
        // Act
        ServiceResult result = await _adminProductService.ToggleProductPublicityAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.Found);
    }

    [Test]
    public async Task ToggleProductPublicityAsync_ReturnsFailure_OnError()
    {
        // Arrange
        Mock<IApplicationRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.All<Product>())
            .Throws(new Exception("Db error"));

        _adminProductService = new AdminProductService(repositoryMock.Object, _userManagerMock.Object);

        // Act
        ServiceResult result = await _adminProductService.ToggleProductPublicityAsync(_defaultProduct.Id);

        // Assert
        Assert.IsFalse(result.Success);
    }

    private async Task<Product> SeedProductAsync()
    {
        ProductType productType = new()
        {
            Id = 1,
            Name = "Books",
            AttributeDefinitions = new List<ProductAttributeDefinition>
            {
                new ()
                {
                    Id = 1,
                    Key = "Author",
                    Label = "Author",
                    DataType = AttributeDataType.Text,
                    IsRequired = true,
                    HasPredefinedValue = false
                }
            }
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
            Weight = 0.5m,
            ThumbnailImage = new ProductImage
            {
                Id = Guid.NewGuid(),
                AltText = "Thumb",
                PublicId = "thumb-public-id",
                ImageUrl = "thumb.jpg"
            },
            Images = new List<ProductImage>
            {
                new ()
                {
                    Id = Guid.NewGuid(),
                    AltText = "image 1",
                    PublicId = "public-id",
                    ImageUrl = "image.jpg"
                }
            },
            Attributes = new List<ProductAttribute>
            {
                new ()
                {
                    Id = 1,
                    Key = "Author",
                    Value = "John Doe",
                    AttributeDefinition = productType.AttributeDefinitions.First(),
                }
            },
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}