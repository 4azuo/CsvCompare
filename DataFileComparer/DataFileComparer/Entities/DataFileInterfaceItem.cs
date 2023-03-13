using DataFileComparer.Attributes;
using DataFileComparer.Commons;
using NPOI.HSSF.Util;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DataFileComparer.Entities
{
    public class DataFileInterfaceItem : AutoNotifiableObject
    {
        public DataFileInterface Interface { get; set; }
        public int ItemIndex { get; set; }
        public string ItemName { get; set; }
        [NotifyMethod("ResetSortIndex")]
        public bool IsKey { get; set; }
        public Type Type { get; set; } //typeof(string)/typeof(double)
        public int Length { get; set; }
        public int ByteLength { get; set; }
        public bool IsRequired { get; set; }
        public Visibility DisplaySortIndex
        {
            get
            {
                return IsKey ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public int SortIndex { get; set; }
        public Brush Background
        {
            get
            {
                return IsKey ? Brushes.Aqua : Brushes.LightGray;
            }
        }
        public short HSSFBackground
        {
            get
            {
                return IsKey ? HSSFColor.Aqua.Index : HSSFColor.Grey25Percent.Index;
            }
        }

        public void ResetSortIndex(string propName)
        {
            SortIndex = IsKey ? (Interface.Items?.Max(x => x.SortIndex) ?? 0) + 1 : 0;
        }
    }
}
