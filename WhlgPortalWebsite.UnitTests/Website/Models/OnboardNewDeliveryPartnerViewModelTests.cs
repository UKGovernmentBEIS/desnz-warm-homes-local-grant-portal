using FluentAssertions;
using NUnit.Framework;
using WhlgPortalWebsite.Models;
using Tests.Helpers;

namespace Tests.Website.Models;

public class OnboardNewDeliveryPartnerViewModelTests
{
    [Test]
    public void OnboardNewDeliveryPartnerViewModel_EmailAddress_ShouldPassWhenValid()
    {
        // Arrange
        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "valid@example.com"
        };

        // Act
        var validationResults = ModelValidator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(0);
    }

    [Test]
    public void OnboardNewDeliveryPartnerViewModel_EmailAddress_ShouldFailWhenInvalid()
    {
        // Arrange
        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "invalid-email"
        };

        // Act
        var validationResults = ModelValidator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should()
            .Be("Enter an email address in the correct format, like name@example.com");
    }

    [Test]
    public void OnboardNewDeliveryPartnerViewModel_EmailAddress_ShouldFailWhenEmpty()
    {
        // Arrange
        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = ""
        };

        // Act
        var validationResults = ModelValidator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should()
            .Be("Email address is required.");
    }
}