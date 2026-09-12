using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infra.Postgres.Persistence;

public static class EnumPropertyBuilderExtensions
{
    public static PropertyBuilder<TEnum> HasEnumComment<TEnum>(
        this PropertyBuilder<TEnum> propertyBuilder)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasComment(CreateComment<TEnum>());
    }

    public static PropertyBuilder<TEnum?> HasEnumComment<TEnum>(
        this PropertyBuilder<TEnum?> propertyBuilder)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasComment(CreateComment<TEnum>());
    }

    private static string CreateComment<TEnum>() where TEnum : struct, Enum
    {
        IEnumerable<string> entries = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(CreateEntry);
        return string.Join("; ", entries);
    }

    private static string CreateEntry(FieldInfo field)
    {
        DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
        if (string.IsNullOrWhiteSpace(attribute?.Description))
        {
            throw new InvalidOperationException(
                $"O enum {field.DeclaringType?.Name}.{field.Name} deve possuir DescriptionAttribute.");
        }
        string value = Convert.ToString(field.GetRawConstantValue(), CultureInfo.InvariantCulture)!;
        return $"{value} = {field.Name} — {attribute.Description}";
    }
}
