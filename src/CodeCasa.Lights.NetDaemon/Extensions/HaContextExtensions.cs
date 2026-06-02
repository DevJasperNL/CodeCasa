using CodeCasa.Lights.NetDaemon.Generated;
using NetDaemon.HassModel;

namespace CodeCasa.Lights.NetDaemon.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHaContext"/> to support light entity operations.
/// </summary>
public static class HaContextExtensions
{
    /// <summary>
    /// Flattens a light entity by recursively resolving all its child lights.
    /// If the light is a group or composite light with children, returns all leaf light entities.
    /// If the light has no children, returns the light entity itself.
    /// </summary>
    /// <param name="haContext">The Home Assistant context.</param>
    /// <param name="lightEntityId">The entity ID of the light to flatten.</param>
    /// <returns>An array of entity IDs representing all leaf light entities.</returns>
    public static string[] FlattenLight(this IHaContext haContext, string lightEntityId)
    {
        return new LightEntity(haContext, lightEntityId).Flatten().Select(e => e.EntityId).ToArray();
    }

    /// <summary>
    /// Flattens multiple light entities by recursively resolving all their child lights.
    /// If any light is a group or composite light with children, includes all leaf light entities.
    /// Duplicate entity IDs are removed from the result.
    /// </summary>
    /// <param name="haContext">The Home Assistant context.</param>
    /// <param name="lightEntityIds">The entity IDs of the lights to flatten.</param>
    /// <returns>An array of unique entity IDs representing all leaf light entities.</returns>
    public static string[] FlattenLights(this IHaContext haContext, IEnumerable<string> lightEntityIds) =>
        haContext.FlattenLights(lightEntityIds.ToArray());

    /// <summary>
    /// Flattens multiple light entities by recursively resolving all their child lights.
    /// If any light is a group or composite light with children, includes all leaf light entities.
    /// Duplicate entity IDs are removed from the result.
    /// </summary>
    /// <param name="haContext">The Home Assistant context.</param>
    /// <param name="lightEntityIds">The entity IDs of the lights to flatten.</param>
    /// <returns>An array of unique entity IDs representing all leaf light entities.</returns>
    public static string[] FlattenLights(this IHaContext haContext, params string[] lightEntityIds)
    {
        return lightEntityIds.Select(id => new LightEntity(haContext, id)).SelectMany(le => le.Flatten())
            .Select(e => e.EntityId).Distinct().ToArray();
    }
}