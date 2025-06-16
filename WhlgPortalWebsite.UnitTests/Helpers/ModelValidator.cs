using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tests.Helpers;

public static class ModelValidator
{
    public static IList<ValidationResult> ValidateModel(object viewModel)
    {
        var context = new ValidationContext(viewModel);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(viewModel, context, results, true);
        return results;
    }
}