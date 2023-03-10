using Cm.CmCWPC.Com.Enum;
using DataFileComparer.Attributes;
using DataFileComparer.Commons;
using DataFileComparer.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DataFileComparer
{
    //\\bsys-fs\16_リテール＆ペイメント事業部\03_リテールソリューション部\10_プロジェクト\20_MCCM\10_次世代顧客基盤\10_開発\01_顧客ポイント管理\70_試験支援\50_バリエーションテスト\04_比較結果\比較方法検討\
    public class MainWindowData : CWindowData
    {
        #region Properties
        /// <summary>
        /// ファイル
        /// </summary>
        [NotifyMethod("LoadOldFiles")]
        public string OldFolderUrl { get; set; }// = @"c:/web/old";
        [NotifyMethod("LoadNewFiles")]
        public string NewFolderUrl { get; set; }// = @"c:/web/new";
        public List<DataFile> OldFiles { get; set; }
        public List<DataFile> NewFiles { get; set; }
        public DataFile OldFileSelected { get; set; }
        public DataFile NewFileSelected { get; set; }

        /// <summary>
        /// インタフェース
        /// </summary>
        [NotifyMethod("LoadInterfaceFiles", "LoadInterfaceSheets", "LoadInterfaceItems")]
        public string InterfaceFolderUrl { get; set; }// = @"c:/web/test";
        public List<DataFile> InterfaceFiles { get; set; }
        public List<DataFileInterface> Interfaces { get; set; }
        public List<DataFileInterfaceItem> InterfaceItems { get; set; }
        [NotifyMethod("LoadInterfaceSheets", "LoadInterfaceItems")]
        public DataFile InterfaceFileSelected { get; set; }
        [NotifyMethod("LoadInterfaceItems")]
        public DataFileInterface InterfaceSelected { get; set; }
        public DataFileInterfaceItem InterfaceItemSelected { get; set; }

        /// <summary>
        /// Screen Control
        /// </summary>
        [NotifyMethod("ReloadInterface")]
        public Visibility IsLoading
        {
            get
            {
                FileProcessUtil.Tasks = FileProcessUtil.Tasks.Where(x => x.Status == TaskStatus.Running).ToList();
                if (FileProcessUtil.Tasks.Count == 0)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }
        #endregion

        public override void OnLoad()
        {
            LoadOldFiles(null);
            LoadNewFiles(null);
            LoadInterfaceFiles(null);
            LoadInterfaceSheets(null);
            LoadInterfaceItems(null);
        }

        public void LoadOldFiles(string propName)
        {
            OldFiles = new List<DataFile>(FileProcessUtil.GetFiles(OldFolderUrl, FileExtEnum.CSV.Value));
        }

        public void LoadNewFiles(string propName)
        {
            NewFiles = new List<DataFile>(FileProcessUtil.GetFiles(NewFolderUrl, FileExtEnum.CSV.Value));
        }

        public void LoadInterfaceFiles(string propName)
        {
            InterfaceFiles = new List<DataFile>(FileProcessUtil.GetInterfaceFiles(InterfaceFolderUrl));
        }

        public void LoadInterfaceSheets(string propName)
        {
            Interfaces = new List<DataFileInterface>();
            if (InterfaceFileSelected != null)
            {
                if (InterfaceFileSelected.Interfaces == null)
                    InterfaceFileSelected.Interfaces = FileProcessUtil.GetSheets(InterfaceFileSelected);
                Interfaces = InterfaceFileSelected.Interfaces;
            }
        }

        public void LoadInterfaceItems(string propName)
        {
            InterfaceItems = new List<DataFileInterfaceItem>();
            if (InterfaceSelected != null)
            {
                InterfaceItems = InterfaceSelected.Items;
            }
        }

        public void ReloadInterface(string propName)
        {
            if (IsLoading == Visibility.Collapsed)
            {
                Interfaces = null;
                InterfaceItems = null;
                Notify("Interfaces");
                Notify("InterfaceItems");

                LoadInterfaceSheets(null);
                LoadInterfaceItems(null);
                Notify("Interfaces");
                Notify("InterfaceItems");
            }
        }
    }
}
