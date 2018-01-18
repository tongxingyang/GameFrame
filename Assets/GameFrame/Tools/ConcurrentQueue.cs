using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	/// <summary>
	/// 自定义currentQueue
	/// </summary>
	public class ConcurrentQueue<T>
	{
		private readonly Queue<T> m_inner = new Queue<T>();

		public bool TryDequeue(out T item)
		{
			lock (Singleton<UpdateManager>.GetInstance().m_obj)
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
			lock (Singleton<UpdateManager>.GetInstance().m_obj)
			{
				m_inner.Enqueue(item);
			}
		}

		public int Count
		{
			get
			{
				lock (Singleton<UpdateManager>.GetInstance().m_obj)
				{
					return m_inner.Count;
				}
			}
		}

		public void Clear()
		{
			lock (Singleton<UpdateManager>.GetInstance().m_obj)
			{
				m_inner.Clear();
			}
		}
        
	}

}
