using FluentValidation.Results;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;

namespace HabitTracker.UnitTests.Validators
{
    public class CreateHabitDtoValidatorTests
    {
        private readonly CreateHabitDtoValidator validator = new();

        [Fact]
        public async Task Validate_ShouldSucceed_WhenInputDtoIsValid()
        {
            // Arrange
            CreateHabitDto dto = new()
            {
                Name = "Anything",
                Type = HabitType.Measurable,
                Frequency = new FrequencyDto
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new TargetDto
                {
                    Value = 30,
                    Unit = "minutes"
                },
            };

            // Act
            ValidationResult validationResult = await validator.ValidateAsync(dto);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        public async Task Validate_ShouldFail_WhenNameIsEmptyOrLessThanTwoChars()
        {
            // Arrange
            CreateHabitDto dto = new()
            {
                Name = "",
                Type = HabitType.Measurable,
                Frequency = new FrequencyDto
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new TargetDto
                {
                    Value = 30,
                    Unit = "minutes"
                },
            };

            // Act
            ValidationResult validationResult = await validator.ValidateAsync(dto);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Equal(2, validationResult.Errors.Count);
            Assert.Equal(nameof(CreateHabitDto.Name), validationResult.Errors.Select(e => e.PropertyName).ElementAt(0));
            Assert.Equal(nameof(CreateHabitDto.Name), validationResult.Errors.Select(e => e.PropertyName).ElementAt(1));
        }

        [Fact]
        public async Task Validate_ShouldFail_WhenTypeIsNotInEnumSoAlsoIsNotCompatibleWithUnit()
        {
            // Arrange
            CreateHabitDto dto = new()
            {
                Name = "Anything",
                Type = (HabitType)3,
                Frequency = new FrequencyDto
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new TargetDto
                {
                    Value = 30,
                    Unit = "minutes"
                },
            };

            // Act
            ValidationResult validationResult = await validator.ValidateAsync(dto);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Equal(2, validationResult.Errors.Count);
            Assert.Contains(nameof(CreateHabitDto.Type), validationResult.Errors.Select(e => e.PropertyName));
            Assert.Contains(
                $"{nameof(CreateHabitDto.Target)}.{nameof(TargetDto.Unit)}",
                validationResult.Errors.Select(e => e.PropertyName));
        }
    }
}
