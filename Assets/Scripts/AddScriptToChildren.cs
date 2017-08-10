using UnityEngine;

[ExecuteInEditMode]
public class AddScriptToChildren : MonoBehaviour
{
	private delegate void ChildHandler(GameObject child);
	public System.Type scriptType = typeof(ReCalcCubeTexture);
	public bool Activate;

	private void OnValidate()
	{
		if (Activate) {
			DoIterate(gameObject, AddScript, true);
			Activate = false;
		}
	}

	private void DoIterate(GameObject gameObject, ChildHandler childHandler, bool recursive)
	{
		if (gameObject == null || childHandler == null) {
			Debug.LogWarningFormat("{0} was null!", gameObject == null ? "gameObject" : "childHandler");
			return;
		}

		foreach (Transform child in gameObject.transform) {
			childHandler(child.gameObject);
			if (recursive) {
				DoIterate(child.gameObject, childHandler, true);
			}
		}
	}

	private void AddScript(GameObject gameObject)
	{
		if (gameObject == null) {
			Debug.LogWarning("gameObject was null!");
		}

		var scriptComp = gameObject.GetComponent(scriptType);
		if (!scriptComp) {
			gameObject.AddComponent(scriptType);
		}
	}
}