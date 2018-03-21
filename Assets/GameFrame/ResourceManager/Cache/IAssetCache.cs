using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public interface IAssetCache  {
		string Name { get; }// 资源名称
		Object Asset { get; }// 资源实体
		bool PermanentMemory { get; }// 是否常驻内存
		bool IsDisposeable { get; }// 是否可以销毁
		float LastUseTime { get; }// 上一次使用的时间
		void SetName(string name);
		void SetAsset(Object asset);
		void SetLastUseTime(float time);
		void SetPermanentMemory(bool Permanent);
		void Dispose();// 销毁
		void ForcedDispose();// 强制销毁
	}
}
