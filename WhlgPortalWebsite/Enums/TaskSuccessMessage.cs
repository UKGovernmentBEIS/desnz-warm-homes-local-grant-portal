using System;

namespace WhlgPortalWebsite.Enums;
public enum TaskSuccessMessage
{
    NoSucessMessage,
    UserAddedSuccessfully,
    UserDeletedSuccessfully,
    LaAssignedSuccessfully,
    ConsortiumAssignedSuccessfully,
    JobRanSuccessfully
}

public static class TaskSuccessMessageExtensions
{
    public static string Parse(this TaskSuccessMessage taskSuccessMessage)
    {
        return taskSuccessMessage switch
        {
            TaskSuccessMessage.NoSucessMessage => string.Empty,
            TaskSuccessMessage.UserAddedSuccessfully => "User added successfully",
            TaskSuccessMessage.UserDeletedSuccessfully => "User deleted successfully",
            TaskSuccessMessage.LaAssignedSuccessfully => "Local Authority assigned successfully",
            TaskSuccessMessage.ConsortiumAssignedSuccessfully => "Consortium assigned successfully",
            TaskSuccessMessage.JobRanSuccessfully => "Job ran successfully",
            _ => throw new ArgumentOutOfRangeException(nameof(TaskSuccessMessage), taskSuccessMessage, null)
        };
    }
}