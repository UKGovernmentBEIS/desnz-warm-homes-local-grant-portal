using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Controllers;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Controllers;

[TestFixture]
public class ServiceManagerControllerTests
{
    private Mock<IUserService> mockUserService;
    private Mock<IAuthorityService> mockAuthorityService;
    private Mock<IReminderEmailsService> mockReminderEmailsService;
    private ServiceManagerController serviceManagerController;

    [SetUp]
    public void Setup()
    {
        mockUserService = new Mock<IUserService>();
        mockAuthorityService = new Mock<IAuthorityService>();
        mockReminderEmailsService = new Mock<IReminderEmailsService>();
        serviceManagerController = new ServiceManagerController(mockUserService.Object, mockAuthorityService.Object,
            mockReminderEmailsService.Object);
    }

    [Test]
    public async Task OnboardDeliveryPartnerPost_ShouldRedirectWhenEmailIsValid()
    {
        // Arrange
        mockUserService
            .Setup(x => x.IsEmailAddressInUseAsync("new@email.com"))
            .ReturnsAsync(false);
        mockUserService
            .Setup(x => x.CreateDeliveryPartnerAsync("new@email.com"))
            .ReturnsAsync(new User());

        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "new@email.com"
        };

        // Act
        var result = await serviceManagerController.OnboardDeliveryPartner_Post(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("AssignCodesToDeliveryPartner_Get");
        redirect.ControllerName.Should().Be("ServiceManager");

        mockUserService.Verify(x => x.CreateDeliveryPartnerAsync("new@email.com"), Times.Once);
    }

    [Test]
    public async Task OnboardDeliveryPartnerPost_ShouldReturnViewWithModelErrorWhenEmailAlreadyExists()
    {
        // Arrange
        mockUserService
            .Setup(x => x.IsEmailAddressInUseAsync("existing@email.com"))
            .ReturnsAsync(true);

        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = "existing@email.com"
        };

        // Act
        var result = await serviceManagerController.OnboardDeliveryPartner_Post(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("OnboardDeliveryPartner");
        viewResult.Model.Should().BeEquivalentTo(viewModel);

        serviceManagerController.ModelState.ContainsKey(nameof(viewModel.EmailAddress)).Should().BeTrue();
        serviceManagerController.ModelState[nameof(viewModel.EmailAddress)]!.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("This email address is already in use.");
    }

    [Test]
    public async Task OnboardDeliveryPartnerPost_ShouldStripSpacesFromEmailAddress()
    {
        // Arrange
        mockUserService
            .Setup(x => x.IsEmailAddressInUseAsync("new@email.com"))
            .ReturnsAsync(false);
        mockUserService
            .Setup(x => x.CreateDeliveryPartnerAsync("new@email.com"))
            .ReturnsAsync(new User());

        var viewModel = new OnboardNewDeliveryPartnerViewModel
        {
            EmailAddress = " new@email.com "
        };

        // Act
        await serviceManagerController.OnboardDeliveryPartner_Post(viewModel);

        // Assert
        mockUserService.Verify(x => x.IsEmailAddressInUseAsync("new@email.com"), Times.Once);
        mockUserService.Verify(x => x.CreateDeliveryPartnerAsync("new@email.com"), Times.Once);
        mockUserService.VerifyNoOtherCalls();
    }

    [Test]
    public void OnboardDeliveryPartnerGet_ShouldReturnViewWithNewViewModel()
    {
        // Act
        var result = serviceManagerController.OnboardDeliveryPartner_Get();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("OnboardDeliveryPartner");
        viewResult.Model.Should().BeOfType<OnboardNewDeliveryPartnerViewModel>();
    }

    [Test]
    public async Task AssignCodesToDeliveryPartnerGet_ShouldReturnViewWithNewViewModel()
    {
        // Arrange
        mockUserService.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        mockAuthorityService.Setup(x => x.SearchAllLasAsync(null)).ReturnsAsync(new List<LocalAuthority>());
        mockAuthorityService.Setup(x => x.SearchAllConsortiaAsync(null)).ReturnsAsync(new List<Consortium>());

        // Act
        var result = await serviceManagerController.AssignCodesToDeliveryPartner_Get(1, null);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("AssignCodesToDeliveryPartner");
        viewResult.Model.Should().BeOfType<AssignCodesToDeliveryPartnerViewModel>();
    }

    [Test]
    public async Task ConfirmAuthorityCodeToDeliveryPartnerGet_ShouldReturnViewWithNewViewModel()
    {
        // Act
        var result =
            await serviceManagerController.ConfirmAuthorityCodeToDeliveryPartner_Get(0, AuthorityType.LocalAuthority,
                "test-code");

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("ConfirmCodeToDeliveryPartner");
        viewResult.Model.Should().BeOfType<ConfirmCodesToDeliveryPartnerViewModel>();
    }

    [Test]
    public async Task ConfirmAuthorityCodeToDeliveryPartnerPost_ShouldReturnToConfirmPageOnError()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel();
        serviceManagerController.ModelState.AddModelError(nameof(viewModel.IsConfirmed), "Example error");

        // Act
        var result =
            await serviceManagerController.ConfirmAuthorityCodeToDeliveryPartner_Post(viewModel, 1,
                AuthorityType.LocalAuthority, "test-code");

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("ConfirmCodeToDeliveryPartner");
    }

    [Test]
    public async Task ConfirmAuthorityCodeToDeliveryPartnerPost_ShouldReturnToIndexOnSuccess()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel();

        // Act
        var result =
            await serviceManagerController.ConfirmAuthorityCodeToDeliveryPartner_Post(viewModel, 1,
                AuthorityType.LocalAuthority, "test-code");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var viewResult = result as RedirectToActionResult;
        viewResult!.ActionName.Should().Be("Index");
    }

    [Test]
    public async Task ConfirmAuthorityCodeToDeliveryPartnerPost_ShouldCallOnboardLaMethodsOnSuccess()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel();
        var user = new User { Id = 1 };
        const string custodianCode = "test-code";
        var localAuthority = new LocalAuthority { CustodianCode = custodianCode };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        mockAuthorityService.Setup(x => x.GetLocalAuthorityByCustodianCodeAsync(custodianCode))
            .ReturnsAsync(localAuthority);

        // Act
        await serviceManagerController.ConfirmAuthorityCodeToDeliveryPartner_Post(viewModel, user.Id,
            AuthorityType.LocalAuthority, custodianCode);

        // Assert
        mockUserService.Verify(x => x.GetUserByIdAsync(user.Id), Times.Once);
        mockUserService.Verify(x => x.AddLaToDeliveryPartnerAsync(user, localAuthority), Times.Once);
        mockUserService.VerifyNoOtherCalls();

        mockAuthorityService.Verify(x => x.GetLocalAuthorityByCustodianCodeAsync(custodianCode),
            Times.Once);
        mockAuthorityService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ConfirmAuthorityCodeToDeliveryPartnerPost_ShouldCallOnboardConsortiumMethodsOnSuccess()
    {
        // Arrange
        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel();
        var user = new User { Id = 1 };
        const string consortiumCode = "test-code";
        var consortium = new Consortium { ConsortiumCode = consortiumCode };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        mockAuthorityService.Setup(x => x.GetConsortiumByConsortiumCodeAsync(consortiumCode))
            .ReturnsAsync(consortium);

        // Act
        await serviceManagerController.ConfirmAuthorityCodeToDeliveryPartner_Post(viewModel, user.Id,
            AuthorityType.Consortium, consortiumCode);

        // Assert
        mockUserService.Verify(x => x.GetUserByIdAsync(user.Id), Times.Once);
        mockUserService.Verify(x => x.AddConsortiumToDeliveryPartnerAsync(user, consortium), Times.Once);
        mockUserService.VerifyNoOtherCalls();

        mockAuthorityService.Verify(x => x.GetConsortiumByConsortiumCodeAsync(consortiumCode), Times.Once);
        mockAuthorityService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SendReminderEmailsPost_CallsReminderEmailsJob()
    {
        // Act
        await serviceManagerController.SendReminderEmails_Post();

        // Assert
        mockReminderEmailsService.Verify(x => x.SendReminderEmailsAsync(), Times.Once);
        mockReminderEmailsService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ConfirmDeleteDeliveryPartnerUserPost_ShouldCallDeleteUserMethodsOnSuccess()
    {
        // Arrange
        var viewModel = new ConfirmDeleteDeliveryPartnerViewModel();
        var user = new User { Id = 1 };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        
        // Act
        await serviceManagerController.ConfirmDeleteDeliveryPartner_Post(viewModel, user.Id);
        
        // Assert
        mockUserService.Verify(x => x.GetUserByIdAsync(user.Id), Times.Once);
        mockUserService.Verify(x => x.DeleteUserAsync(user), Times.Once);
        mockUserService.VerifyNoOtherCalls();
        mockAuthorityService.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task ConfirmDeleteDeliveryPartnerUserPost_ShouldReturnToIndexOnSuccess()
    {
        // Arrange
        var viewModel = new ConfirmDeleteDeliveryPartnerViewModel();
        var user = new User { Id = 1 };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        mockUserService.Setup(x => x.DeleteUserAsync(user)).Returns(Task.CompletedTask);

        // Act
        var result = await serviceManagerController.ConfirmDeleteDeliveryPartner_Post(viewModel, 1);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var viewResult = result as RedirectToActionResult;
        viewResult!.ActionName.Should().Be("Index");
    }
    
    [Test]
    public async Task ConfirmDeleteDeliveryPartnerUserPost_ShouldReturnToConfirmPageOnError()
    {
        // Arrange
        var viewModel = new ConfirmDeleteDeliveryPartnerViewModel();
        var user = new User { Id = 1 };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        mockUserService.Setup(x => x.DeleteUserAsync(user)).Returns(Task.CompletedTask);
        serviceManagerController.ModelState.AddModelError(nameof(viewModel.IsConfirmed), "Example error");

        // Act
        var result = await serviceManagerController.ConfirmDeleteDeliveryPartner_Post(viewModel, 1);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("ConfirmDeleteDeliveryPartner");
    }
}