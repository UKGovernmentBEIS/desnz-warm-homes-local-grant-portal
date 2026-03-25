using System;

namespace WhlgPortalWebsite.Enums;

public enum TaskSuccessMessage
{
    UserDeleted,
    LaAssigned,
    ConsortiumAssigned,
    JobRan,
    JobConfirmationRequired
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
            TaskSuccessMessage.JobConfirmationRequired => "You must confirm before running this job",
            _ => throw new ArgumentOutOfRangeException(nameof(TaskSuccessMessage), taskSuccessMessage, null)
        };
    }
}