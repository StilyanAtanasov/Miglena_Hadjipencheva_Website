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

    private Mock<UserManager<IdentityUser>> _userManagerMock = null!;

    private Product _defaultProduct = null!;
    private const string DefaultUserId = "test-user";

    [SetUp]
    public async Task Setup()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(new Mock<IUserStore<IdentityUser>>().Object);

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductTestDb")
            .Options;

        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
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

        IdentityUser user = new()
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
        IdentityUser user = new()
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
        IdentityUser user = new()
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

    private async Task<Product> SeedProductAsync()
    {
        var productType = new ProductType
        {
            Id = 1,
            Name = "Books"
        };

        var product = new Product
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
            }
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}