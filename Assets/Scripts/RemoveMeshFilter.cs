using UnityEngine;

public class RemoveMeshFilterFromSubObjects : MonoBehaviour
{
	private delegate void ChildHandler(GameObject child);
	[InspectorButton("OnActivatePress")]
	public bool Activate;
	
	private void OnActivatePress()
	{
		DoIterate(gameObject, ClearMeshFilter, true);
		Activate = false;
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

	private void ClearMeshFilter(GameObject gameObject)
	{
		if (gameObject == null) {
			Debug.LogWarning("gameObject was null!");
		}

		var meshFilter = gameObject.GetComponent<MeshFilter>();
		var meshRenderer = gameObject.GetComponent<MeshRenderer>();

		if (meshFilter) {
			DestroyImmediate(meshFilter);
		}
		if (meshRenderer) {
			DestroyImmediate(meshRenderer);
		}

	}
}