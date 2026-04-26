using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.Utils.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class ProductServiceTests
{
    private IProductService _productService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;
    private Mock<IFastCacheService> _cacheMock = null!;
    private Mock<IProductDataService> _productDataServiceMock = null!;
    private readonly Mock<IGlobalCacheKeysManagementService> _globalCacheKeysManagementServiceMock = new();
    private readonly Mock<ILogger<ProductService>> _loggerMock = new();

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

        _cacheMock = new Mock<IFastCacheService>();
        _productDataServiceMock = new Mock<IProductDataService>();

        _cacheMock
            .Setup(c => c.GetBatchAsync<ProductCardGeneralInfoDto>(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> keys) => keys.Select(_ => (ProductCardGeneralInfoDto?)null).ToList());

        _cacheMock
            .Setup(c => c.SetBatchAsync(It.IsAny<IDictionary<string, ProductCardGeneralInfoDto>>(), It.IsAny<TimeSpan>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);


        // GlobalCacheKeysManagementService default: return empty admin set
        _globalCacheKeysManagementServiceMock
            .Setup(g => g.GetAllAdminIdsAsync())
            .ReturnsAsync(Array.Empty<string>());

        _dbContext = new ApplicationDbContext(options);
        _productService = new ProductService(_cacheMock.Object, _productDataServiceMock.Object, _globalCacheKeysManagementServiceMock.Object,
            new ApplicationRepository(_dbContext), _userManagerMock.Object, _loggerMock.Object);

        // Arrange
        _defaultProduct = await SeedProductAsync();

        // Wire up ProductDataService mock to delegate to in-memory DB
        _productDataServiceMock
            .Setup(ds => ds.GetProductWithLikesForEditByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
                _dbContext.Products
                    .IgnoreQueryFilters()
                    .Include(p => p.Likes)
                    .FirstOrDefault(p => p.Id == id));
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
        ICollection<ProductCardDto> products = await _productService
            .GetAllProductCardsReadonlyAsync(DefaultUserId, 1, sort, null);

        // Assert
        Assert.That(products.Count == 1);
    }

    [Test]
    public async Task GetLikedProductsReadonlyAsync_ReturnsArray_WhenUserHasNone()
    {
        // Act
        ICollection<LikedProductDto> products = await _productService
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
        ICollection<LikedProductDto> products = await _productService
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
        // Arrange — ProductDataService returns the product; UserManager.FindByIdAsync returns null
        _userManagerMock
            .Setup(um => um.FindByIdAsync("invalid-user-id"))
            .ReturnsAsync((ApplicationUser?)null);

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
        ServiceResult<ProductDetailsDto> sr = await _productService
            .GetProductDetailsReadonlyAsync(new Guid(), DefaultUserId);

        // Assert
        Assert.IsFalse(sr.Success);
        Assert.IsFalse(sr.Found);
    }

    [Test]
    public async Task GetProductDetailsReadonlyAsync_ReturnsOk_WhenProductIsFound()
    {
        // Arrange
        _productDataServiceMock.Setup(ds => ds.GetProductDetailsGeneralInfoByIdAsync(_defaultProduct.Id, false))
            .ReturnsAsync(new ProductDetailsGeneralInfoDto
            {
                Id = _defaultProduct.Id,
                Name = _defaultProduct.Name,
                Description = _defaultProduct.Description,
                Price = _defaultProduct.Price,
                IsInStock = _defaultProduct.StockQuantity > 0,
                IsPublic = _defaultProduct.IsPublic,
                Quantity = _defaultProduct.StockQuantity,
                ProductTypeName = _defaultProduct.ProductType.Name,
                Images = _defaultProduct.Images
                    .Where(i => i.Id != _defaultProduct.Thumbnail.ImageId)
                    .OrderByDescending(i => i.Id == _defaultProduct.Thumbnail.ImageOriginalId)
                    .Select(i => new ProductDetailsImageDto
                    {
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText
                    })
                    .ToHashSet(),
                Attributes = _defaultProduct.Attributes
                    .Select(a => new ProductAttributeDetailsDto
                    {
                        Label = a.AttributeDefinition.Label,
                        Value = a.Value
                    })
                    .ToHashSet()
            });

        Guid commitId = Guid.NewGuid();

        _productDataServiceMock.Setup(ds => ds.GetProductDetailsCommentsInfoByIdAsync(_defaultProduct.Id, false, new HashSet<string>()))
            .ReturnsAsync(new ProductDetailsCommentsInfoDto
            {
                Comments = new List<ProductBaseCommentGeneralInfoDto>(),
                CommitId = commitId,
            });

        _productDataServiceMock.Setup(ds => ds.GetProductDetailsUserInfoByIdAsync(_defaultProduct.Id, false, DefaultUserId, commitId))
            .ReturnsAsync(new ProductDetailsUserInfoDto
            {
                IsLiked = false
            });

        // Act
        ServiceResult<ProductDetailsDto> sr = await _productService
            .GetProductDetailsReadonlyAsync(_defaultProduct.Id, DefaultUserId);

        // Assert
        Assert.IsTrue(sr.Success);
        Assert.IsTrue(sr.HasResult());
        Assert.That(sr.Result!.Id == _defaultProduct.Id);
        Assert.That(sr.Result.Images.Count == 1);
        Assert.That(sr.Result.Attributes.Count == 1);
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

        ProductService service = new ProductService(_cacheMock.Object, _productDataServiceMock.Object, _globalCacheKeysManagementServiceMock.Object,
            repoMock.Object, _userManagerMock.Object, _loggerMock.Object);

        // Act
        ServiceResult<ProductDetailsDto> result = await service
            .GetProductDetailsReadonlyAsync(Guid.NewGuid(), "user-id");

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetAllProductsCountAsync_ReturnsCorrectAnswer()
    {
        // Act
        int count = await _productService.GetAllProductsCountAsync(null);

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

        Guid originalImageId = Guid.NewGuid();

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Commenter",
            UserName = "commenter@test.com"
        };
        _dbContext.Users.Add(user);

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
            Currency = "EUR",
            Weight = 0.5m,
            Thumbnail = new ProductThumbnail
            {
                ImageOriginalId = originalImageId,
                Image = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    AltText = "thumbnail",
                    PublicId = "thumb-public-id",
                    ImageUrl = "thumb.jpg"
                }
            },
            Images = new List<ProductImage>
            {
                new ()
                {
                    Id = originalImageId,
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
                    Value = "John Doe",
                    AttributeDefinition = new ProductAttributeDefinition
                    {
                        Id = 1,
                        DataType = Core.Models.Enums.AttributeDataType.Text,
                        Label = "test",
                        Key = "test",
                        IsRequired = false,
                        ProductTypeId = 1
                    }
                }
            },
            Comments = new List<ProductComment>
            {
                new ()
                {
                    Id = Guid.NewGuid(),
                    Text = "Great product!",
                    Rating = 5,
                    Date = DateTime.UtcNow,
                    User = user
                }
            }
        };

        _dbContext.ProductTypes.Add(productType);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}
