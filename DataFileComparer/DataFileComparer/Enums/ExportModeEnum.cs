namespace Cm.CmCWPC.Com.Enum
{
    public class ExportModeEnum : EnumableObject
    {
        public static ExportModeEnum FormatNone { get; } = new ExportModeEnum("フォーマット ‐ なし");
        public static ExportModeEnum FormatSort { get; } = new ExportModeEnum("フォーマット ‐ ソート");
        public static ExportModeEnum FormatSame { get; } = new ExportModeEnum("フォーマット ‐ 同行");

        #region Constructor
        private ExportModeEnum(string value) : base(value)
        {
        }
        #endregion

    }
}