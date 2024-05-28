using System.Collections.Generic;
using HerPortal.BusinessLogic.Models;

namespace Tests.Builders;

public class UserBuilder
{
    private User user;

    public UserBuilder(string emailAddress)
    {
        user = new User
        {
            Id = 13,
            EmailAddress = emailAddress,
            HasLoggedIn = true,
            LocalAuthorities = new List<LocalAuthority>(),
            Consortia = new List<Consortium>()
        };
    }

    public User Build()
    {
        return user;
    }

    public UserBuilder WithLocalAuthorities(List<LocalAuthority> localAuthorities)
    {
        user.LocalAuthorities = localAuthorities;
        return this;
    }

    public UserBuilder WithConsortia(List<Consortium> consortia)
    {
        user.Consortia = consortia;
        return this;
    }

    public UserBuilder WithHasLoggedIn(bool hasLoggedIn)
    {
        user.HasLoggedIn = hasLoggedIn;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        user.EmailAddress = email;
        return this;
    }
}

