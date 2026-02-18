using FluentValidation;
using HomeBanking.Contracts.Requests;

namespace HomeBanking.API.Validators;

/// <summary>
/// Validates the agent chat request body.
/// </summary>
public class AgentChatRequestValidator : AbstractValidator<AgentChatRequest>
{
    public AgentChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters");

        When(x => x.ConversationHistory != null, () =>
        {
            RuleFor(x => x.ConversationHistory!)
                .Must(h => h.Count <= 20).WithMessage("Conversation history must not exceed 20 messages");

            RuleForEach(x => x.ConversationHistory!).ChildRules(msg =>
            {
                msg.RuleFor(m => m.Role)
                    .NotEmpty().WithMessage("Role is required")
                    .Must(r => r == "user" || r == "assistant").WithMessage("Role must be 'user' or 'assistant'");

                msg.RuleFor(m => m.Content)
                    .NotEmpty().WithMessage("Content is required")
                    .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters");
            });
        });
    }
}
