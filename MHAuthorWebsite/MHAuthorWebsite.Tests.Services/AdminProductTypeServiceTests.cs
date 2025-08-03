using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.ProductType;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MHAuthorWebsite.Tests.Services;

[TestFixture]
public class AdminProductTypeServiceTests
{
    private IAdminProductTypeService _adminProductTypeService = null!;
    private ApplicationDbContext _dbContext = null!;

    private ProductType _defaultProductType = null!;

    [SetUp]
    public async Task Setup()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductTypeTestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _adminProductTypeService = new AdminProductTypeService(new ApplicationRepository(_dbContext));

        // Arrange
        _defaultProductType = await SeedProductTypeAsync();
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
    public async Task GetAllReadonlyAsync_ReturnsAllProductTypes()
    {
        // Act
        ICollection<ProductTypeDto> productTypes = await _adminProductTypeService.GetAllReadonlyAsync();

        // Assert
        Assert.That(productTypes, Is.Not.Null);
        Assert.That(productTypes.Count, Is.EqualTo(1));
        Assert.That(productTypes.First().Id, Is.EqualTo(_defaultProductType.Id));
        Assert.That(productTypes.First().Name, Is.EqualTo(_defaultProductType.Name));
    }

    [Test]
    public async Task GetAllReadonlyAsync_ReturnsEmptyCollection_WhenNoProductTypesExist()
    {
        // Arrange
        _dbContext.ProductTypes.RemoveRange(_dbContext.ProductTypes);
        await _dbContext.SaveChangesAsync();

        // Act
        ICollection<ProductTypeDto> productTypes = await _adminProductTypeService.GetAllReadonlyAsync();

        // Assert
        Assert.That(productTypes, Is.Not.Null);
        Assert.That(productTypes.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task AddProductTypeAsync_ReturnsOk_WhenThereAreAdditionalProperties()
    {
        // Arrange
        AddProductTypeForm newProductType = new()
        {
            Name = "New Product Type",
            HasAdditionalProperties = true,
            Attributes = new List<AttributeDefinitionForm>
            {
                new ()
                {
                    Key = "New Key",
                    Label = "New Label",
                    DataType = 1,
                    IsRequired = true,
                    HasPredefinedValue = false
                }
            }
        };

        // Act
        ServiceResult result = await _adminProductTypeService.AddProductTypeAsync(newProductType);

        // Assert
        ProductType? addedProductType = await _dbContext.ProductTypes
            .Include(pt => pt.AttributeDefinitions)
            .FirstOrDefaultAsync(pt => pt.Name == newProductType.Name);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(addedProductType);
        Assert.That(_dbContext.ProductTypes.Count(), Is.EqualTo(2));
        Assert.That(addedProductType!.AttributeDefinitions.Count, Is.EqualTo(1));
        Assert.That(addedProductType!.AttributeDefinitions.ElementAt(0).Key, Is.EqualTo(newProductType.Attributes.First().Key));
    }

    [Test]
    public async Task AddProductTypeAsync_ReturnsOk_WhenThereAreNoneAdditionalProperties()
    {
        // Arrange
        AddProductTypeForm newProductType = new()
        {
            Name = "New Product Type",
            HasAdditionalProperties = false,
            Attributes = new List<AttributeDefinitionForm>()
        };

        // Act
        ServiceResult result = await _adminProductTypeService.AddProductTypeAsync(newProductType);

        // Assert
        ProductType? addedProductType = await _dbContext.ProductTypes
            .Include(pt => pt.AttributeDefinitions)
            .FirstOrDefaultAsync(pt => pt.Name == newProductType.Name);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(addedProductType);
        Assert.That(_dbContext.ProductTypes.Count(), Is.EqualTo(2));
        Assert.That(addedProductType!.AttributeDefinitions.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task AddProductTypeAsync_ReturnsFailure_OnError()
    {
        // Arrange
        AddProductTypeForm newProductType = new()
        {
            Name = "New Product Type",
            HasAdditionalProperties = false,
            Attributes = new List<AttributeDefinitionForm>()
        };

        // Simulate an error by throwing an exception in the repository
        Mock<IApplicationRepository> repoMock = new();
        AdminProductTypeService mockedRepoService = new(repoMock.Object);

        repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductType>()))
            .Throws(new Exception("Simulated failure"));

        // Act
        ServiceResult result = await mockedRepoService.AddProductTypeAsync(newProductType);

        // Assert
        ProductType? addedProductType = await _dbContext.ProductTypes
            .Include(pt => pt.AttributeDefinitions)
            .FirstOrDefaultAsync(pt => pt.Name == newProductType.Name);

        Assert.IsFalse(result.Success);
        Assert.IsNull(addedProductType);
        Assert.That(_dbContext.ProductTypes.Count(), Is.EqualTo(1));
    }

    private async Task<ProductType> SeedProductTypeAsync()
    {
        ProductType productType = new()
        {
            Id = 1,
            Name = "Test Product Type",
            AttributeDefinitions = new HashSet<ProductAttributeDefinition>()
            {
                new ()
                {
                    Id = 1,
                    DataType = AttributeDataType.Text,
                    Key = "TestKey",
                    Label = "Test Label",
                    HasPredefinedValue = false,
                    IsRequired = true,
                }
            }
        };

        await _dbContext.ProductTypes.AddAsync(productType);
        await _dbContext.SaveChangesAsync();

        return productType;
    }
}