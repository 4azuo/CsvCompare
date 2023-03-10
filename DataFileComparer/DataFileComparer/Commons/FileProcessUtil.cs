using Cm.CmCWPC.Com.Enum;
using DataFileComparer.Entities;
using IronXL;
using IronXL.Styles;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataFileComparer.Commons
{
    public static class FileProcessUtil
    {
        public static List<Task> Tasks { get; set; } = new List<Task>();

        public static List<DataFile> GetFiles(string folderPath, string searchPattern)
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                return Directory.GetFiles(folderPath, searchPattern).Select(x => new DataFile
                {
                    FileName = Path.GetFileName(x),
                    FilePath = x,
                }).ToList();
            }

            return new List<DataFile>();
        }

        public static List<DataFile> GetInterfaceFiles(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                return Directory.GetFiles(folderPath, FileExtEnum.XLSX.Value).Select(path =>
                {
                    return new DataFile
                    {
                        FileName = Path.GetFileName(path),
                        FilePath = path,
                        //Interfaces = GetSheets(path),
                    };
                }).ToList();
            }
            
            return new List<DataFile>();
        }
        
        public static List<DataFileInterface> GetSheets(DataFile df)
        {
            var rs = new List<DataFileInterface>();

            Tasks.Add(Task.Run(() =>
            {
                var wb = WorkBook.Load(df.FilePath);
                foreach (var sheet in wb.WorkSheets)
                {
                    if (!IsInterfaceSheet(sheet["A1"].StringValue)) continue;
                    var itf = new DataFileInterface();
                    var items = GetSheetItems(sheet, itf);
                    if (items.Count <= 0) continue;

                    itf.DataFile = df;
                    itf.SheetName = sheet.Name.Trim();
                    itf.SystemName = sheet["L2"].StringValue.Trim();
                    itf.InterfaceName = sheet["AF2"].StringValue.Trim();
                    itf.InterfaceId = sheet["J10"].StringValue.Trim();
                    //itf.FileName = GetCellValue(cells.FirstOrDefault(x => x.CellReference == "AK10"), stringTablePart);
                    //itf.Connection = GetCellValue(cells.FirstOrDefault(x => x.CellReference == "J11"), stringTablePart);
                    //itf.Format = GetCellValue(cells.FirstOrDefault(x => x.CellReference == "AK11"), stringTablePart);
                    itf.Delimiter = sheet["AK12"].StringValue.Trim();
                    itf.Newline = sheet["AK13"].StringValue.Trim();
                    itf.Items = items;

                    rs.Add(itf);
                }
                wb.Close();
            }));

            return rs;
        }

        public static List<DataFileInterfaceItem> GetSheetItems(WorkSheet sheet, DataFileInterface itf)
        {
            var lisItem = new List<DataFileInterfaceItem>();
            var sortIndex = 1;
            
            foreach (var row in sheet.Rows)
            {
                if (row.RangeAddress.FirstRow < 18)
                    continue;
                
                var itemName = row.AllColumnsInRange[4].StringValue;
                if (itemName.Contains("改行コード"))
                    break;
                
                var isKey = !string.IsNullOrEmpty(row.AllColumnsInRange[2].StringValue);
                var type = row.AllColumnsInRange[24].StringValue.Contains("文字型") == true ? typeof(string) : typeof(double);
                //var length = ParseUtil.Parse<int>(GetCellValue(cells[29], stringTablePart));
                //var byteLength = ParseUtil.Parse<int>(GetCellValue(cells[32], stringTablePart));
                var isRequired = !string.IsNullOrEmpty(row.AllColumnsInRange[35].StringValue);

                var i = new DataFileInterfaceItem();
                i.Pause();

                i.Interface = itf;
                i.ItemIndex = lisItem.Count;
                i.ItemName = itemName;
                i.IsKey = isKey;
                i.Type = type;
                //i.Length = length;
                //i.ByteLength = byteLength;
                i.IsRequired = isRequired;
                i.SortIndex = isKey ? sortIndex++ : 0;

                i.RefreshAll();
                i.Unpause();
                lisItem.Add(i);
            }

            return lisItem;
        }

        public static DataFileContent ReadDataFile(string filePath, string delimiter)
        {
            var rs = new DataFileContent();
            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(delimiter);

                rs.Rows = new List<DataFileContentRow>();
                while (!parser.EndOfData)
                {
                    var row = new DataFileContentRow();

                    row.Content = rs;
                    row.Cells = parser.ReadFields().Select((x, i) => new DataFileContentCell
                    {
                        Row = row,
                        Value = x,
                    }).ToList();

                    rs.Rows.Add(row);
                }
            }
            return rs;
        }

        public static void WriteDataFile(string filePath, FileCompareResult result)
        {
            var colCount = result.Interface.Items.Count;
            const int SEP_COL_COUNT = 3;

            var wb = WorkBook.Create(ExcelFileFormat.XLSX);
            var sheet = wb.CreateWorkSheet("比較結果");
            sheet.SetZoom(40);

            //interface
            sheet.SetCellValue(0, 0, "インタフェース");
            sheet.SetCellValue(1, 0, result.Interface.DataFile.FilePath);
            sheet.SetCellValue(2, 0, result.Interface.InterfaceName);
            sheet.SetCellValue(3, 0, result.Interface.InterfaceId);

            //data info
            sheet.SetCellValue(5, 0, "現世代");
            sheet.SetCellValue(6, 0, result.OldFile.FilePath);
            sheet.SetCellValue(5, 0 + colCount + SEP_COL_COUNT, "次世代");
            sheet.SetCellValue(6, 0 + colCount + SEP_COL_COUNT, result.NewFile.FilePath);
            
            //data
            const int FROM_ROW_INDEX = 8;
            const int FROM_COL_INDEX = 1;

            //header
            foreach (var col in result.Interface.Items)
            {
                var rowIndex = FROM_ROW_INDEX;
                var colIndex = FROM_COL_INDEX + col.ItemIndex;
                sheet.SetCellValue(rowIndex, colIndex, col.ItemName);

                var sheetCell = sheet.GetCellAt(rowIndex, colIndex);
                SetBorder(sheetCell);
                SetBackgroundColor(sheetCell, col.Background);
            }
            //data
            foreach (var row in result.OldFileContent.Rows)
            {
                foreach (var col in result.Interface.Items)
                {
                    var cell = row.GetCell(col.ItemIndex);
                    var rowIndex = 1 + FROM_ROW_INDEX + row.RowIndex;
                    var colIndex = FROM_COL_INDEX + cell.CellIndex;
                    sheet.SetCellValue(rowIndex, colIndex, row.GetCellValue(cell.CellIndex));

                    var sheetCell = sheet.GetCellAt(rowIndex, colIndex);
                    SetBorder(sheetCell);
                    SetBackgroundColor(sheetCell, cell.Background);
                }
            }

            //header
            foreach (var col in result.Interface.Items)
            {
                var rowIndex = FROM_ROW_INDEX;
                var colIndex = FROM_COL_INDEX + col.ItemIndex + colCount + SEP_COL_COUNT;
                sheet.SetCellValue(rowIndex, colIndex, col.ItemName);

                var sheetCell = sheet.GetCellAt(rowIndex, colIndex);
                SetBorder(sheetCell);
                SetBackgroundColor(sheetCell, col.Background);
            }
            //data
            foreach (var row in result.NewFileContent.Rows)
            {
                foreach (var col in result.Interface.Items)
                {
                    var cell = row.GetCell(col.ItemIndex);
                    var rowIndex = 1 + FROM_ROW_INDEX + row.RowIndex;
                    var colIndex = FROM_COL_INDEX + cell.CellIndex + colCount + SEP_COL_COUNT;
                    sheet.SetCellValue(rowIndex, colIndex, row.GetCellValue(cell.CellIndex));

                    var sheetCell = sheet.GetCellAt(rowIndex, colIndex);
                    SetBorder(sheetCell);
                    SetBackgroundColor(sheetCell, cell.Background);
                }
            }

            wb.SaveAs(filePath);
            wb.Close();
        }

        private static bool IsInterfaceSheet(string a1Value)
        {
            return a1Value != null && a1Value.Contains("基本設計書");
        }

        private static string GetColorCode(Brush brush)
        {
            return "#" + brush.ToString().Substring(3);
        }

        private static void SetBorder(Cell cell)
        {
            cell.Style.BottomBorder.Type = BorderType.Medium;
            cell.Style.TopBorder.Type = BorderType.Medium;
            cell.Style.LeftBorder.Type = BorderType.Medium;
            cell.Style.RightBorder.Type = BorderType.Medium;
        }

        private static void SetBackgroundColor(Cell cell, Brush brush)
        {
            cell.Style.BackgroundColor = GetColorCode(brush);
        }
    }
}