using System;

namespace DataFileComparer.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoredPropertyAttribute : Attribute
    {
    }
}