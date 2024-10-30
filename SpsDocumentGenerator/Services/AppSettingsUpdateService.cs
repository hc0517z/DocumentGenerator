using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpsDocumentGenerator.Services;

public class AppSettingsUpdateService
{
    private const string EmptyJson = "{}";

    public void UpdateAppSetting(string key, object value)
    {
        if (key == null) throw new ArgumentException("Json property key cannot be null", nameof(key));

        const string settingsFileName = "appsettings.json";
        if (!File.Exists(settingsFileName)) File.WriteAllText(settingsFileName, EmptyJson);
        var config = File.ReadAllText(settingsFileName);

        var updatedConfigDict = UpdateJson(key, value, config);
        var updatedJson = JsonSerializer.Serialize(updatedConfigDict, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(settingsFileName, updatedJson);
    }

    private Dictionary<string, object> UpdateJson(string key, object value, string jsonSegment)
    {
        const char keySeparator = ':';

        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonSegment);
        var keyParts = key.Split(keySeparator);
        var isKeyNested = keyParts.Length > 1;
        if (isKeyNested)
        {
            var firstKeyPart = keyParts[0];
            var remainingKey = string.Join(keySeparator, keyParts.Skip(1));

            // If the key does not exist already, we will create a new key and append it to the json
            var newJsonSegment = config.ContainsKey(firstKeyPart) && config[firstKeyPart] != null
                ? config[firstKeyPart].ToString()
                : EmptyJson;
            config[firstKeyPart] = UpdateJson(remainingKey, value, newJsonSegment);
        }
        else
        {
            config[key] = value;
        }

        return config;
    }
}