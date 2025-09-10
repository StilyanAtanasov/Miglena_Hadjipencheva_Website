using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.Utils;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class ProductServiceTests
{
    private IProductService _productService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private Product _defaultProduct = null!;
    private const string DefaultUserId = "test-user";

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductTestDb")
            .Options;

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        _dbContext = new ApplicationDbContext(options);
        _productService = new ProductService(new ApplicationRepository(_dbContext), _userManagerMock.Object);

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
    public async Task GetAllProductCardsReadonlyAsync_ReturnsArray_WhenUserIdIsKnown()
    {
        // Arrange 
        (bool descending, Expression<Func<Product, object>>? expression) sort = SortValueMapper.SortMap["recommended"];

        // Act
        ICollection<ProductCardViewModel> products = await _productService
            .GetAllProductCardsReadonlyAsync(DefaultUserId, 1, sort);

        // Assert
        Assert.That(products.Count == 1);
    }

    [Test]
    public async Task GetLikedProductsReadonlyAsync_ReturnsArray_WhenUserHasNone()
    {
        // Act
        ICollection<LikedProductViewModel> products = await _productService
            .GetLikedProductsReadonlyAsync(DefaultUserId);

        // Assert
        Assert.That(products.Count == 0);
    }

    [Test]
    public async Task GetLikedProductsReadonlyAsync_ReturnsArray_WhenUserHasAny()
    {
        // Arrange
        Product p = await _dbContext.Products.FirstAsync();

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test-user2"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        p.Likes.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        ICollection<LikedProductViewModel> products = await _productService
            .GetLikedProductsReadonlyAsync(user.Id);

        // Assert
        Assert.That(products.Count == 1);
    }


    [Test]
    public async Task ToggleLikeProduct_ReturnsOk_WhenLiking()
    {
        // Arrange
        ApplicationUser user = new()
        {
            Id = DefaultUserId,
            UserName = "testuser@example.com"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(um => um.FindByIdAsync(DefaultUserId))
            .ReturnsAsync(user);

        // Act
        ServiceResult sr = await _productService
            .ToggleLikeProduct(DefaultUserId, _defaultProduct.Id);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.That(_dbContext.Products.First().Likes.Count == 1);
    }

    [Test]
    public async Task ToggleLikeProduct_ReturnsOk_WhenDisliking()
    {
        // Arrange
        ApplicationUser user = new()
        {
            Id = DefaultUserId,
            UserName = "testuser@example.com"
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(um => um.FindByIdAsync(DefaultUserId))
            .ReturnsAsync(user);

        // Act
        ServiceResult sr = await _productService
            .ToggleLikeProduct(DefaultUserId, _defaultProduct.Id);

        ServiceResult sr2 = await _productService
            .ToggleLikeProduct(DefaultUserId, _defaultProduct.Id);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.IsTrue(sr2.Success);
        Assert.That(_dbContext.Products.First().Likes.Count == 0);
    }

    [Test]
    public async Task ToggleLikeProduct_Returns404_WhenInvalidProduct()
    {
        // Act
        ServiceResult sr = await _productService
            .ToggleLikeProduct(DefaultUserId, Guid.NewGuid());

        // Assert
        Assert.IsFalse(sr.Success);
        Assert.IsFalse(sr.Found);
        Assert.That(_dbContext.Products.First().Likes.Count == 0);
    }

    [Test]
    public async Task ToggleLikeProduct_Returns403_WhenUserNotFound()
    {
        // Act
        ServiceResult sr = await _productService
            .ToggleLikeProduct("invalid-user-id", _defaultProduct.Id);

        // Assert
        Assert.IsFalse(sr.Success);
        Assert.IsFalse(sr.HasPermission);
        Assert.That(_dbContext.Products.First().Likes.Count == 0);
    }

    [Test]
    public async Task GetProductDetailsReadonlyAsync_Returns404_WhenProductNotFound()
    {
        // Act
        ServiceResult<ProductDetailsViewModel> sr = await _productService
            .GetProductDetailsReadonlyAsync(new Guid(), DefaultUserId);

        // Assert
        Assert.IsFalse(sr.Success);
        Assert.IsFalse(sr.Found);
    }

    [Test]
    public async Task GetProductDetailsReadonlyAsync_ReturnsOk_WhenProductIsFound()
    {
        // Act
        ServiceResult<ProductDetailsViewModel> sr = await _productService
            .GetProductDetailsReadonlyAsync(_defaultProduct.Id, DefaultUserId);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.IsTrue(sr.HasResult());
        Assert.That(sr.Result!.Id == _defaultProduct.Id);
        Assert.That(sr.Result.Images.Count == 1);
        Assert.That(sr.Result.Attributes.Count == 1);
    }

    [Test]
    public async Task GetProductDetailsReadonlyAsync_ReturnsFailure_OnError()
    {
        // Arrange
        Mock<IApplicationRepository> repoMock = new();

        repoMock
            .Setup(r => r.AllReadonly<Product>())
            .Throws(new Exception("Simulated failure"));

        ProductService service = new ProductService(repoMock.Object, _userManagerMock.Object);

        // Act
        ServiceResult<ProductDetailsViewModel> result = await service
            .GetProductDetailsReadonlyAsync(Guid.NewGuid(), "user-id");

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetAllProductsCountAsync_ReturnsCorrectAnswer()
    {
        // Act
        int count = await _productService.GetAllProductsCountAsync();

        // Assert
        Assert.That(count == _dbContext.Products.Count());
        Assert.That(count == 1);
    }

    private async Task<Product> SeedProductAsync()
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
            Images = new List<Image>
            {
                new ()
                {
                    Id = Guid.NewGuid(),
                    IsThumbnail = true,
                    ThumbnailUrl = "thumb.jpg",
                    AltText = "Thumb",
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
                    Value = "John Doe"
                }
            },
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}