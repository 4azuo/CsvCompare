using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;

public static class ParseUtil
{
    #region Const
    public static readonly MethodInfo PARSER = typeof(ParseUtil).GetMethod("Parse", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    #endregion

    /// <summary>
    /// Oに変換する
    /// ※別な変数を作成する
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static O ParseJson<O>(this object value)
    {
        return JsonConvert.DeserializeObject<O>(JsonConvert.SerializeObject(value));
    }

    /// <summary>
    /// toTypeに変換する
    /// </summary>
    /// <param name="value"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static object ParseUnknown(this object value, Type toType)
    {
        return PARSER.MakeGenericMethod(toType).Invoke(null, new object[] { value, null });
    }

    /// <summary>
    /// Oに変換する
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <param name="value"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public static O Parse<O>(this object value, O def = default(O))
    {
        var t = typeof(O);
        var converter = TypeDescriptor.GetConverter(t);
        try
        {
            if (converter.CanConvertTo(t))
                return (O)converter.ConvertTo(value, t);
            if (converter.CanConvertFrom(t))
                return (O)converter.ConvertFrom(value);
            return (O)Convert.ChangeType(value, t);
        }
        catch
        {
            return def;
        }
    }
}