using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using HerPortal.Models;
using NUnit.Framework;

namespace Tests.Website.Models;

[TestFixture]
public class HomepageViewModelTests
{
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void HomepageViewModel_OnlyWhenCreatedForUserThatHasntLoggedInBefore_ShouldShowBanner
    (
        bool hasUserLoggedIn,
        bool shouldShowBanner
    ) {
        // Arrange
        var user = new User
        {
            HasLoggedIn = hasUserLoggedIn,
        };
        
        // Act
        var viewModel = new HomepageViewModel(user);
        
        // Assert
        viewModel.ShouldShowBanner.Should().Be(shouldShowBanner);
    }
}