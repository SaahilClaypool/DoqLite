using System.Text.Json;

namespace DoqLite;

public static class Extensions
{
    public static string ToJson<T>(this T t) =>
        JsonSerializer.Serialize(t, options: new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

    public static T ToObject<T>(this string t) =>
        JsonSerializer.Deserialize<T>(t, options: new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })!;

    public static string Join(this IEnumerable<string> items, string separator) =>
        string.Join(separator, items);

}