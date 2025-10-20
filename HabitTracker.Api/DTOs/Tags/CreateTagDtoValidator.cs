using FluentValidation;

namespace HabitTracker.Api.DTOs.Tags
{
    public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
    {
        public CreateTagDtoValidator()
        {
            RuleFor(t => t.Name).NotEmpty().MinimumLength(2);

            RuleFor(t => t.Description).MaximumLength(100);
        }
    }
}
