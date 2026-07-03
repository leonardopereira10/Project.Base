using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Base.Contracts.Models;
using Project.Base.Contracts.ServiceContracts;
using Project.Base.Enumerators;
using Project.Base.Tests.Domain;
using Project.Base.WebApi.Controllers;

namespace Project.Base.Tests.Controllers;

/// <summary>
/// Teste unitário para <see cref="AbstractController{TDto}"/>.
/// Usa mocks de <see cref="IBaseService{TDto}"/> para cobrir todos os branches.
/// </summary>
public class AbstractControllerTests
{
    private readonly Mock<IBaseService<TestDto>> _serviceMock = new();
    private readonly TestController _controller;

    public AbstractControllerTests()
    {
        _controller = new TestController(_serviceMock.Object);
    }

    #region FindById

    [Fact]
    public async Task FindById_WithSuccessId_ReturnsOkWithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new TestDto { Id = id, Name = "Test", Email = "test@test.com" };
        var output = new DtoOutput<TestDto> { ResultSet = new[] { dto }, TotalCount = 1 };

        _serviceMock
            .Setup(s => s.FindById(id))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.FindById(id);

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.True(actualOutput.Success);
        Assert.NotNull(actualOutput.ResultSet);
        Assert.Single(actualOutput.ResultSet);
    }

    [Fact]
    public async Task FindById_WithFailure_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[] { new ValidationFail { Message = "Not found", Property = "Id", IsImpeditive = false } }
        };

        _serviceMock
            .Setup(s => s.FindById(id))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.FindById(id);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    #endregion

    #region FindAll

    [Fact]
    public async Task FindAll_WithResults_ReturnsOk()
    {
        // Arrange
        var dtos = new[]
        {
            new TestDto { Id = Guid.NewGuid(), Name = "Test1", Email = "test1@test.com" },
            new TestDto { Id = Guid.NewGuid(), Name = "Test2", Email = "test2@test.com" }
        };
        var output = new DtoOutput<TestDto> { ResultSet = dtos, TotalCount = 2 };

        _serviceMock
            .Setup(s => s.FindAll())
            .ReturnsAsync(output);

        // Act
        var result = await _controller.FindAll();

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.Equal(2, actualOutput.TotalCount);
    }

    [Fact]
    public async Task FindAll_WithNull_ReturnsNoContent()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.FindAll())
            .ReturnsAsync((DtoOutput<TestDto>)null!);

        // Act
        var result = await _controller.FindAll();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    #endregion

    #region Find

    [Fact]
    public async Task Find_WithResults_ReturnsOk()
    {
        // Arrange
        var dtos = new[] { new TestDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" } };
        var output = new DtoOutput<TestDto> { ResultSet = dtos, TotalCount = 1, Page = 0, PageSize = 10 };

        _serviceMock
            .Setup(s => s.Find(1, 10, EnumOrder.ASCENDING, null, "search"))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Find(1, 10, EnumOrder.ASCENDING, null, "search");

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.True(actualOutput.Success);
    }

    [Fact]
    public async Task Find_WithNullResult_ReturnsNoContent()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.Find(1, 10, EnumOrder.ASCENDING, null, null))
            .ReturnsAsync((DtoOutput<TestDto>)null!);

        // Act
        var result = await _controller.Find(1, 10, EnumOrder.ASCENDING, null, null);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Find_WithZeroTotalCount_ReturnsNoContent()
    {
        // Arrange
        var output = new DtoOutput<TestDto> { ResultSet = Array.Empty<TestDto>(), TotalCount = 0 };

        _serviceMock
            .Setup(s => s.Find(1, 10, EnumOrder.ASCENDING, null, ""))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Find(1, 10, EnumOrder.ASCENDING, null, "");

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    #endregion

    #region Insert

    [Fact]
    public async Task Insert_WithSuccessAndResult_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new TestDto { Name = "New", Email = "new@test.com" };
        var createdDto = new TestDto { Id = Guid.NewGuid(), Name = "New", Email = "new@test.com" };
        var output = new DtoOutput<TestDto>
        {
            Success = true,
            ResultSet = new[] { createdDto },
            TotalCount = 1
        };

        _serviceMock
            .Setup(s => s.Insert(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Insert(dto);

        // Assert
        var actionResult = Assert.IsAssignableFrom<CreatedAtActionResult>(result.Result);
        Assert.Equal("FindById", actionResult.ActionName);
        Assert.NotNull(actionResult.Value);
    }

    [Fact]
    public async Task Insert_WithNullDto_ReturnsBadRequest()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.Insert(It.IsAny<TestDto>()))
            .ReturnsAsync((DtoOutput<TestDto>)null!);

        // Act
        var result = await _controller.Insert(new TestDto());

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Insert_WithSuccessFalseAndNoValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var dto = new TestDto { Name = "New", Email = "new@test.com" };
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = Array.Empty<ValidationFail>()
        };

        _serviceMock
            .Setup(s => s.Insert(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Insert(dto);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Insert_WithValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var dto = new TestDto { Name = "", Email = "invalid" };
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Name is required", Property = "Name", IsImpeditive = true }
            }
        };

        _serviceMock
            .Setup(s => s.Insert(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Insert(dto);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_WithSuccess_ReturnsOk()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Updated", Email = "updated@test.com" };
        var output = new DtoOutput<TestDto>
        {
            Success = true,
            ResultSet = new[] { dto },
            TotalCount = 1
        };

        _serviceMock
            .Setup(s => s.Update(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Update(dto);

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.True(actualOutput.Success);
    }

    [Fact]
    public async Task Update_WithValidationFailAndImpeditive_ReturnsBadRequest()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "", Email = "invalid" };
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Name is required", Property = "Name", IsImpeditive = true }
            }
        };

        _serviceMock
            .Setup(s => s.Update(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Update(dto);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_WithValidationFailNoImpeditive_ReturnsOk()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Warning message", Property = "Email", IsImpeditive = false }
            }
        };

        _serviceMock
            .Setup(s => s.Update(dto))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Update(dto);

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.False(actualOutput.Success);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_WithSuccess_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var output = new DtoOutput<TestDto>
        {
            Success = true,
            TotalCount = 0
        };

        _serviceMock
            .Setup(s => s.Delete(id))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result);
        var actualOutput = Assert.IsType<DtoOutput<TestDto>>(actionResult.Value);
        Assert.True(actualOutput.Success);
    }

    [Fact]
    public async Task Delete_WithException_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.Delete(id))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var actionResult = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        Assert.Equal(id, actionResult.Value);
    }

    [Fact]
    public async Task Delete_WithFailure_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var output = new DtoOutput<TestDto>
        {
            Success = false
        };

        _serviceMock
            .Setup(s => s.Delete(id))
            .ReturnsAsync(output);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(result);
    }

    #endregion

    #region ThrowIfFailed

    [Fact]
    public async Task ThrowIfFailed_WithImpeditiveValidationFails_ThrowsValidationException()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Error 1", Property = "Prop1", IsImpeditive = true },
                new ValidationFail { Message = "Error 2", Property = "Prop2", IsImpeditive = true }
            }
        };
        var task = Task.FromResult(output);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => TestController.ThrowIfFailedStatic(task, "test"));
        Assert.Contains("Error 1", exception.Message);
        Assert.Contains("Error 2", exception.Message);
    }

    [Fact]
    public async Task ThrowIfFailed_WithNonImpeditiveValidationFails_DoesNotThrow()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Warning", Property = "Prop", IsImpeditive = false }
            }
        };
        var task = Task.FromResult(output);

        // Act & Assert - não deve lançar exceção
        var exception = await Record.ExceptionAsync(() => TestController.ThrowIfFailedStatic(task, "test"));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ThrowIfFailed_WithSuccess_DoesNotThrow()
    {
        // Arrange
        var output = new DtoOutput<TestDto> { Success = true };
        var task = Task.FromResult(output);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => TestController.ThrowIfFailedStatic(task, "test"));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ThrowIfFailed_WithNullValidationFails_DoesNotThrow()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = null
        };
        var task = Task.FromResult(output);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => TestController.ThrowIfFailedStatic(task, "test"));
        Assert.Null(exception);
    }

    #endregion

    #region CheckResult

    [Fact]
    public void CheckResult_WithImpeditiveValidationFails_ThrowsValidationException()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Impeditive error", Property = "Prop", IsImpeditive = true }
            }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => TestController.CheckResultStatic(output));
        Assert.Contains("Impeditive error", exception.Message);
    }

    [Fact]
    public void CheckResult_WithNoImpeditiveAndNoMessages_ThrowsGenericException()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = new[]
            {
                new ValidationFail { Message = "Non-impeditive", Property = "Prop", IsImpeditive = false }
            }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => TestController.CheckResultStatic(output));
        Assert.Equal("Failed operation..", exception.Message);
    }

    [Fact]
    public void CheckResult_WithNullValidationFails_ThrowsGenericException()
    {
        // Arrange
        var output = new DtoOutput<TestDto>
        {
            Success = false,
            ValidationFails = null
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => TestController.CheckResultStatic(output));
        Assert.Equal("Failed operation..", exception.Message);
    }

    [Fact]
    public void CheckResult_WithSuccess_ReturnsFalse()
    {
        // Arrange
        var output = new DtoOutput<TestDto> { Success = true };

        // Act
        var result = TestController.CheckResultStatic(output);

        // Assert
        Assert.False(result);
    }

    #endregion
}

/// <summary>
/// Controller concreto para testes (AbstractController é abstract).
/// Expõe métodos protected static para testes diretos.
/// </summary>
public class TestController : AbstractController<TestDto>
{
    public TestController(IBaseService<TestDto> service) : base(service)
    {
    }

    // Expõe métodos protegidos para testes
    public new Task<ActionResult<DtoOutput<TestDto>>> FindById(Guid id) => base.FindById(id);
    public new Task<ActionResult<DtoOutput<TestDto>>> FindAll() => base.FindAll();
    public new Task<ActionResult<DtoOutput<TestDto>>> Find(int page, int limit, EnumOrder order, string? searchTarget, string? searchTerm)
        => base.Find(page, limit, order, searchTarget, searchTerm);
    public new Task<ActionResult<DtoOutput<TestDto>>> Insert(TestDto newObj) => base.Insert(newObj);
    public new Task<ActionResult<DtoOutput<TestDto>>> Update(TestDto newObj) => base.Update(newObj);
    public new Task<ActionResult> Delete(Guid id) => base.Delete(id);

    // Expõe métodos protected static para testes diretos
    public static Task ThrowIfFailedStatic(Task<DtoOutput<TestDto>> task, string operation)
        => ThrowIfFailed(task, operation);

    public static bool CheckResultStatic(DtoOutput<TestDto> result)
        => CheckResult(result);
}
