using System.Runtime.Serialization;

namespace Lib;

public static class EnumHelper
{
    public static List<string> GetEnumValuesAsStringList<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<Enum>()
            .Select(GetEnumStringValue)
            .ToList();
    }

    public static string GetEnumStringValue<T>(T enumValue) where T : Enum
    {
        return typeof(T)
            .GetField(enumValue.ToString())?
            .GetCustomAttributes(typeof(EnumMemberAttribute), false)
            .Cast<EnumMemberAttribute>()
            .FirstOrDefault()?.Value ?? enumValue.ToString();
    }

    public static T? GetEnumValueFromString<T>(string value) where T : Enum
    {
        return (from field in typeof(T).GetFields()
            let attribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute))!
            where attribute.Value == value
            select (T)Enum.Parse(typeof(T), field.Name)).FirstOrDefault();
    }
}