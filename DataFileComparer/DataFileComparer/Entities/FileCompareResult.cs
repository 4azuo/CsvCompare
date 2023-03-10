using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFileComparer.Entities
{
    public class FileCompareResult
    {
        public DataFile OldFile { get; set; }
        public DataFileContent OldFileContent { get; set; }
        public DataFile NewFile { get; set; }
        public DataFileContent NewFileContent { get; set; }
        public DataFileInterface Interface { get; set; }
    }
}
