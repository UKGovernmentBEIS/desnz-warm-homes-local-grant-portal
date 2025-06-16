using FluentAssertions;
using NUnit.Framework;
using Tests.Helpers;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

public class ConfirmCodesToDeliveryPartnerViewModelTests
{
    private ModelValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new ModelValidator();
    }

    [Test]
    public void ConfirmCodesToDeliveryPartnerViewModel_LocalAuthority_ShouldPassValidation()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel()
        {
            User = new User(),
            Code = "1",
            AuthorityType = AuthorityType.LocalAuthority,
            IsConfirmed = true
        };

        // Act
        var validationResults = validator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(0);
    }

    [Test]
    public void ConfirmCodesToDeliveryPartnerViewModel_Consortium_ShouldPassValidation()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel()
        {
            User = new User(),
            Code = "1",
            AuthorityType = AuthorityType.Consortium,
            IsConfirmed = true
        };

        // Act
        var validationResults = validator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(0);
    }

    [Test]
    public void ConfirmCodesToDeliveryPartnerViewModel_IsConfirmedFalse_ShouldThrowError()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel()
        {
            User = new User(),
            Code = "1",
            AuthorityType = AuthorityType.Consortium,
            IsConfirmed = false
        };

        // Act
        var validationResults = validator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should()
            .Be("You must confirm the assignment to onboard.");
    }
}