using Google.Protobuf.WellKnownTypes;

namespace MarketGateway.Contracts.Utils;


public static class ProtoJson
{
    public static Dictionary<string, object?> ToDictionary(Struct? s)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (s is null) return dict;
        foreach (var kv in s.Fields)
            dict[kv.Key] = FromValue(kv.Value);
        return dict;
    }

    public static object? FromValue(Value v) => v.KindCase switch
    {
        Value.KindOneofCase.NullValue  => null,
        Value.KindOneofCase.BoolValue  => v.BoolValue,
        Value.KindOneofCase.NumberValue=> v.NumberValue, // double
        Value.KindOneofCase.StringValue=> v.StringValue,
        Value.KindOneofCase.StructValue=> ToDictionary(v.StructValue),
        Value.KindOneofCase.ListValue  => v.ListValue.Values.Select(FromValue).ToList(),
        _ => null
    };

    public static Struct ToStruct(IReadOnlyDictionary<string, object?> dict)
    {
        var s = new Struct();
        foreach (var (k, o) in dict)
            s.Fields[k] = ToValue(o);
        return s;
    }

    public static Value ToValue(object? o) => o switch
    {
        null               => Value.ForNull(),
        bool b             => Value.ForBool(b),
        string s           => Value.ForString(s),
        byte or sbyte or short or ushort or int or uint or long or ulong or float or double
                           => Value.ForNumber(Convert.ToDouble(o)),
        IDictionary<string, object?> m
                           => Value.ForStruct(ToStruct(new Dictionary<string, object?>(m.ToDictionary(x=>x.Key, x=>x.Value)))),
        IEnumerable<object?> list
                           => new Value { ListValue = new ListValue { Values = { list.Select(ToValue) } } },
        DateTime dt        => Value.ForString(dt.ToUniversalTime().ToString("O")),
        DateTimeOffset dto => Value.ForString(dto.ToUniversalTime().ToString("O")),
        _                  => Value.ForString(o.ToString() ?? string.Empty)
    };
}
