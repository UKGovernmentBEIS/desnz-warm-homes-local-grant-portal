using System;

namespace WhlgPortalWebsite.Enums;

public enum TaskSuccessMessage
{
    UserDeleted,
    LaAssigned,
    ConsortiumAssigned,
    JobRan
}

public static class TaskSuccessMessageExtensions
{
    public static string Parse(this TaskSuccessMessage taskSuccessMessage)
    {
        return taskSuccessMessage switch
        {
            TaskSuccessMessage.UserDeleted => "User deleted successfully",
            TaskSuccessMessage.LaAssigned => "Local Authority assigned successfully",
            TaskSuccessMessage.ConsortiumAssigned => "Consortium assigned successfully",
            TaskSuccessMessage.JobRan => "Job ran successfully",
            _ => throw new ArgumentOutOfRangeException(nameof(TaskSuccessMessage), taskSuccessMessage, null)
        };
    }
}