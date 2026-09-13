namespace HierarchyKit;

/// <summary>
/// Provides operations for enumerable sequences.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Executes an action for each item in the sequence, in enumeration order.
    /// </summary>
    /// <typeparam name="T">The type of items in the sequence.</typeparam>
    /// <param name="source">The sequence to enumerate.</param>
    /// <param name="action">The action to execute for each item.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }
}
