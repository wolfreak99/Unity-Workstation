using System;
using IEnumerator = System.Collections.IEnumerator;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

public class EditorCoroutine
{
	public static EditorCoroutine Start(IEnumerator _routine)
	{
		EditorCoroutine coroutine = new EditorCoroutine(_routine);
		coroutine.Start();
		return coroutine;
	}

	readonly IEnumerator _routine;
	
	EditorCoroutine(IEnumerator routine)
	{
		this._routine = routine;
	}
	
	void Start()
	{
		EditorApplication.update += Update;
	}

	public void Stop()
	{
		EditorApplication.update -= Update;
	}

	void Update()
	{
		/* NOTE: no need to try/catch MoveNext,
			* if an IEnumerator throws its next iteration returns false.
			* Also, Unity probably catches when calling EditorApplication.update.
			*/

		//Debug.Log("update");

		if (!_routine.MoveNext()) {
			Stop();
		}

	}
}
