using DataFileComparer.Commons;
using DataFileComparer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataFileComparer.Processes
{
    public static class FileComparer
    {
        public static FileCompareResult FormatNone(DataFile oldFile, DataFile newFile, DataFileInterface itf)
        {
            var rs = new FileCompareResult
            {
                OldFile = oldFile,
                NewFile = newFile,
                Interface = itf,
            };

            //keys
            var keys = itf.Items.Where(x => x.IsKey).OrderBy(x => x.SortIndex).ToList();

            //get data
            var oldFileRows = FileProcessUtil.ReadDataFile(oldFile.FilePath, itf.DelimiterChar).Rows;
            var newFileRows = FileProcessUtil.ReadDataFile(newFile.FilePath, itf.DelimiterChar).Rows;
            rs.OldFileContent = oldFileRows.FirstOrDefault()?.Content;
            rs.OldFileContent.Rows = oldFileRows;
            rs.NewFileContent = newFileRows.FirstOrDefault()?.Content;
            rs.NewFileContent.Rows = newFileRows;

            //add blankrow
            AddBlankRows(oldFileRows, newFileRows, rs);

            //mock
            MockSameRow(oldFileRows, newFileRows);

            //trim
            TrimBlankRow(oldFileRows, newFileRows);
            return rs;
        }

        public static FileCompareResult FormatSort(DataFile oldFile, DataFile newFile, DataFileInterface itf)
        {
            var rs = new FileCompareResult
            {
                OldFile = oldFile,
                NewFile = newFile,
                Interface = itf,
            };

            //keys
            var keys = itf.Items.Where(x => x.IsKey).OrderBy(x => x.SortIndex).ToList();

            //get data
            var oldFileRowsOrdered = FileProcessUtil.ReadDataFile(oldFile.FilePath, itf.DelimiterChar).Rows.OrderBy(x => x.GetCellValue(0));
            var newFileRowsOrdered = FileProcessUtil.ReadDataFile(newFile.FilePath, itf.DelimiterChar).Rows.OrderBy(x => x.GetCellValue(0));

            //sort
            foreach (var col in keys)
            {
                oldFileRowsOrdered.ThenByDescending(x => x.GetCellValue(col.ItemIndex));
                newFileRowsOrdered.ThenByDescending(x => x.GetCellValue(col.ItemIndex));
            }
            var oldFileRows = oldFileRowsOrdered.ToList();
            var newFileRows = newFileRowsOrdered.ToList();
            rs.OldFileContent = oldFileRows.FirstOrDefault()?.Content;
            rs.OldFileContent.Rows = oldFileRows;
            rs.NewFileContent = newFileRows.FirstOrDefault()?.Content;
            rs.NewFileContent.Rows = newFileRows;

            //add blankrow
            AddBlankRows(oldFileRows, newFileRows, rs);

            //mock
            MockSameRow(oldFileRows, newFileRows);

            //pair
            PairRow(oldFileRows, newFileRows, keys);

            //trim
            TrimBlankRow(oldFileRows, newFileRows);
            return rs;
        }

        public static FileCompareResult FormatSameRow(DataFile oldFile, DataFile newFile, DataFileInterface itf)
        {
            var data = FormatSort(oldFile, newFile, itf);
            
            for (int r = 0; r < data.OldFileContent.Rows.Count; r++)
            {
                var oldRow = data.OldFileContent.Rows[r];
                if (oldRow.PairedRow == null) continue;

                if (oldRow.RowIndex > oldRow.PairedRow.RowIndex)
                {
                    var orgIndex = oldRow.PairedRow.RowIndex;
                    while (oldRow.RowIndex != oldRow.PairedRow.RowIndex)
                    {
                        data.NewFileContent.Rows.Insert(orgIndex, new DataFileContentRow
                        {
                            Content = data.NewFileContent,
                            IsBlankRow = true,
                        });
                    }
                }
                else if (oldRow.RowIndex < oldRow.PairedRow.RowIndex)
                {
                    var orgIndex = oldRow.RowIndex;
                    while (oldRow.RowIndex != oldRow.PairedRow.RowIndex)
                    {
                        data.OldFileContent.Rows.Insert(orgIndex, new DataFileContentRow
                        {
                            Content = data.OldFileContent,
                            IsBlankRow = true,
                        });
                    }
                }
            }

            //mock
            MockSameRow(data.OldFileContent.Rows, data.NewFileContent.Rows);

            //pair
            var keys = itf.Items.Where(x => x.IsKey).OrderBy(x => x.SortIndex).ToList();
            PairRow(data.OldFileContent.Rows, data.NewFileContent.Rows, keys);

            //trim
            TrimBlankRow(data.OldFileContent.Rows, data.NewFileContent.Rows);
            return data;
        }

        private static void AddBlankRows(List<DataFileContentRow> oldFileRows, List<DataFileContentRow> newFileRows, FileCompareResult rs)
        {
            var min = Math.Min(oldFileRows.Count, newFileRows.Count);
            var max = Math.Max(oldFileRows.Count, newFileRows.Count);
            for (int r = min; r < max; r++)
            {
                if (r < oldFileRows.Count)
                {
                    newFileRows.Add(new DataFileContentRow
                    {
                        Content = rs.NewFileContent,
                        IsBlankRow = true,
                    });
                }
                else if (r < newFileRows.Count)
                {
                    oldFileRows.Add(new DataFileContentRow
                    {
                        Content = rs.OldFileContent,
                        IsBlankRow = true,
                    });
                }
            }
        }

        private static void MockSameRow(List<DataFileContentRow> oldFileRows, List<DataFileContentRow> newFileRows)
        {
            var min = Math.Min(oldFileRows.Count, newFileRows.Count);
            for (int r = 0; r < min; r++)
            {
                var oldRow = oldFileRows[r];
                var newRow = newFileRows[r];
                oldRow.SameRow = newRow;
                newRow.SameRow = oldRow;
            }
        }

        private static void PairRow(List<DataFileContentRow> oldFileRows, List<DataFileContentRow> newFileRows, List<DataFileInterfaceItem> keys)
        {
            foreach (var oldRow in oldFileRows)
            {
                if (oldRow.IsBlankRow) continue;

                var newRow = newFileRows.FirstOrDefault(x => keys.All(k => oldRow.GetCellValue(k.ItemIndex) == x.GetCellValue(k.ItemIndex)));
                if (newRow != null && !newRow.IsBlankRow)
                {
                    oldRow.PairedRow = newRow;
                    newRow.PairedRow = oldRow;
                }
            }
        }

        private static void TrimBlankRow(List<DataFileContentRow> oldFileRows, List<DataFileContentRow> newFileRows)
        {
            var min = Math.Min(oldFileRows.Count, newFileRows.Count);
            var max = Math.Max(oldFileRows.Count, newFileRows.Count);
            for (int r = max - 1; r >= min; r--)
            {
                if (r < oldFileRows.Count && oldFileRows[r].IsBlankRow)
                {
                    oldFileRows.RemoveAt(r);
                }
                else if (r < newFileRows.Count && newFileRows[r].IsBlankRow)
                {
                    newFileRows.RemoveAt(r);
                }
            }
        }
    }
}
