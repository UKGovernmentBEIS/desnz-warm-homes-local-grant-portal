using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUkDesignSystem.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Enums;

namespace WhlgPortalWebsite.Models;

public class ConfirmCodesToDeliveryPartnerViewModel : IValidatableObject
{
    public User User { get; set; }
    public string Code { get; set; }
    public List<string> ManagedLocalAuthorityCodes { get; set; }
    public AuthorityType AuthorityType { get; set; }

    [ModelBinder(typeof(GovUkCheckboxBoolBinder))]
    public bool IsConfirmed { get; set; }

    public string GetAuthorityName()
    {
        return AuthorityType switch
        {
            AuthorityType.LocalAuthority => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[Code],
            AuthorityType.Consortium => ConsortiumData.ConsortiumNamesByConsortiumCode[Code],
            _ => throw new InvalidOperationException("Unknown authority type")
        };
    }

    public string GetAuthorityTypePlural()
    {
        return AuthorityType switch
        {
            AuthorityType.LocalAuthority => "Local Authorities",
            AuthorityType.Consortium => "Consortia",
            _ => throw new InvalidOperationException("Unknown authority type")
        };
    }

    public IEnumerable<string> GetAllManagedLocalAuthorityNames()
    {
        return ManagedLocalAuthorityCodes
            .Select(custodianCode => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[custodianCode]);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsConfirmed)
        {
            yield return new ValidationResult(
                "You must confirm the assignment to onboard.",
                [nameof(IsConfirmed)]);
        }
    }
}