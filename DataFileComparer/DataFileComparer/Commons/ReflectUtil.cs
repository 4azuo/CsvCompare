using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;

public static class ReflectUtil
{
    private const char MAP_SEPARATE_CHAR = '=';

    /// <summary>
    /// コントロールのイベントを取得
    /// </summary>
    /// <param name="ctrl"></param>
    /// <param name="eventName"></param>
    /// <returns></returns>
    //public static Delegate[] GetInvocationList(this Control ctrl, string eventName)
    //{
    //    var propertyInfo = ctrl.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
    //    var eventHandlerList = propertyInfo.GetValue(ctrl, new object[] { }) as EventHandlerList;
    //    var fieldInfo = ctrl.GetType().GetField("Event" + eventName, BindingFlags.NonPublic | BindingFlags.Static);

    //    var eventKey = fieldInfo.GetValue(ctrl);
    //    var eventHandler = eventHandlerList[eventKey] as Delegate;
    //    var invocationList = eventHandler.GetInvocationList();
    //    return invocationList;
    //}

    /// <summary>
    /// 全プロジェクトのクラスを取得
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetEnumerableChildren(this Type parent, Func<Type, bool> predicate = null)
    {
        var rs = new List<Type>();
        foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
        {
            rs.AddRange(assem.GetTypes().Where(myType => (predicate == null || predicate.Invoke(myType)) && parent.IsAssignableFrom(myType)));
        }
        return rs;
    }

    /// <summary>
    /// プロパティ値ゲット（全角）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldNm"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    //public static object GetFieldWideValue(this object obj, string fieldNm, string code)
    //{
    //    return obj.GetType().GetProperty(
    //        $"{fieldNm}{Strings.StrConv(code, VbStrConv.Wide)}")
    //        .GetValue(obj, null);
    //}

    /// <summary>
    /// プロパティ値セット（全角）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldNm"></param>
    /// <param name="newValue"></param>
    /// <param name="code"></param>
    //public static void SetFieldWideValue(this object obj, string fieldNm, object newValue, string code)
    //{
    //    var prop = obj.GetType().GetProperty($"{fieldNm}{Strings.StrConv(code, VbStrConv.Wide)}");
    //    var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
    //    prop.SetValue(obj, ParseUtil.ParseUnknown(newValue, type), null);
    //}

    /// <summary>
    /// プロパティ値ゲット
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldNm"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static object GetFieldValue(this object obj, string fieldNm, string code)
    {
        return obj.GetType().GetProperty(
            $"{fieldNm}{code}")
            .GetValue(obj, null);
    }

    /// <summary>
    /// プロパティ値セット
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldNm"></param>
    /// <param name="newValue"></param>
    /// <param name="code"></param>
    public static void SetFieldValue(this object obj, string fieldNm, object newValue, string code)
    {
        var prop = obj.GetType().GetProperty($"{fieldNm}{code}");
        var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
        prop.SetValue(obj, ParseUtil.ParseUnknown(newValue, type), null);
    }

    /// <summary>
    /// マージ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="source"></param>
    public static T MergeObj<T>(this T target, T source)
    {
        var properties = target.GetType().GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(source, null);
            if (value != null)
                prop.SetValue(target, value, null);
        }

        return target;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    //public static T Clone<T>(this T instance)
    //{
    //    return ValueUtil.Copy<T>(instance);
    //}

    /// <summary>
    /// プロパティマップ
    /// </summary>
    public static T MapProperties<T>(this T dest, object source, params string[] maps)
    {
        if (source == null)
            return default(T); //null
        if (dest == null)
            dest = (T)Activator.CreateInstance<T>();
        foreach (var m in maps)
        {
            var t = m.Split(MAP_SEPARATE_CHAR);
            var toFnm = t[0].Trim();
            var fromFnm = (t.Length > 1 ? t[1] : t[0]).Trim();
            var to = dest.GetType().GetProperty(toFnm, BindingFlags.Public | BindingFlags.Instance);
            var from = source.GetType().GetProperty(fromFnm, BindingFlags.Public | BindingFlags.Instance);
            to.SetValue(dest, from.GetValue(source, null), null);
        }
        return dest;
    }

    public static T MapLeftProperties<T>(this T dest, object source, string prefix, params string[] maps)
    {
        return MapProperties(dest, source, maps.Select(x =>
        {
            var t = x.Split(MAP_SEPARATE_CHAR);
            var toFnm = t[0].Trim();
            var fromFnm = (t.Length > 1 ? t[1] : t[0]).Trim();
            return $"{toFnm}{MAP_SEPARATE_CHAR}{prefix}{fromFnm}";
        }).ToArray());
    }

    public static T MapRightProperties<T>(this T dest, object source, string prefix, params string[] maps)
    {
        return MapProperties(dest, source, maps.Select(x =>
        {
            var t = x.Split(MAP_SEPARATE_CHAR);
            var toFnm = t[0].Trim();
            var fromFnm = (t.Length > 1 ? t[1] : t[0]).Trim();
            return $"{prefix}{toFnm}{MAP_SEPARATE_CHAR}{fromFnm}";
        }).ToArray());
    }
}