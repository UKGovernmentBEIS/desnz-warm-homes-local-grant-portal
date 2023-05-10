using HerPortal.BusinessLogic.Models;

namespace HerPortal.Models;

public class HomepageViewModel
{
    public bool ShouldShowBanner { get; }

    public HomepageViewModel(User user)
    {
        ShouldShowBanner = !user.HasLoggedIn;
    }
}
