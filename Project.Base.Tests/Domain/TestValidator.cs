using FluentValidation;
using Project.Base.Domain.Validators;

namespace Project.Base.Tests.Domain;

public class TestValidator : BaseAbstractValidator<TestEntity>
{
    public override void AssignCommonValidations()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }
}

public class TestValidatorMinimal : BaseAbstractValidator<TestEntity>
{
    public override void AssignCommonValidations()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}
