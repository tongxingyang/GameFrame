using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class ConcurrentQueue<T>
	{
		private Queue<T> m_inner = new Queue<T>();
	    public readonly object m_obj = new object();
        public bool TryDequeue(out T item)
		{
			lock (m_obj)
			{
				if (m_inner.Count == 0)
				{
					item = default(T);
					return false;
				}
				item = m_inner.Dequeue();
				return true;
			}
		}

		public void Enqueue(T item)
		{
			lock (m_obj)
			{
				m_inner.Enqueue(item);
			}
		}

		public int Count
		{
			get
			{
				lock (m_obj)
				{
					return m_inner.Count;
				}
			}
		}

		public void Clear()
		{
			lock (m_obj)
			{
				m_inner.Clear();
			}
		}
        
	}

}
