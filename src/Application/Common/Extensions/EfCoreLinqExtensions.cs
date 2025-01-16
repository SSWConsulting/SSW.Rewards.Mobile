using System.Runtime.CompilerServices;

namespace SSW.Rewards.Application.Common.Extensions;

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
}
