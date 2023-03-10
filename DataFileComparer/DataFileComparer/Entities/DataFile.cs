using DataFileComparer.Commons;
using System.Collections.Generic;

namespace DataFileComparer.Entities
{
    public class DataFile : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public List<DataFileInterface> Interfaces { get; set; }
    }
}
