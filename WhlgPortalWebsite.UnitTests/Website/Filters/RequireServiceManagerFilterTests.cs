using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Tests.Builders;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Filters;

namespace Tests.Website.Filters;

[TestFixture]
public class RequiresServiceManagerFilterTests
{
    private RequiresServiceManagerFilter underTest;
    private Mock<IUserService> mockUserService;

    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockUserService = new Mock<IUserService>();
        underTest = new RequiresServiceManagerFilter(mockUserService.Object);
    }

    [Test]
    public async Task OnAuthorizationAsync_WhenUserIsDeleted_RedirectsToDeletedUserPage()
    {
        // Arrange
        mockUserService.Setup(x => x.GetUserByEmailAsync(EmailAddress))
            .ThrowsAsync(new InvalidOperationException("User not found."));
        var authorizationContext = CreateAuthorizationContext();

        // Act
        await underTest.OnAuthorizationAsync(authorizationContext);

        // Assert
        authorizationContext.Result.Should().BeOfType<RedirectToActionResult>();
        var redirectToActionResult = authorizationContext.Result as RedirectToActionResult;
        redirectToActionResult.ActionName.Should().Be("DeletedUser");
        redirectToActionResult.ControllerName.Should().Be("Error");
    }

    private static AuthorizationFilterContext CreateAuthorizationContext()
    {
        var httpContext = new HttpContextBuilder(EmailAddress).Build();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }
}