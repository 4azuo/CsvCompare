using Cm.CmCWPC.Com.Enum;
using DataFileComparer.Entities;
using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
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

            //Tasks.Add(Task.Run(() =>
            //{
                using (var stream = File.Open(df.FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var ds = reader.AsDataSet();
                        foreach (DataTable sheet in ds.Tables)
                        {
                            if (!IsInterfaceSheet(sheet.GetValue("A1"))) continue;
                            var itf = new DataFileInterface();
                            var items = GetSheetItems(sheet, itf);
                            if (items.Count <= 0) continue;

                            itf.DataFile = df;
                            itf.SheetName = sheet.TableName;
                            itf.SystemName = sheet.GetValue("L2");
                            itf.InterfaceName = sheet.GetValue("AF2");
                            itf.InterfaceId = sheet.GetValue("J10");
                            itf.FileName = sheet.GetValue("AK10");
                            itf.Connection = sheet.GetValue("J11");
                            itf.Format = sheet.GetValue("AK11");
                            itf.Delimiter = sheet.GetValue("AK12");
                            itf.Newline = sheet.GetValue("AK13");
                            itf.Items = items;

                            rs.Add(itf);
                        }
                    }
                }
            //}));

            return rs;
        }

        public static List<DataFileInterfaceItem> GetSheetItems(DataTable sheet, DataFileInterface itf)
        {
            var lisItem = new List<DataFileInterfaceItem>();
            var sortIndex = 1;

            for (int r = 18; r < sheet.Rows.Count; r++)
            {
                var row = sheet.Rows[r];

                var itemName = row[4].Parse<string>();
                if (itemName.Contains("改行コード"))
                    break;

                var isKey = !string.IsNullOrEmpty(row[2].Parse<string>());
                var type = row[24].Parse<string>().Contains("文字型") == true ? typeof(string) : typeof(double);
                var length = row[29].Parse<int>();
                var byteLength = row[32].Parse<int>();
                var isRequired = !string.IsNullOrEmpty(row[35].Parse<string>());

                var i = new DataFileInterfaceItem();
                i.Pause();

                i.Interface = itf;
                i.ItemIndex = lisItem.Count;
                i.ItemName = itemName;
                i.IsKey = isKey;
                i.Type = type;
                i.Length = length;
                i.ByteLength = byteLength;
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

            using (var wb = new XSSFWorkbook())
            {
                var sheet = wb.CreateSheet("比較結果");
                sheet.SetZoom(40);

                //interface
                sheet.GetCell(0, 0).SetCellValue("インタフェース");
                sheet.GetCell(1, 0).SetCellValue(result.Interface.DataFile.FilePath);
                sheet.GetCell(2, 0).SetCellValue(result.Interface.InterfaceName);
                sheet.GetCell(3, 0).SetCellValue(result.Interface.InterfaceId);

                //data info
                sheet.GetCell(5, 0).SetCellValue("現世代");
                sheet.GetCell(6, 0).SetCellValue(result.OldFile.FilePath);
                sheet.GetCell(5, 0 + colCount + SEP_COL_COUNT).SetCellValue("次世代");
                sheet.GetCell(6, 0 + colCount + SEP_COL_COUNT).SetCellValue(result.NewFile.FilePath);

                //data
                const int FROM_ROW_INDEX = 8;
                const int FROM_COL_INDEX = 1;

                //data
                foreach (var row in result.OldFileContent.Rows)
                {
                    foreach (var col in result.Interface.Items)
                    {
                        var info = row.GetCell(col.ItemIndex);
                        var rowIndex = 1 + FROM_ROW_INDEX + row.RowIndex;
                        var colIndex = FROM_COL_INDEX + info.CellIndex;
                        var cell = sheet.GetCell(rowIndex, colIndex);
                        cell.SetCellValue(info.Value);
                        FormatCell(wb, cell, info);
                    }
                }
                //header
                foreach (var info in result.Interface.Items)
                {
                    var rowIndex = FROM_ROW_INDEX;
                    var colIndex = FROM_COL_INDEX + info.ItemIndex;
                    var cell = sheet.GetCell(rowIndex, colIndex);
                    cell.SetCellValue(info.ItemName);
                    FormatHeader(wb, cell, info);
                }

                //data
                foreach (var row in result.NewFileContent.Rows)
                {
                    foreach (var col in result.Interface.Items)
                    {
                        var info = row.GetCell(col.ItemIndex);
                        var rowIndex = 1 + FROM_ROW_INDEX + row.RowIndex;
                        var colIndex = FROM_COL_INDEX + info.CellIndex + colCount + SEP_COL_COUNT;
                        var cell = sheet.GetCell(rowIndex, colIndex);
                        cell.SetCellValue(info.Value);
                        FormatCell(wb, cell, info);
                    }
                }
                //header
                foreach (var info in result.Interface.Items)
                {
                    var rowIndex = FROM_ROW_INDEX;
                    var colIndex = FROM_COL_INDEX + info.ItemIndex + colCount + SEP_COL_COUNT;
                    var cell = sheet.GetCell(rowIndex, colIndex);
                    cell.SetCellValue(info.ItemName);
                    FormatHeader(wb, cell, info);
                }

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    wb.Write(fs);
                }
            }
        }

        private static bool IsInterfaceSheet(string a1Value)
        {
            return a1Value != null && a1Value.Contains("基本設計書");
        }

        private static void FormatHeader(XSSFWorkbook wb, ICell cell, DataFileInterfaceItem info)
        {
            cell.CellStyle = wb.CreateCellStyle();
            cell.CellStyle.BorderBottom = BorderStyle.Medium;
            cell.CellStyle.BorderTop = BorderStyle.Medium;
            cell.CellStyle.BorderLeft = BorderStyle.Medium;
            cell.CellStyle.BorderRight = BorderStyle.Medium;
            cell.CellStyle.FillPattern = FillPattern.SolidForeground;
            cell.CellStyle.FillForegroundColor = info.HSSFBackground;
        }

        private static void FormatCell(XSSFWorkbook wb, ICell cell, DataFileContentCell info)
        {
            cell.CellStyle = wb.CreateCellStyle();
            cell.CellStyle.BorderBottom = BorderStyle.Thin;
            cell.CellStyle.BorderTop = BorderStyle.Thin;
            cell.CellStyle.BorderLeft = BorderStyle.Thin;
            cell.CellStyle.BorderRight = BorderStyle.Thin;
            cell.CellStyle.FillPattern = FillPattern.SolidForeground;
            cell.CellStyle.FillForegroundColor = info.HSSFBackground;
        }
    }
}