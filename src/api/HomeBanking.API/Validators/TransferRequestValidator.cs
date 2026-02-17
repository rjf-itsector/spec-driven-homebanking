using FluentValidation;
using HomeBanking.Contracts.Requests;

namespace HomeBanking.API.Validators;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Source account is required");
        
        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("Destination account is required")
            .NotEqual(x => x.FromAccountId).WithMessage("Cannot transfer to the same account");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount exceeds maximum transfer limit");
        
        RuleFor(x => x.Description)
            .MaximumLength(250).When(x => x.Description != null);
    }
}
