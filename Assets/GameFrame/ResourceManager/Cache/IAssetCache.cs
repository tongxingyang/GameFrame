using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public interface IAssetCache  {
		/// <summary>
		/// 资源名称
		/// </summary>
		string Name { get; }
		/// <summary>
		/// 资源
		/// </summary>
		Object Asset { get; }
		/// <summary>
		/// 常驻内存标志
		/// </summary>
		bool Permanent { get; set; }
		/// <summary>
		/// 最后一次使用的时间
		/// </summary>
		CachePriority Priority { get; }
		
		float LastUseTime { get; }
		bool IsDisposeable { get; }
		void SetName(string name);
		void SetAsset(Object obj);
		void SetLastUseTime(float time);

		void SetCachePriority(CachePriority cachePriority);
		bool Dispose();
		void ForceDispose();
	}
}
