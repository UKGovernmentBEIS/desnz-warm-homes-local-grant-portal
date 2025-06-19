using FluentAssertions;
using NUnit.Framework;
using Tests.Helpers;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

public class ConfirmCodesToDeliveryPartnerViewModelTests
{
    [TestCase(AuthorityType.LocalAuthority)]
    [TestCase(AuthorityType.Consortium)]
    public void ConfirmCodesToDeliveryPartnerViewModel_ShouldPassValidation(AuthorityType authorityType)
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel
        {
            User = new User(),
            Code = "1",
            AuthorityType = authorityType,
            IsConfirmed = true
        };

        // Act
        var validationResults = TestModelValidator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(0);
    }

    [Test]
    public void ConfirmCodesToDeliveryPartnerViewModel_IsConfirmedFalse_ShouldThrowError()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel
        {
            User = new User(),
            Code = "1",
            AuthorityType = AuthorityType.Consortium,
            IsConfirmed = false
        };

        // Act
        var validationResults = TestModelValidator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should()
            .Be("You must confirm the assignment to onboard.");
    }
}