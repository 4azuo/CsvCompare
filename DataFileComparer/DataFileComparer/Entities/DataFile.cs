using DataFileComparer.Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFileComparer.Entities
{
    public class DataFile : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public List<DataFileInterface> Interfaces { get; set; }
    }
}
