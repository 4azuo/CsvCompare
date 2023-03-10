using System.Linq;

namespace Cm.CmCWPC.Com.Enum
{
    public class FileExtEnum : EnumableObject
    {
        public static FileExtEnum CSV { get; } = new FileExtEnum(".csv", "*.csv");
        public static FileExtEnum XLSX { get; } = new FileExtEnum(".xlsx", "*.xlsx");

        #region Constructor
        public string Ext { get; set; }
        private FileExtEnum(string ext, string value) : base(value)
        {
            Ext = ext;
        }
        #endregion

    }
}