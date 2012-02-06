using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Superiority.Model.Util
{
    public class MemoryPool<T> where T : new()
    {
        protected Stack<T> items;
        protected readonly object sync;

        public MemoryPool()
        {
            items = new Stack<T>();
            sync = new object();
        }

        public T Pull()
        {
            lock (sync)
            {
                if (items.Count == 0)
                    return new T();
                else
                    return items.Pop();
            }
        }

        public void Push(T item)
        {
            lock (sync)
            {
                items.Push(item);
            }
        }
    }
}
