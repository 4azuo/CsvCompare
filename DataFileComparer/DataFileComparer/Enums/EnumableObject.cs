using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cm.CmCWPC.Com.Enum
{
    public abstract class EnumableObject
    {
        #region Collector
        public static List<EnumableObject> AllEnums { get; } = new List<EnumableObject>();
        #endregion

        public string Value { get; }

        public EnumableObject(string value)
        {
            Value = value;
            AllEnums.Add(this);
        }

        public static T[] GetAllEnums<T>() where T : EnumableObject
        {
            var properties = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return properties.Select((x) => (T)x.GetValue(null, null)).ToArray();
        }

        public static T GetEnumByVal<T>(string val) where T : EnumableObject
        {
            return GetAllEnums<T>().FirstOrDefault(x => x.Value == val);
        }

        public static T GetEnumByProp<T>(string propName, string propValue) where T : EnumableObject
        {
            var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return GetAllEnums<T>().FirstOrDefault(x => prop.GetValue(x, null).ToString() == propValue);
        }

        public static T GetEnum<T>(string enumName) where T : EnumableObject
        {
            var property = typeof(T).GetProperty(enumName, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return (T)property.GetValue(null, null);
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}