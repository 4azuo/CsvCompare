using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DataFileComparer.Commons
{
    public static class WindowUtil
    {
        /// <summary>
        /// Consts
        /// </summary>
        private const int ASYNC_PERIOD = 100;
        private const int MAX_LOAD_ASYNC = 1;

        /// <summary>
        /// Load list async
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="dstList"></param>
        /// <param name="srcList"></param>
        /// <param name="formatter"></param>
        public static void LoadAsync<A, B>(IList<A> dstList, IList<B> srcList, Func<B, A> formatter, Action callback = null, int period = ASYNC_PERIOD, int onceLoad = MAX_LOAD_ASYNC)
        {
            var max = srcList.Count;
            var cnt = 0;
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(period), DispatcherPriority.Background,
                (s, e) =>
                {
                    for (; cnt < Math.Min(max, cnt + onceLoad); cnt++)
                    {
                        var item = formatter(srcList[cnt]);
                        if (item != null)
                            dstList.Add(item);
                    }
                    if (cnt >= max)
                    {
                        ((DispatcherTimer)s).Stop();
                        callback?.Invoke();
                    }
                }, Application.Current.Dispatcher);
            timer.Start();
        }
        public static Task LoadAsync2<A, B>(IList<A> dstList, IList<B> srcList, Func<B, A> formatter, Action callback = null, int period = ASYNC_PERIOD)
        {
            return Task.Run(() =>
            {
                for (var cnt = 0; cnt < srcList.Count; cnt++)
                {
                    var item = formatter(srcList[cnt]);
                    if (item != null)
                        dstList.Add(item);
                }
                callback?.Invoke();
            });
        }

        /// <summary>
        /// Load list async
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="dstList"></param>
        /// <param name="src"></param>
        /// <param name="formatter"></param>
        public static void LoadAsync<A, B>(IList<A> dstList, Func<IList<B>> src, Func<B, A> formatter, Action callback = null, int period = ASYNC_PERIOD, int onceLoad = MAX_LOAD_ASYNC)
        {
            IList<B> srcList = null;
            Task.WhenAll(Task.Run(() => srcList = src.Invoke()))
                .ContinueWith(x =>
                {
                    var max = srcList.Count;
                    var cnt = 0;
                    var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(period), DispatcherPriority.Background,
                        (s, e) =>
                        {
                            for (; cnt < Math.Min(max, cnt + onceLoad); cnt++)
                            {
                                var item = formatter(srcList[cnt]);
                                if (item != null)
                                    dstList.Add(item);
                            }
                            if (cnt >= max)
                            {
                                ((DispatcherTimer)s).Stop();
                                callback?.Invoke();
                            }
                        }, Application.Current.Dispatcher);
                    timer.Start();
                });
        }
        public static Task LoadAsync2<A, B>(IList<A> dstList, Func<IList<B>> src, Func<B, A> formatter, Action callback = null)
        {
            IList<B> srcList = null;
            return Task.WhenAll(Task.Run(() => srcList = src.Invoke()))
                .ContinueWith(x =>
                {
                    for (var cnt = 0; cnt < srcList.Count; cnt++)
                    {
                        var item = formatter(srcList[cnt]);
                        if (item != null)
                            dstList.Add(item);
                    }
                    callback?.Invoke();
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
