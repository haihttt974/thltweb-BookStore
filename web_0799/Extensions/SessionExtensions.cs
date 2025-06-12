using System.Text.Json;

namespace web_0799.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var jsonString = JsonSerializer.Serialize(value);
            session.SetString(key, jsonString);
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var jsonString = session.GetString(key);
            return string.IsNullOrEmpty(jsonString)
                ? default
                : JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
