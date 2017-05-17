using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

public static class Tools
{
    /// <summary>
    /// Gets permissive serializer settings
    /// </summary>
    private static JsonSerializerSettings Settings = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore };

    /// <summary>
    /// Converts an object to JSON
    /// </summary>
    /// <param name="o">Object</param>
    /// <param name="Pretty">Pretty print</param>
    /// <returns>JSON</returns>
    public static string ToJson(this object o, bool Pretty = false)
    {
        return JsonConvert.SerializeObject(o, Pretty ? Formatting.Indented : Formatting.None);
    }

    /// <summary>
    /// Converts a JSON string to an object
    /// </summary>
    /// <typeparam name="T">Type of serialized object</typeparam>
    /// <param name="S">JSON</param>
    /// <param name="Default">Default in case of errors</param>
    /// <returns>Deserialized object on success and "Default" on error</returns>
    public static T FromJson<T>(this string S, T Default)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(S, Settings);
        }
        catch
        {
            return Default;
        }
    }

    /// <summary>
    /// Converts a JSON string to an object
    /// </summary>
    /// <typeparam name="T">Type of serialized object</typeparam>
    /// <param name="S">JSON</param>
    /// <returns>Deserialized object on success and default on error</returns>
    /// <remarks>Default means "null" for nullable types and "default(T)" for non-nullable types</remarks>
    public static T FromJson<T>(this string S)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(S, Settings);
        }
        catch
        {
            return default(T);
        }
    }

    public static byte[] ReadToEnd(this Stream stream)
    {
        using (var MS = new MemoryStream())
        {
            stream.CopyTo(MS);
            return MS.ToArray();
        }
    }

    public static bool IsAlphaNum(this string s)
    {
        Regex r = new Regex("^[a-zA-Z0-9]*$");
        return r.IsMatch(s);
    }
}