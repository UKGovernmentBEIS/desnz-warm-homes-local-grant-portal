using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUkDesignSystem.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Models;

public class ConfirmDeleteDeliveryPartnerViewModel : IValidatableObject
{
    public int UserId { get; set; }
    public string EmailAddress { get; set; } = string.Empty;

    [ModelBinder(typeof(GovUkCheckboxBoolBinder))]
    public bool IsConfirmed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsConfirmed)
        {
            yield return new ValidationResult(
                "You must confirm the user deletion.",
                [nameof(IsConfirmed)]);
        }
    }
}