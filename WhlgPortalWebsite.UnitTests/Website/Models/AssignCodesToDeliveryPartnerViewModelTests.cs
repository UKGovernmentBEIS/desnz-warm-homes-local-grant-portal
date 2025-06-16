using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tests.Helpers;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

public class AssignCodesToDeliveryPartnerViewModelTests
{
    private ModelValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new ModelValidator();
    }

    [Test]
    public void AssignCodesToDeliveryPartnerViewModel_ShouldPassValidation()
    {
        // Arrange
        var viewModel = new AssignCodesToDeliveryPartnerViewModel
        {
            User = new User(),
            SearchTerm = null,
            LocalAuthorities = new List<LocalAuthority>(),
            Consortia = new List<Consortium>()
        };

        // Act
        var validationResults = validator.ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(0);
    }
}