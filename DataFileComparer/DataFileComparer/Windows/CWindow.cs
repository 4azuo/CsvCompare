using DataFileComparer.Attributes;
using System.ComponentModel;
using System.Windows;

namespace DataFileComparer
{
    public abstract class CWindow<T> : Window where T : CWindowData, new()
    {
        private const string COMP_INIT_METHOD = "InitializeComponent";

        /// <summary>
        /// Properties
        /// </summary>
        [IgnoredProperty]
        public T WindowData { get; private set; }

        /// <summary>
        /// Events
        /// </summary>
        public virtual T InitData(CancelEventArgs e) { return new T(); }
        public virtual void ClosingWindow(object s, CancelEventArgs e) { }

        public CWindow()
        {
            GetType().GetMethod(COMP_INIT_METHOD).Invoke(this, null);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var initDataFlg = new CancelEventArgs(false);
            WindowData = InitData(initDataFlg);
            DataContext = WindowData;
            
            Loaded += (s, e) =>
            {
                WindowData.InitWindow(this, initDataFlg);
            };

            Closing += (s, e) =>
            {
                ClosingWindow(s, e);
            };
        }
    }
}