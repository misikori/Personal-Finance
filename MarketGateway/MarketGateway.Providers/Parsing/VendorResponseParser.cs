using System.Text.Json;
using MarketGateway.Interfaces;
using MarketGateway.Shared.Configuration;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Application;

public sealed class VendorResponseParser : IVendorResponseParser
{
    public MarketDataResultBase Parse(VendorConfig config, MarketDataRequest request, string json)
    {
        var endpoint = config.Endpoints.Values.FirstOrDefault(e => e.DataType == request.Type)
                       ?? throw new InvalidOperationException("Endpoint for requested type is not supported");

        using var doc = JsonDocument.Parse(json);
        var root = ResolveRoot(doc.RootElement, endpoint.Response.RootPath!);

        var result = MarketDataResultFactory.Create(endpoint.DataType);
        result.Vendor = config.Vendor;
        result.Type   = endpoint.DataType;

        // Optional timestamp
        if (!string.IsNullOrWhiteSpace(endpoint.Response.TimestampKey))
        {
            var tsProp  = result.GetType().GetProperty("Timestamp");
            var tsValue = ReadByPath(root, endpoint.Response.TimestampKey);
            if (tsProp != null && !string.IsNullOrWhiteSpace(tsValue)
                && DateTime.TryParse(tsValue, System.Globalization.CultureInfo.InvariantCulture,
                                     System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
            {
                tsProp.SetValue(result, DateTime.SpecifyKind(dt, DateTimeKind.Utc));
            }
        }
        
        foreach (var kv in endpoint.Response.FieldMappings)
        {
            var targetProp = result.GetType().GetProperty(kv.Key);
            if (targetProp is null) continue;

            string? valueStr = ExtractMappedValue(root, kv.Value);
            if (valueStr is null)
            {
                continue;
            }

            if (TryConvert(valueStr, targetProp.PropertyType, out var typedValue))
            {
                targetProp.SetValue(result, typedValue);
            }
            else
            {
                if (IsNullable(targetProp.PropertyType))
                    targetProp.SetValue(result, null);
            }
        }

        return result;
    }

    private static JsonElement ResolveRoot(JsonElement root, string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath)) return root;
        var cur = root;
        foreach (var seg in rootPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (cur.ValueKind == JsonValueKind.Object && cur.TryGetProperty(seg, out var next)) { cur = next; continue; }
            var match = cur.ValueKind == JsonValueKind.Object
                ? cur.EnumerateObject().FirstOrDefault(p => p.Name.Equals(seg, StringComparison.OrdinalIgnoreCase))
                : default;
            if (!match.Equals(default(JsonProperty))) cur = match.Value; else return root;
        }
        return cur;
    }

    private static string? ReadByPath(JsonElement cur, string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        if (cur.ValueKind == JsonValueKind.Object)
        {
            if (cur.TryGetProperty(path, out var exact)) return Stringify(exact);
            foreach (var prop in cur.EnumerateObject())
                if (string.Equals(prop.Name, path, StringComparison.OrdinalIgnoreCase))
                    return Stringify(prop.Value);
        }

        foreach (var seg in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (cur.ValueKind == JsonValueKind.Array && int.TryParse(seg, out var idx))
            { if (idx < 0 || idx >= cur.GetArrayLength()) return null; cur = cur[idx]; continue; }

            if (cur.ValueKind == JsonValueKind.Object && cur.TryGetProperty(seg, out var next))
            { cur = next; continue; }

            var match = cur.ValueKind == JsonValueKind.Object
                ? cur.EnumerateObject().FirstOrDefault(p => p.Name.Equals(seg, StringComparison.OrdinalIgnoreCase))
                : default;
            if (!match.Equals(default(JsonProperty))) cur = match.Value; else return null;
        }
        return Stringify(cur);

        static string? Stringify(JsonElement el) => el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.ToString(),
            JsonValueKind.True or JsonValueKind.False => el.ToString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => el.ToString()
        };
    }
    
    private static bool IsNullable(Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

    private static bool TryConvert(string? valueStr, Type targetType, out object? value)
    {
        value = null;
        if (valueStr is null) return false;

        var culture = System.Globalization.CultureInfo.InvariantCulture;
        var isNullable = IsNullable(targetType);
        var underlying = isNullable ? Nullable.GetUnderlyingType(targetType)! : targetType;

        // string
        if (underlying == typeof(string)) { value = valueStr; return true; }

        // bool
        if (underlying == typeof(bool))
        {
            if (bool.TryParse(valueStr, out var b)) { value = b; return true; }
            return false;
        }

        // ints / long
        if (underlying == typeof(int))
        {
            if (int.TryParse(valueStr, System.Globalization.NumberStyles.Integer, culture, out var i))
            { value = i; return true; }
            return false;
        }
        if (underlying == typeof(long))
        {
            if (long.TryParse(valueStr, System.Globalization.NumberStyles.Integer, culture, out var l))
            { value = l; return true; }
            return false;
        }

        // decimal / double
        if (underlying == typeof(decimal))
        {
            if (decimal.TryParse(valueStr, System.Globalization.NumberStyles.Number, culture, out var m))
            { value = m; return true; }
            return false;
        }
        if (underlying == typeof(double))
        {
            if (double.TryParse(valueStr, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, culture, out var d))
            { value = d; return true; }
            return false;
        }
        if (underlying == typeof(DateTime))
        {
            if (TryParseTimestamp(valueStr, out var ts)) { value = ts; return true; }
            return false;
        }
        try
        {
            value = Convert.ChangeType(valueStr, underlying, culture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? ExtractMappedValue(JsonElement root, object mappingValue)
    {
        switch (mappingValue)
        {
            case string single:
                return ReadByPath(root, single);

            case IEnumerable<string> many:
                foreach (var path in many)
                {
                    var v = ReadByPath(root, path);
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }

                return null;

            case IEnumerable<object> objs:
                foreach (var o in objs)
                {
                    if (o is string s)
                    {
                        var v = ReadByPath(root, s);
                        if (!string.IsNullOrWhiteSpace(v)) return v;
                    }
                }

                return null;

            default:
                return null;
        }
    }
    private static bool TryParseTimestamp(string? text, out DateTime tsUtc)
    {
        tsUtc = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        
        if (DateTime.TryParseExact(text, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var d))
        {
            tsUtc = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            return true;
        }
        if (DateTime.TryParse(text,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            tsUtc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return true;
        }

        return false;
    }
}
