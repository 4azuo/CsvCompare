using DataFileComparer.Commons;
using DataFileComparer.Consts;
using DataFileComparer.Entities;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataFileComparer
{
    public partial class MainWindow : CWindow<MainWindowData>
    {
        public const string INIT_DATA_FILEPATH = "./initdata.json";

        private void DoubleClickOpenFile(object sender, MouseButtonEventArgs e)
        {
            var s = sender as ListView;
            Process.Start(s.SelectedValue as string);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            if (IsProcessValid())
            {
                var cfmWindow = new ConfirmWindow();
                cfmWindow.Init(WindowData.OldFileSelected, WindowData.NewFileSelected, WindowData.InterfaceSelected);
                cfmWindow.ShowDialog();
            }
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            if (IsProcessValid())
            {
                WindowData.OnLoad();
            }
        }

        private bool IsProcessValid()
        {
            if (WindowData.OldFileSelected == null)
            {
                MessageBox.Show(Messages.WRN001);
                return false;
            }
            if (WindowData.NewFileSelected == null)
            {
                MessageBox.Show(Messages.WRN002);
                return false;
            }
            if (WindowData.InterfaceSelected == null)
            {
                MessageBox.Show(Messages.WRN003);
                return false;
            }
            return true;
        }

        public override MainWindowData InitData()
        {
            if (File.Exists(INIT_DATA_FILEPATH))
            {
                InitLoadData = false;
                AutoNotifiableObject.Begin();
                var data = JsonConvert.DeserializeObject<MainWindowData>(File.ReadAllText(INIT_DATA_FILEPATH), new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                });
                AutoNotifiableObject.End();
                return data;
            }
            return new MainWindowData();
        }

        public override void ClosingWindow(object s, CancelEventArgs e)
        {
            File.WriteAllText(INIT_DATA_FILEPATH, JsonConvert.SerializeObject(WindowData, Formatting.None, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            }));
        }
    }
}
