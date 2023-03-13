using NPOI.HSSF.Util;
using System.Collections.Generic;
using System.Windows.Media;

namespace DataFileComparer.Entities
{
    public class DataFileContentRow
    {
        public DataFileContent Content { get; set; }
        public int RowIndex
        {
            get
            {
                return Content.Rows.IndexOf(this);
            }
        }
        public bool IsBlankRow { get; set; } = false;
        public Brush Background
        {
            get
            {
                if (IsBlankRow)
                    return Brushes.DarkGray;
                if (SameRow == null || SameRow.IsBlankRow)
                    return Brushes.LightGreen;
                return Brushes.White;
            }
        }
        public short HSSFBackground
        {
            get
            {
                if (IsBlankRow)
                    return HSSFColor.Grey80Percent.Index;
                if (SameRow == null || SameRow.IsBlankRow)
                    return HSSFColor.LightGreen.Index;
                return HSSFColor.White.Index;
            }
        }
        public List<DataFileContentCell> Cells { get; set; } = new List<DataFileContentCell>();
        public DataFileContentRow PairedRow { get; set; }
        public DataFileContentRow SameRow { get; set; }

        public string GetCellValue(int index)
        {
            if (index < 0 || index >= Cells.Count)
                return "";
            return Cells[index].Value;
        }

        public DataFileContentCell GetCell(int index)
        {
            if (index < 0 || index >= Cells.Count)
                return new DataFileContentCell
                {
                    Row = this,
                    VirtualCellIndex = index,
                };
            return Cells[index];
        }
    }
}
