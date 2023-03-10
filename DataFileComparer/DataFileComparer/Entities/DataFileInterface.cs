using DataFileComparer.Commons;
using System.Collections.Generic;

namespace DataFileComparer.Entities
{
    public class DataFileInterface : AutoNotifiableObject
    {
        public DataFile DataFile { get; set; }
        public string SheetName { get; set; }
        public string SystemName { get; set; }
        public string InterfaceName { get; set; }
        public string InterfaceId { get; set; }
        public string FileName { get; set; }
        public string Connection { get; set; }
        public string Format { get; set; }
        public string Delimiter { get; set; }
        public string DelimiterChar
        {
            get
            {
                switch (Delimiter?.ToLower())
                {
                    case "tab":
                        return @"\t";
                    default:
                        return ",";
                }
            }
        }
        public string Newline { get; set; }
        public string NewlineChar
        {
            get
            {
                switch (Newline?.ToLower())
                {
                    default:
                        return @"\n";
                }
            }
        }
        public List<DataFileInterfaceItem> Items { get; set; }
    }
}
