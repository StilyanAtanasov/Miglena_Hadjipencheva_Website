using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class CartServiceTests
{
    private ICartService _cartService = null!;
    private ApplicationDbContext _dbContext = null!;

    private Cart _cart = null!;
    private CartItem _item = null!;

    private const string DefaultUserId = "test-user";

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("CartTestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _cartService = new CartService(new ApplicationRepository(_dbContext));

        // Arrange
        (_cart, _item) = await SeedCartAsync(DefaultUserId);
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
    public async Task AddItemToCartAsync_Returns400_WhenProductNotFound()
    {
        // Act
        ServiceResult result = await _cartService.AddItemToCartAsync(DefaultUserId, Guid.NewGuid(), 1);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("product"), Is.True);
    }

    [Test]
    public async Task AddItemToCartAsync_Returns400_WhenInsufficientStockQuantity()
    {
        // Act
        ServiceResult result = await _cartService.AddItemToCartAsync(DefaultUserId, _item.ProductId, 100);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("quantity"), Is.True);
    }

    [Test]
    public async Task AddItemToCartAsync_AddsItem_OnNonExistingCart()
    {
        // Arrange
        string newUserId = "test-user2";

        // Act
        ServiceResult result = await _cartService.AddItemToCartAsync(newUserId, _item.ProductId, 1);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(_dbContext.Carts.Count(), Is.EqualTo(2));
        Assert.That(_dbContext.Carts.Any(c => c.UserId == newUserId));
        Assert.That(_dbContext.Carts.First(c => c.UserId == newUserId).CartItems.Count == 1);
    }

    [Test]
    public async Task AddItemToCartAsync_UpdatesItemQuantity_WhenAlreadyAdded()
    {
        // Act
        ServiceResult result = await _cartService.AddItemToCartAsync(DefaultUserId, _item.ProductId, 1);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(_dbContext.Carts.Count(), Is.EqualTo(1));
        Assert.That(_dbContext.Carts.First(c => c.UserId == DefaultUserId).CartItems.Count == 1);
        Assert.That(_dbContext.Carts.First(c => c.UserId == DefaultUserId).CartItems.First().Quantity == 2);
    }

    [Test]
    public async Task AddItemToCartAsync_AddsItem_OnExistingCart()
    {
        // Arrange
        Product newProduct = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Product 2",
            Description = "Test Description",
            StockQuantity = 10,
            IsDeleted = false,
            IsPublic = true,
            ProductTypeId = 1,
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
        };

        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync();

        // Act
        ServiceResult result = await _cartService.AddItemToCartAsync(DefaultUserId, newProduct.Id, 1);

        Cart cart = await _dbContext.Carts
            .Include(c => c.CartItems)
            .FirstAsync(c => c.UserId == DefaultUserId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(_dbContext.Carts.Count(), Is.EqualTo(1));
        Assert.That(cart.CartItems.Count == 2);
        Assert.That(cart.CartItems.Any(ci => ci.ProductId == newProduct.Id));
    }

    [Test]
    public async Task GetCartReadonlyAsync_ReturnsNull_WhenCartNotFound()
    {
        CartViewModel result = await _cartService.GetCartReadonlyAsync("test-user-id");

        Assert.IsEmpty(result.Items);
    }

    [Test]
    public async Task GetCartReadonlyAsync_ReturnsViewModel_WhenCartExists()
    {
        // Act
        CartViewModel result = await _cartService.GetCartReadonlyAsync(DefaultUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.Items.First().Name, Is.EqualTo("Test Product"));
    }

    [Test]
    public async Task RemoveFromCartAsync_RemovesItem_AndDeletesCart_WhenLastItem()
    {
        // Act
        ServiceResult result = await _cartService.RemoveFromCartAsync(_cart.UserId, _item.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(await _dbContext.CartItems.FindAsync(_item.Id), Is.Null);
        Assert.That(await _dbContext.Carts.FindAsync(_cart.Id), Is.Null);
    }

    [Test]
    public async Task RemoveFromCartAsync_ReturnsBadRequest_WhenCartNotFound()
    {
        // Act
        ServiceResult result = await _cartService.RemoveFromCartAsync("unknown-user", Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("cart"), Is.True);
    }

    [Test]
    public async Task RemoveFromCartAsync_ReturnsBadRequest_WhenItemNotFound()
    {
        // Act
        ServiceResult result = await _cartService.RemoveFromCartAsync(_cart.UserId, Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("product"), Is.True);
    }

    [Test]
    public async Task UpdateItemQuantityAsync_UpdatesQuantity_AndReturnsCorrectTotals()
    {
        // Act
        ServiceResult<UpdatedItemQuantityViewModel> result = await _cartService
            .UpdateItemQuantityAsync(_cart.UserId, _item.Id, 5);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.That(result.Result!.LineTotal, Is.EqualTo(100));
        Assert.That(result.Result.Total, Is.EqualTo(100));
        Assert.That(_item.Quantity, Is.EqualTo(5));
    }

    [Test]
    public async Task UpdateItemQuantityAsync_ReturnsBadRequest_WhenCartNotFound()
    {
        // Act
        ServiceResult<UpdatedItemQuantityViewModel> result = await _cartService
            .UpdateItemQuantityAsync("unknown-user", Guid.NewGuid(), 2);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("cart"), Is.True);
    }

    [Test]
    public async Task UpdateItemQuantityAsync_ReturnsBadRequest_WhenItemNotFound()
    {
        // Act
        ServiceResult<UpdatedItemQuantityViewModel> result = await _cartService
            .UpdateItemQuantityAsync(_cart.UserId, Guid.NewGuid(), 2);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Errors.ContainsKey("product"), Is.True);
    }

    private async Task<(Cart cart, CartItem item)> SeedCartAsync(string userId, int quantity = 1)
    {
        Guid originalImageId = Guid.NewGuid();

        Product product = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            StockQuantity = 10,
            IsDeleted = false,
            IsPublic = true,
            ProductType = new ProductType { Id = 1, Name = "Books" },
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
        };

        CartItem cartItem = new()
        {
            Id = Guid.NewGuid(),
            Price = 20,
            Quantity = quantity,
            Product = product
        };

        Cart cart = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CartItems = new List<CartItem> { cartItem }
        };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        return (cart, cartItem);
    }
}