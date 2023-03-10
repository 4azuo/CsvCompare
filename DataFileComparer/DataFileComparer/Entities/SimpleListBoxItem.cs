using DataFileComparer.Commons;

namespace DataFileComparer.Entities
{
    public class SimpleListBoxItem : AutoNotifiableObject
    {
        public string Display { get; set; }
        public string Value { get; set; }
        public object Tag { get; set; }
    }
}
