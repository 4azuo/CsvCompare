using DataFileComparer.Commons;
using DataFileComparer.Consts;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            WindowData.OnLoad();
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

        public override MainWindowData InitData(CancelEventArgs initDataFlg)
        {
            if (File.Exists(INIT_DATA_FILEPATH))
            {
                initDataFlg.Cancel = true;

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
