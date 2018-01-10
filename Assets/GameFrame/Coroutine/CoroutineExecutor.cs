using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineExecutor : MonoBehaviour {
	private Action FinishCallbcak;
	void Start(){
		DontDestroyOnLoad(transform.gameObject);
	}
	private void Do(AsyncOperation ao,Action callback){
		FinishCallbcak = callback;
		StartCoroutine(WaitForDone(ao));
	}
	IEnumerator WaitForDone(AsyncOperation ao){
		if(ao != null){
			while(!ao.isDone){
				yield return 1;
			}
		}
		if(FinishCallbcak!=null){
			FinishCallbcak();
		}
	    Destroy(this.gameObject);
		yield return 0;
	}
	private void Do(IEnumerator ao,Action callback){
		FinishCallbcak = callback;
		StartCoroutine(WaitForDone(ao));
	}
	IEnumerator WaitForDone(IEnumerator ao){
		if(ao != null){
			yield return ao;
		}
		if(FinishCallbcak!=null){
			FinishCallbcak();
		}
		Destroy(this.gameObject);
		yield return 0;
	}

	public static void Create(AsyncOperation ao, System.Action callback)
	{
		GameObject go = new GameObject();
		CoroutineExecutor executor = go.AddComponent<CoroutineExecutor>();
		if (executor != null)
		{
			executor.Do(ao, callback);
		}
	}

	public static void Create(IEnumerator routine, System.Action callback)
	{
		GameObject go = new GameObject();
		CoroutineExecutor executor = go.AddComponent<CoroutineExecutor>();
		if (executor != null)
		{
			executor.Do(routine, callback);
		}
	}
}
