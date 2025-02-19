using System.Text.Json;

namespace Magic.IndexedDb.Models
{
    public class MagicJsonSerializationSettings
    {
        private JsonSerializerOptions _options = new()
        {
            Converters = { new MagicContractResolver() }
        };
        public JsonSerializerOptions Options
        {
            get => _options;
            set => _options = value ?? new JsonSerializerOptions();
        }

        public bool UseCamelCase
        {
            get => _options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase;
            set
            {
                _options = new JsonSerializerOptions(_options) // Clone existing settings
                {
                    PropertyNamingPolicy = value ? JsonNamingPolicy.CamelCase : null
                };
            }
        }
    }
}
