using System.Windows.Media;

namespace DataFileComparer.Entities
{
    public class DataFileContentCell
    {
        public DataFileContentRow Row { get; set; }
        public int CellIndex
        {
            get
            {
                if (VirtualCellIndex >= 0)
                    return VirtualCellIndex;
                return Row.Cells.IndexOf(this);
            }
        }
        public int VirtualCellIndex { get; set; } = -1;
        public Brush Background
        {
            get
            {
                if (Row.Background == Brushes.White)
                {
                    if (Row.GetCellValue(CellIndex) != Row.SameRow.GetCellValue(CellIndex))
                        return Brushes.Red;
                }
                return Row.Background;
            }
        }
        public string Value { get; set; }
    }
}
