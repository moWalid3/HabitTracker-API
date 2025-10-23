using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services.Sorting;

namespace HabitTracker.UnitTests.Services
{
    public class SortMappingProviderTests
    {
        private readonly SortMappingProvider sortMappingProvider = new([HabitMappings.SortMapping]);

        [Fact]
        public void Validate_ShouldSuccess_WhenSortFieldIsValid()
        {
            // Arrange
            string sort = "name,type,createdAtUtc";

            // Act
            bool isMappingValid = sortMappingProvider.ValidateMappings<HabitDto, Habit>(sort);

            // Assert
            Assert.True(isMappingValid);
        }

        [Fact]
        public void Validate_ShouldFail_WhenSortFieldIsNotValidByWrongSeparator()
        {
            // Arrange
            string sort = "name,type-createdAtUtc";

            // Act
            bool isMappingValid = sortMappingProvider.ValidateMappings<HabitDto, Habit>(sort);

            // Assert
            Assert.False(isMappingValid);
        }

        [Fact]
        public void Validate_ShouldFail_WhenSortFieldIsNotValidByNotExistingField()
        {
            // Arrange
            string sort = "name,type,createdAtUtc,time";

            // Act
            bool isMappingValid = sortMappingProvider.ValidateMappings<HabitDto, Habit>(sort);

            // Assert
            Assert.False(isMappingValid);
        }
    }
}
