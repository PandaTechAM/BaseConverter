﻿using System.Linq.Expressions;
using BaseConverter;
using System.Text.Json;
using System.Text.Json.Serialization;

public class MyDataModel
{
    [PandaPropertyBaseConverter]
    public long MyLongValue { get; set; }

    [PandaPropertyBaseConverter]
    public long? MyNullableLongValue { get; set; }
}

internal  class PandaJsonBaseConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var method = typeof(PandaBaseConverter).GetMethod("Base36ToBase10");

        var ss = reader.GetString();
        
        var call = Expression.Call(null, method, Expression.Constant(ss));

        var lamda = Expression.Lambda<Func<T>>(
            Expression.Convert(call, typeof(T)));
        return lamda.Compile().Invoke();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            throw new JsonException("This converter should only be used for non-nullable value types.");
        }

        writer.WriteStringValue(PandaBaseConverter.Base10ToBase36(Convert.ToInt64(value)));
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(T);
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class PandaPropertyBaseConverterAttribute : JsonConverterAttribute
{
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        var converterType = typeof(PandaJsonBaseConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

