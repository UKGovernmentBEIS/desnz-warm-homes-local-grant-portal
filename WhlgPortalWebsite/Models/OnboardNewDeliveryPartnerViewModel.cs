using System.ComponentModel.DataAnnotations;

namespace WhlgPortalWebsite.Models;

public class OnboardNewDeliveryPartnerViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string EmailAddress { get; set; } = string.Empty;
}