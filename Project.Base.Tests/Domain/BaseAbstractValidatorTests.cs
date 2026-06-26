using FluentAssertions;
using FluentValidation.Results;
using Project.Base.Contracts.Models;

namespace Project.Base.Tests.Domain;

public class BaseAbstractValidatorTests
{
    private readonly TestValidator _validator;

    public BaseAbstractValidatorTests()
    {
        _validator = new TestValidator();
    }

    #region Insert Validations

    [Fact]
    public void AssignInsertValidations_ValidEntity_ShouldPass()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Valid", Email = "valid@test.com" };

        // Act
        _validator.AssignInsertValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignInsertValidations_EmptyName_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "", Email = "valid@test.com" };

        // Act
        _validator.AssignInsertValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public void AssignInsertValidations_InvalidEmail_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Valid", Email = "not-an-email" };

        // Act
        _validator.AssignInsertValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void AssignInsertValidations_NameTooLong_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 101),
            Email = "valid@test.com"
        };

        // Act
        _validator.AssignInsertValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void AssignInsertValidations_NullName_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = null!, Email = "valid@test.com" };

        // Act
        _validator.AssignInsertValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    #endregion

    #region Update Validations

    [Fact]
    public void AssignUpdateValidations_MissingId_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.Empty, Name = "Valid", Email = "valid@test.com" };

        // Act
        _validator.AssignUpdateValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void AssignUpdateValidations_ValidEntity_ShouldPass()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Updated",
            Email = "updated@test.com"
        };

        // Act
        _validator.AssignUpdateValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignUpdateValidations_EmptyEmail_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Valid",
            Email = ""
        };

        // Act
        _validator.AssignUpdateValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    #endregion

    #region Delete Validations

    [Fact]
    public void AssignDeleteValidations_MissingId_ShouldFail()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.Empty };

        // Act
        _validator.AssignDeleteValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void AssignDeleteValidations_ValidId_ShouldPass()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid() };

        // Act
        _validator.AssignDeleteValidations();
        var result = _validator.Validate(entity);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignDeleteValidations_DoesNotCheckName_ShouldPass()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "", Email = "" };

        // Act
        _validator.AssignDeleteValidations();
        var result = _validator.Validate(entity);

        // Assert — delete only checks ID, not name/email
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region GetValidationOutput

    [Fact]
    public void GetValidationOutput_ValidResult_ShouldReturnEmptyList()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Valid", Email = "valid@test.com" };
        _validator.AssignInsertValidations();
        var validationResult = _validator.Validate(entity);

        // Act
        var output = _validator.GetValidationOutput(validationResult);
        var outputList = output.ToList();

        // Assert
        outputList.Should().BeEmpty();
    }

    [Fact]
    public void GetValidationOutput_InvalidResult_ShouldReturnFails()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.Empty, Name = "", Email = "invalid" };
        _validator.AssignInsertValidations();
        var validationResult = _validator.Validate(entity);

        // Act
        var output = _validator.GetValidationOutput(validationResult);
        var outputList = output.ToList();

        // Assert
        outputList.Should().NotBeEmpty();
        outputList.Should().Contain(f => f.IsImpeditive == true);
        outputList.Should().Contain(f => f.Property == "Name");
        outputList.Should().Contain(f => f.Property == "Email");
    }

    [Fact]
    public void GetValidationOutput_ErrorSeverity_ShouldBeImpeditive()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.Empty, Name = "", Email = "" };
        _validator.AssignInsertValidations();
        var validationResult = _validator.Validate(entity);

        // Act
        var output = _validator.GetValidationOutput(validationResult);
        var outputList = output.ToList();

        // Assert
        outputList.All(f => f.IsImpeditive == true).Should().BeTrue();
    }

    [Fact]
    public void GetValidationOutput_ShouldPreserveErrorMessage()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.Empty, Name = "", Email = "" };
        _validator.AssignInsertValidations();
        var validationResult = _validator.Validate(entity);

        // Act
        var output = _validator.GetValidationOutput(validationResult);
        var outputList = output.ToList();

        // Assert
        var nameFail = outputList.First(f => f.Property == "Name");
        nameFail.Message.Should().Be("Name is required");
    }

    #endregion

    #region Virtual method override

    [Fact]
    public void AssignInsertValidations_CanBeOverridden_ShouldUseCustomValidation()
    {
        // Arrange
        var minimalValidator = new TestValidatorMinimal();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Valid", Email = "not-an-email" };

        // Act
        minimalValidator.AssignInsertValidations();
        var result = minimalValidator.Validate(entity);

        // Assert — email validation not defined in TestValidatorMinimal
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignUpdateValidations_MinimalValidator_OnlyChecksNameAndId()
    {
        // Arrange
        var minimalValidator = new TestValidatorMinimal();
        var entity = new TestEntity { Id = Guid.Empty, Name = "", Email = "not-an-email" };

        // Act
        minimalValidator.AssignUpdateValidations();
        var result = minimalValidator.Validate(entity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().NotContain(e => e.PropertyName == "Email");
    }

    #endregion
}
