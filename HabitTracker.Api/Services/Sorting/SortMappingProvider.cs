﻿namespace HabitTracker.Api.Services.Sorting
{
    public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
    {
        public SortMapping[] GetMappings<TSource, TDestination>()
        {
            SortMappingDefinition<TSource, TDestination>? sortMappingDefinition = sortMappingDefinitions
                .OfType<SortMappingDefinition<TSource, TDestination>>()
                .FirstOrDefault();

            if (sortMappingDefinition == null)
            {
                throw new InvalidOperationException(
                    $"The mapping from '{typeof(TSource).Name}' into '{typeof(TDestination).Name}' is not defined");
            }

            return sortMappingDefinition.Mappings;
        }

        public bool ValidateMappings<TSource, TDestination>(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                return true;
            }

            var sortFields = sort.Split(',')
                .Select(f => f.Trim().Split(' ')[0])
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();

            SortMapping[] mappings = GetMappings<TSource, TDestination>();

            return sortFields.All(f => mappings.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
