using Cm.CmCWPC.Com.Enum;
using DataFileComparer.Entities;

namespace DataFileComparer
{
    public class ConfirmWindowData : CWindowData
    {
        public ExportModeEnum Mode { get; set; }
        public DataFile OldFile { get; set; }
        public DataFile NewFile { get; set; }
        public DataFileInterface Interface { get; set; }
        public FileCompareResult ExportData { get; set; }
        public DataFileContentRow SelectedOldRow { get; set; }
        public DataFileContentRow SelectedNewRow { get; set; }
    }
}
