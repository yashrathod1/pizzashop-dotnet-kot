using System.ComponentModel.DataAnnotations;

namespace pizzashop_repository.ViewModels;

public class TaxsAndFeesViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Tax Type is required")]
    public string Type { get; set; } = null!;

    [Required(ErrorMessage = "Tax Amount is required")]
    public decimal Value { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == "Percentage")
        {
            if (Value < 1 || Value > 100)
            {
                yield return new ValidationResult("Percentage value must be between 1 and 100.", new[] { nameof(Value) });
            }
        }
        else if (Type == "Flat Amount")
        {
            if (Value < 1 || Value > 10000)
            {
                yield return new ValidationResult("Flat value must be between 1 and 10,000.", new[] { nameof(Value) });
            }
        }
    }

}
