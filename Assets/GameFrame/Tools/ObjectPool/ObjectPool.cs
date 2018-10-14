using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFrame
{
	public class ObjectPool<T>where T :class ,new()
	{
		private Queue<T> m_Objects = null;
		private int m_MaxNum = 5;
		private int m_AllocNum = 0;
		private bool m_CreateWhenPoolIsFull;
		
		public int Count {
			get { return m_Objects == null ? 0 : m_Objects.Count; }
		}

		public bool IsFull {
			get { return Count >= m_MaxNum; }
		}

		public ObjectPool()
		{
			m_Objects = new Queue<T>(m_MaxNum);
			m_CreateWhenPoolIsFull = true;
		}
		public ObjectPool(int maxNum, bool createWhenPoolIsFull = true)
		{
			m_MaxNum = maxNum;
			m_CreateWhenPoolIsFull = createWhenPoolIsFull;
			m_Objects = new Queue<T>(m_MaxNum);
		}

		public T GetObject()
		{
		    this.m_AllocNum++;

			if (m_Objects.Count > 0)
			{
				T item = m_Objects.Dequeue();
				if (item != null)
				{
					return item;
				}

			}
			if (m_CreateWhenPoolIsFull)
			{
				return new T();
			}
			return null;
		}
		public bool PutObject(T item)
		{
		    this.m_AllocNum--;
			if (item == null)
				return false;

			if (m_Objects.Count < m_MaxNum)
			{
				m_Objects.Enqueue(item);
				return true;
			}
			return false;
		}

		public T[] ToArray()
		{
			return m_Objects.ToArray<T>();
		}
	}

}

