using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace DataFileComparer.Commons
{
    public static class ExcelUtil
    {
        private const string CELL_ID_REGEX = "([a-zA-Z]+)([0-9]+)";

        public static int[] ConvertCellReferenceToIndex(string cellref)
        {
            var m = Regex.Match(cellref, CELL_ID_REGEX);
            if (m.Success)
            {
                var colId = m.Groups[1].Value.ToUpper();
                var colIndex = colId.ToCharArray().Sum(x => x - 65);
                var rowIndex = m.Groups[2].Value.Parse<int>() - 1;
                return new int[] { colIndex, rowIndex };
            }
            return null;
        }

        #region ExcelDataReader
        public static string GetValue(this DataTable table, int rowIndex, int colIndex)
        {
            return table.Rows[rowIndex][colIndex].Parse<string>()?.Trim() ?? "";
        }

        public static string GetValue(this DataTable table, string cellref)
        {
            var indexes = ConvertCellReferenceToIndex(cellref);
            return indexes == null ? "" : table.Rows[indexes[1]][indexes[0]].Parse<string>()?.Trim() ?? "";
        }
        #endregion

        #region NPOI
        public static ICell GetCell(this ISheet sheet, int rowIndex, int colIndex)
        {
            var row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            return row.GetCell(colIndex) ?? row.CreateCell(colIndex);
        }

        public static ICell GetCell(this ISheet sheet, string cellref)
        {
            var cellrefObj = new CellReference(cellref);
            var row = sheet.GetRow(cellrefObj.Row) ?? sheet.CreateRow(cellrefObj.Row);
            return row.GetCell(cellrefObj.Col) ?? row.CreateCell(cellrefObj.Col);
        }
        #endregion
    }
}
