using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using Tests.Builders;
using WhlgPortalWebsite.ErrorHandling;

namespace Tests.Website.ErrorHandling;

[TestFixture]
public class ErrorHandlingFilterTests
{
    private ErrorHandlingFilter underTest;

    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        underTest = new ErrorHandlingFilter();
    }

    [Test]
    public void OnException_WhenUserIsDeleted_RedirectsToDeletedUserPage()
    {
        // Arrange
        var exception = new InvalidOperationException("User not found.");
        var context = CreateExceptionContext(exception);

        // Act
        underTest.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = context.Result as RedirectToActionResult;
        redirectToActionResult.ActionName.Should().Be("DeletedUser");
        redirectToActionResult.ControllerName.Should().Be("Error");
    }

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var httpContext = new HttpContextBuilder(EmailAddress).Build();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, [])
        {
            Exception = exception
        };
    }
}