using System;

namespace WhlgPortalWebsite.Helpers;

public static class ExceptionHelper
{
    public static bool IsUserNotFoundException(this Exception exception)
    {
        return exception is InvalidOperationException ex && ex.Message.Contains("User not found.");
    }
}