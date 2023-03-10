using Cm.CmCWPC.Com.Enum;
using DataFileComparer.Commons;
using DataFileComparer.Consts;
using DataFileComparer.Entities;
using DataFileComparer.Processes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataFileComparer
{
    public partial class ConfirmWindow : CWindow<ConfirmWindowData>
    {
        public void Init(DataFile oldFile, DataFile newFile, DataFileInterface itf)
        {
            WindowData.OldFile = oldFile;
            WindowData.NewFile = newFile;
            WindowData.Interface = itf;
            FormatSameRow(null, null);
        }

        private void FormatNone(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            FillData(FileComparer.FormatNone(WindowData.OldFile, WindowData.NewFile, WindowData.Interface));
            WindowData.Mode = ExportModeEnum.FormatNone;
            IsEnabled = true;
        }

        private void FormatSort(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            FillData(FileComparer.FormatSort(WindowData.OldFile, WindowData.NewFile, WindowData.Interface));
            WindowData.Mode = ExportModeEnum.FormatSort;
            IsEnabled = true;
        }

        private void FormatSameRow(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            FillData(FileComparer.FormatSameRow(WindowData.OldFile, WindowData.NewFile, WindowData.Interface));
            WindowData.Mode = ExportModeEnum.FormatSame;
            IsEnabled = true;
        }

        public void FillData(FileCompareResult cpmResult)
        {
            dgOldData.Columns.Clear();
            dgNewData.Columns.Clear();

            var columns = new List<DataGridColumn>();
            foreach (var colObj in cpmResult.Interface.Items)
            {
                dgOldData.Columns.Add(CreateColumn(colObj));
                dgNewData.Columns.Add(CreateColumn(colObj));
            }

            dgOldData.ItemsSource = cpmResult.OldFileContent.Rows;
            dgNewData.ItemsSource = cpmResult.NewFileContent.Rows;

            WindowData.ExportData = cpmResult;
        }

        private DataGridColumn CreateColumn(DataFileInterfaceItem colObj)
        {
            var col = new DataGridTextColumn();

            col.Binding = new Binding($"Cells[{colObj.ItemIndex}].Value");
            col.Header = new DataGridColumnHeader
            {
                Content = colObj.ItemName,
                Background = colObj.Background,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
            };

            var cellStyle = new Style();
            cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new Binding($"Cells[{colObj.ItemIndex}].Background")));
            col.CellStyle = cellStyle;

            return col;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var dg = sender as DataGrid;
            var svOldGrid = WindowUtil.GetVisualChild<ScrollViewer>(dgOldData);
            var svNewGrid = WindowUtil.GetVisualChild<ScrollViewer>(dgNewData);
            if (dg == dgOldData)
            {
                svNewGrid.ScrollToVerticalOffset(svOldGrid.VerticalOffset);
                svNewGrid.ScrollToHorizontalOffset(svOldGrid.HorizontalOffset);
            }
            else
            {
                svOldGrid.ScrollToVerticalOffset(svNewGrid.VerticalOffset);
                svOldGrid.ScrollToHorizontalOffset(svNewGrid.HorizontalOffset);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var dg = sender as DataGrid;
                if (dg == dgOldData)
                {
                    dgNewData.SelectedItem = (dg.SelectedItem as DataFileContentRow)?.PairedRow ?? (dgNewData.ItemsSource as IList<DataFileContentRow>)[dgOldData.SelectedIndex];
                }
                else
                {
                    dgOldData.SelectedItem = (dg.SelectedItem as DataFileContentRow)?.PairedRow ?? (dgOldData.ItemsSource as IList<DataFileContentRow>)[dgNewData.SelectedIndex];
                }
            }
            catch
            {

            }
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = $"{WindowData.OldFile.FileName}_{WindowData.NewFile.FileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{FileExtEnum.XLSX.Ext}";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == true)
            {
                if (!dialog.FileName.EndsWith(FileExtEnum.XLSX.Ext))
                    dialog.FileName += FileExtEnum.XLSX.Ext;
                FileProcessUtil.WriteDataFile(dialog.FileName, WindowData.ExportData);
                MessageBox.Show(Messages.INF001);
            }
        }
    }
}
