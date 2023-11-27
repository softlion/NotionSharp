using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient.Lib.Helpers;


/// <summary>
/// Don't forget to also add [JsonConverter(typeof(BufferedJsonPolymorphicConverterFactory))]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public sealed class BufferedJsonPolymorphicAttribute : JsonAttribute
{
    /// <summary>
    /// Gets or sets a custom type discriminator property name for the polymorphic type.
    /// Uses the default '$type' property name if left unset.
    /// </summary>
    public string? TypeDiscriminatorPropertyName { get; set; }

    /// <summary>
    /// Gets or sets the behavior when serializing an undeclared derived runtime type.
    /// </summary>
    public JsonUnknownDerivedTypeHandling UnknownDerivedTypeHandling { get; set; }

    /// <summary>
    /// When set to <see langword="true"/>, instructs the deserializer to ignore any
    /// unrecognized type discriminator id's and reverts to the contract of the base type.
    /// Otherwise, it will fail the deserialization.
    /// </summary>
    public bool IgnoreUnrecognizedTypeDiscriminators { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class BufferedJsonDerivedTypeAttribute : JsonAttribute
{
    // /// <summary>
    // /// Initializes a new attribute with specified parameters.
    // /// </summary>
    // /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared based type.</param>
    // public BufferedJsonDerivedTypeAttribute(Type derivedType)
    // {
    //     DerivedType = derivedType;
    // }

    /// <summary>
    /// Initializes a new attribute with specified parameters.
    /// </summary>
    /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared base type.</param>
    /// <param name="typeDiscriminator">The type discriminator identifier to be used for the serialization of the subtype.</param>
    public BufferedJsonDerivedTypeAttribute(Type derivedType, string typeDiscriminator)
    {
        DerivedType = derivedType;
        TypeDiscriminator = typeDiscriminator;
    }

    // /// <summary>
    // /// Initializes a new attribute with specified parameters.
    // /// </summary>
    // /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared base type.</param>
    // /// <param name="typeDiscriminator">The type discriminator identifier to be used for the serialization of the subtype.</param>
    // public BufferedJsonDerivedTypeAttribute(Type derivedType, int typeDiscriminator)
    // {
    //     DerivedType = derivedType;
    //     TypeDiscriminator = typeDiscriminator;
    // }

    /// <summary>
    /// A derived type that should be supported in polymorphic serialization of the declared base type.
    /// </summary>
    public Type DerivedType { get; }

    /// <summary>
    /// The type discriminator identifier to be used for the serialization of the subtype.
    /// </summary>
    public object? TypeDiscriminator { get; }
}

public sealed class BufferedJsonPolymorphicConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) 
        => typeToConvert.GetCustomAttributes(typeof(BufferedJsonPolymorphicAttribute), false).FirstOrDefault() != null;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeToConvert;
        var jsonPolymorphicAttribute = (BufferedJsonPolymorphicAttribute)type.GetCustomAttributes(typeof(BufferedJsonPolymorphicAttribute), false).First();
        var derivedTypes = type.GetCustomAttributes(typeof(BufferedJsonDerivedTypeAttribute), false)
            .Cast<BufferedJsonDerivedTypeAttribute>()
            .ToDictionary(j => JsonEncodedText.Encode((string)j.TypeDiscriminator!), j => j.DerivedType);

        return (JsonConverter)Activator.CreateInstance(typeof(BufferedJsonPolymorphicConverter<>).MakeGenericType(typeToConvert), jsonPolymorphicAttribute, derivedTypes)!;
    }

    sealed class BufferedJsonPolymorphicConverter<T> : JsonConverter<T>
    {
        private readonly BufferedJsonPolymorphicAttribute jsonPolymorphicAttribute;
        private readonly Dictionary<JsonEncodedText, Type> derivedTypes;
        private readonly JsonEncodedText typeDiscriminatorPropertyName;

        public BufferedJsonPolymorphicConverter(BufferedJsonPolymorphicAttribute jsonPolymorphicAttribute, Dictionary<JsonEncodedText, Type> derivedTypes)
        {
            this.jsonPolymorphicAttribute = jsonPolymorphicAttribute;
            this.derivedTypes = derivedTypes;

            typeDiscriminatorPropertyName = JsonEncodedText.Encode(jsonPolymorphicAttribute.TypeDiscriminatorPropertyName!);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var nestedReader = reader;
            if (nestedReader.TokenType == JsonTokenType.None)
                nestedReader.Read();

            while (nestedReader.Read())
            {
                if (nestedReader.TokenType == JsonTokenType.PropertyName && nestedReader.ValueTextEquals(typeDiscriminatorPropertyName.EncodedUtf8Bytes))
                {
                    nestedReader.Read();
                    // Resolve the type from the discriminator value
                    var subType = GetSubType(ref nestedReader);
                    // Perform the actual deserialization with the original reader
                    return (T)JsonSerializer.Deserialize(ref reader, subType, options)!;
                }

                if (nestedReader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    // Skip until TokenType is EndObject/EndArray
                    // Skip() always throws if IsFinalBlock == false, even when it could actually skip.
                    // We therefore use TrySkip() and if that is impossible, we simply return without advancing the original reader.
                    // For reference, see:
                    // https://stackoverflow.com/questions/63038334/how-do-i-handle-partial-json-in-a-jsonconverter-while-using-deserializeasync-on
                    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Reader/Utf8JsonReader.cs#L310-L318
                    if (!nestedReader.TrySkip())
                    {
                        // Do not advance the reader, i.e. do not set reader = nestedReader,
                        // as that would cause us to have an incomplete object available for full final deserialization.

                        // NOTE: This does not actually work.
                        // The System.Text.Json reader will throw that the converter consumed too few bytes.
                        return default!;
                    }
                }

                if (nestedReader.TokenType == JsonTokenType.EndObject)
                {
                    // We have exhausted the search within the object for a discriminator but it was not found.
                    break;
                }
            }

            // if (DefaultType is not null)
            //     return (T)JsonSerializer.Deserialize(ref reader, DefaultType, options)!;

            if (reader.IsFinalBlock && !jsonPolymorphicAttribute.IgnoreUnrecognizedTypeDiscriminators)
                throw new JsonException($"Unable to find discriminator property '{typeDiscriminatorPropertyName}'");

            return default!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize<object>(writer, value!, options);

        private Type GetSubType(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                // if (reader.TokenType == JsonTokenType.Null && DefaultType is not null)
                //     return DefaultType;
                throw new JsonException($"Expected string discriminator value, got '{reader.TokenType}'");
            }

            foreach (var (subValue, subType) in derivedTypes)
            {
                if (reader.ValueTextEquals(subValue.EncodedUtf8Bytes))
                    return subType;
            }

            throw new JsonException($"'{reader.GetString()}' is not a valid discriminator value");
        }
    }
}


