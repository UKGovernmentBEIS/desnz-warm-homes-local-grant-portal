﻿namespace HerPortal.BusinessLogic.ExternalServices.EmailSending;

public class EmailSenderException : Exception
{
    public readonly EmailSenderExceptionType Type;
    
    public EmailSenderException(EmailSenderExceptionType type)
    {
        Type = type;
    }
}

public enum EmailSenderExceptionType
{
    InvalidEmailAddress,
    Other
}