using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using HerPortal.Models;
using NUnit.Framework;

namespace Tests.Website.Models;

[TestFixture]
public class HomepageViewModelTests
{
    [Test]
    public void ShouldShowBannerIfUserHasNotLoggedIn()
    {
        // Arrange
        var user = new User
        {
            HasLoggedIn = false,
        };
        
        // Act
        var viewModel = new HomepageViewModel(user);
        
        // Assert
        viewModel.ShouldShowBanner.Should().BeTrue();
    }
    
    [Test]
    public void ShouldNotShowBannerIfUserHasLoggedIn()
    {
        // Arrange
        var user = new User
        {
            HasLoggedIn = true,
        };
        
        // Act
        var viewModel = new HomepageViewModel(user);
        
        // Assert
        viewModel.ShouldShowBanner.Should().BeFalse();
    }
}