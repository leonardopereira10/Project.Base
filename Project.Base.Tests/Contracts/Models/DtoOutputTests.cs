using FluentAssertions;
using Project.Base.Contracts.Models;

namespace Project.Base.Tests.Contracts.Models;

public class DtoOutputTests
{
    private class TestDto : DtoBase
    {
        public required string Name { get; set; }
    }

    [Fact]
    public void Ctor_Should_SetSuccessToTrue()
    {
        var output = new DtoOutput<TestDto>();
        output.Success.Should().BeTrue();
    }

    [Fact]
    public void PageCount_WhenResultSetIsNull_ShouldReturnZero()
    {
        var output = new DtoOutput<TestDto>();
        output.ResultSet = null;
        output.PageCount.Should().Be(0);
    }

    [Fact]
    public void PageCount_WhenResultSetHasItems_ShouldReturnCount()
    {
        var output = new DtoOutput<TestDto>();
        output.ResultSet = new List<TestDto>
        {
            new TestDto { Id = Guid.NewGuid(), Name = "A" },
            new TestDto { Id = Guid.NewGuid(), Name = "B" },
            new TestDto { Id = Guid.NewGuid(), Name = "C" }
        };
        output.PageCount.Should().Be(3);
    }

    [Fact]
    public void PageCount_WhenResultSetIsEmpty_ShouldReturnZero()
    {
        var output = new DtoOutput<TestDto>();
        output.ResultSet = new List<TestDto>();
        output.PageCount.Should().Be(0);
    }

    [Fact]
    public void DefaultValues_Should_AllBeCorrect()
    {
        var output = new DtoOutput<TestDto>();
        output.Page.Should().Be(0);
        output.PageSize.Should().Be(0);
        output.TotalCount.Should().Be(0);
    }

    [Fact]
    public void ValidationFails_Should_BeSettable()
    {
        var fail = new ValidationFail
        {
            Message = "Error",
            Property = "Name",
            IsImpeditive = true
        };

        var output = new DtoOutput<TestDto>();
        output.ValidationFails = new[] { fail };
        output.ValidationFails.Should().ContainSingle();
        output.ValidationFails.First().Message.Should().Be("Error");
    }

    [Fact]
    public void Success_Should_BeSettable()
    {
        var output = new DtoOutput<TestDto>();
        output.Success = false;
        output.Success.Should().BeFalse();
    }
}
