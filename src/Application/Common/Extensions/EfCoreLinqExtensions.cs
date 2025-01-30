using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SSW.Rewards.Application.Common.Extensions;

/// <summary>
/// JK's extension methods for EF Core LINQ queries.
/// </summary>
public static class EfCoreLinqExtensions
{
    /// <summary>
    /// This is an alternative to `.WithTag(comment)`.
    /// 
    /// You can use it like `.TagWithContext()` and this will auto-generated comment based on the class and method name ("Class-Method").
    /// Or you can add additional comment to it with `.TagWithContext("CheckAny")` (eg. "TwitterService-DeleteTweet-CheckAny")
    /// </summary>
    public static IQueryable<TSource> TagWithContext<TSource>(this IQueryable<TSource> source, string? message = null, [CallerFilePath] string callerFileName = "", [CallerMemberName] string callerName = "")
    {
        string tag = GenerateTagName(message, callerFileName, callerName);
        return source.TagWith(tag);
    }

    public static string GenerateTagName(string? message = null, string callerFileName = "", string callerName = "")
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            message = "-" + message;
        }

        string className = Path.GetFileNameWithoutExtension(callerFileName);
        return className + "-" + callerName + message;
    }

    /// <summary>
    /// Applies the specified predicate as a filter to the source query only if the given condition is true.
    /// </summary>
    /// <typeparam name="TSource">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="condition">The boolean condition to be checked.</param>
    /// <param name="predicate">The predicate to be applied as a filter when the condition is true.</param>
    /// <returns>A new query with the predicate applied as a filter if the condition is true, or the original query otherwise.</returns>
    public static IQueryable<TSource> WhenTrue<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        => condition
        ? source.Where(predicate)
        : source;

    /// <summary>
    /// Applies the specified predicate as a filter to the source query only if the provided string value is not empty.
    /// </summary>
    /// <typeparam name="TSource">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="value">The string value to be checked for emptiness.</param>
    /// <param name="predicate">The predicate to be applied as a filter when the string value is not empty.</param>
    /// <returns>A new query with the predicate applied as a filter if the string value is not empty, or the original query otherwise.</returns>
    public static IQueryable<TSource> WhenStringNotEmpty<TSource>(this IQueryable<TSource> source, string? value, Expression<Func<TSource, bool>> predicate)
        => !string.IsNullOrEmpty(value)
        ? source.Where(predicate)
        : source;

    /// <summary>
    /// Applies the specified predicate as a filter to the source query only if the provided list is not empty.
    /// </summary>
    /// <typeparam name="TSource">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="value">The List to be checked for emptiness.</param>
    /// <param name="predicate">The predicate to be applied as a filter when the List is not empty.</param>
    /// <returns>A new query with the predicate applied as a filter if the list is not empty, or the original query otherwise.</returns>
    public static IQueryable<TSource> WhenNotEmpty<TSource, TList>(this IQueryable<TSource> source, IEnumerable<TList> list, Expression<Func<TSource, bool>> predicate)
        => list != null && list.Any()
        ? source.Where(predicate)
        : source;

    public static IQueryable<TSource> ApplyPagination<TSource>(this IQueryable<TSource> source, IPagedRequest pagedRequest)
        => ApplyPagination(source, pagedRequest.Page, pagedRequest.PageSize);

    public static IQueryable<TSource> ApplyPagination<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        => source.Skip(page * pageSize).Take(pageSize);
}
