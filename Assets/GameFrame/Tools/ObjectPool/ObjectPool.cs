using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class ObjectPool<T>where T :class ,new()
	{
		private T t;
		public T GetType
		{
			get { return t; }
		}
		private Queue<T> m_Object = null;
		private int m_MaxNum = 5;
		private int m_AllocNum = 0;
		private bool m_CreateWhenPoolIsFull;

		public int Count
		{
			get { return m_Object == null ? 0 : m_Object.Count; }
		}

		public bool IsFull
		{
			get { return Count > m_MaxNum; }
		}

		public ObjectPool()
		{
			m_Object = new Queue<T>(m_MaxNum);
			m_CreateWhenPoolIsFull = true;
		}

		public ObjectPool(int maxNum, bool createWhenPoolIsFull = true)
		{
			m_MaxNum = maxNum;
			m_CreateWhenPoolIsFull = createWhenPoolIsFull;
			m_Object = new Queue<T>(m_MaxNum);
		}

		public T GetObject()
		{
			m_AllocNum++;
			if (m_Object.Count > 0)
			{
				T item = m_Object.Dequeue();
				if (item != null)
				{
					return item;
				}
			}
			if (m_CreateWhenPoolIsFull)
			{
				m_MaxNum++;
				return new T();
			}
			return null;
		}

		public bool PutObject(T item)
		{
			m_AllocNum--;
			if (item == null) return false;
			if (m_Object.Count < m_MaxNum)
			{
				m_Object.Enqueue(item);
				return true;
			}
			return false;
		}

		public T[] GetArray()
		{
			return m_Object.ToArray();
		}
	}

}

