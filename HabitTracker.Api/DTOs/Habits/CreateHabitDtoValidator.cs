using FluentValidation;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.DTOs.Habits
{
    public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
    {
        private static readonly string[] AllowedUnits =
        [
            "minutes", "hours", "steps", "words", "km", "cal",
            "pages", "books", "tasks", "glasses", "sessions"
        ];

        private static readonly string[] AllowedUnitsForBinaryHabits = ["sessions", "tasks"];

        public CreateHabitDtoValidator()
        {
            RuleFor(h => h.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100)
                .WithMessage("Habit Name must be between 2 and 100 characters");

            RuleFor(h => h.Description)
                .MaximumLength(500)
                .When(h => h.Description != null)
                .WithMessage("Description can not exceed 500 characters");

            RuleFor(h => h.Type)
                .IsInEnum()
                .WithMessage("Invalid habit type");

            RuleFor(h => h.Frequency.Type)
                .IsInEnum()
                .WithMessage("Invalid frequency period");

            RuleFor(h => h.Frequency.TimesPerPeriod)
                .GreaterThan(0)
                .WithMessage("Frequency must be greater than 0");

            RuleFor(h => h.Target.Value)
                .GreaterThan(0)
                .WithMessage("Target value must be greater than 0");

            RuleFor(h => h.Target.Unit)
                .NotEmpty()
                .Must(unit => AllowedUnits.Contains(unit.ToLowerInvariant()))
                .WithMessage($"Unit must be one of: {string.Join(", ", AllowedUnits)}");

            RuleFor(h => h.EndDate)
                .Must(date => date == null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("End date must be in the future");

            When(h => h.Milestone != null, () =>
            {
                RuleFor(h => h.Milestone!.Target)
                    .GreaterThan(0)
                    .WithMessage("Milestone target must be greater than 0");
            });

            RuleFor(h => h.Target.Unit)
                .Must((habit, unit) => IsTargetUnitCompatibleWithType(habit.Type, unit))
                .WithMessage("Target unit is not compatible with the habit type");
        }

        private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
        {
            string normalizedUnit = unit.ToLowerInvariant();

            return type switch
            {
                HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit),
                HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
                _ => false
            };
        }
    }
}
