using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Controllers;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Controllers;

[TestFixture]
public class ServiceManagerControllerTests
{
    [Test]
    public async Task OnboardDeliveryPartnerAsync_ShouldRedirectWhenEmailIsValid()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService
            .Setup(x => x.IsEmailAddressInUseAsync("existing@email.com"))
            .ReturnsAsync(false);

        var controller = new ServiceManagerController(mockUserService.Object);
        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "new@email.com"
        };

        // Act
        var result = await controller.OnboardDeliveryPartnerAsync(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Home");

        mockUserService.Verify(x => x.CreateDeliveryPartnerAsync("new@email.com"), Times.Once);
    }

    [Test]
    public async Task OnboardDeliveryPartnerAsync_ShouldReturnViewWithModelErrorWhenEmailAlreadyExists()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService
            .Setup(x => x.IsEmailAddressInUseAsync("existing@email.com"))
            .ReturnsAsync(true);

        var controller = new ServiceManagerController(mockUserService.Object);
        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "existing@email.com"
        };

        // Act
        var result = await controller.OnboardDeliveryPartnerAsync(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("OnboardDeliveryPartner");
        viewResult.Model.Should().BeEquivalentTo(viewModel);

        controller.ModelState.ContainsKey(nameof(viewModel.EmailAddress)).Should().BeTrue();
        controller.ModelState[nameof(viewModel.EmailAddress)]!.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("This email address is already in use.");
    }

    [Test]
    public void OnboardDeliveryPartnerPage_ShouldReturnViewWithNewViewModel()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var controller = new ServiceManagerController(mockUserService.Object);

        // Act
        var result = controller.OnboardDeliveryPartnerPage();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("OnboardDeliveryPartner");
        viewResult.Model.Should().BeOfType<OnboardNewDeliveryPartnerViewModel>();
    }
}