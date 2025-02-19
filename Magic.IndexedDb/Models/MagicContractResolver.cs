using Magic.IndexedDb.SchemaAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Magic.IndexedDb.Models
{
    internal class MagicContractResolver : JsonConverter<object>
    {
        private static readonly ConcurrentDictionary<MemberInfo, bool> _cachedIgnoredProperties = new();

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.GetCustomAttribute<MagicTableAttribute>() is not null;
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null) return;

            writer.WriteStartObject();

            foreach (var property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Check cache first
                if (!_cachedIgnoredProperties.TryGetValue(property, out bool shouldIgnore))
                {
                    shouldIgnore = property.GetCustomAttribute<MagicNotMappedAttribute>() is null;
                    _cachedIgnoredProperties[property] = shouldIgnore;
                }

                if (!shouldIgnore)
                {
                    var propValue = property.GetValue(value);
                    var propName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                    writer.WritePropertyName(propName);
                    JsonSerializer.Serialize(writer, propValue, property.PropertyType, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
