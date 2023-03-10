using DataFileComparer.Attributes;
using DataFileComparer.Commons;
using Newtonsoft.Json;
using System.Windows;

namespace DataFileComparer
{
    public abstract class CWindowData : AutoNotifiableObject
    {
        /// <summary>
        /// Window Values
        /// </summary>
        [JsonIgnore, IgnoredProperty]
        public Window Window { get; private set; }

        /// <summary>
        /// Events
        /// </summary>
        public virtual void New() { }
        public virtual void OnLoad() { }
        public virtual void OnDestroy() { }

        /// <summary>
        /// Set parent window
        /// </summary>
        /// <param name="iWindow"></param>
        public virtual void InitWindow(Window iWindow, bool allowLoad)
        {
            Window = iWindow;

            //load
            if (allowLoad)
            {
                Pause();
                OnLoad();
                Unpause();
            }
        }

        ~CWindowData()
        {
            Pause();
            OnDestroy();
        }
    }
}