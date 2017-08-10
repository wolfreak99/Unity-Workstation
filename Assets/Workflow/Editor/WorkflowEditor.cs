using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System;
using System.IO;

namespace Workstation
{
	public class WorkflowEditor : ScriptableObject
	{
		[MenuItem("Tools/GC Collect")]
		static void MenuGCCollect()
		{
			System.GC.Collect();
		}

        private static void EngageHierarchyRename()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(type);
            var rename = type.GetMethod("RenameGO", BindingFlags.Instance | BindingFlags.NonPublic);
            rename.Invoke(hierarchyWindow, null);
        }
		
		// NOTE Some reason, Workflow isn't recognized when compiling in unity, and I'm too lazy
		// To investigate right now. This is a duplicate of the function found in there.
		public static bool MoveTransformsUnderParent(Transform newParent, params Transform[] transforms)
		{
			if (newParent == null) {
				Debug.LogErrorFormat("ParentTransform is null");
				return false;
			} else if (transforms == null || transforms.Length == 0) {
				Debug.LogWarningFormat("Transforms is null or empty");
				return false;
			}

			if (transforms.Length == 1) {
				Transform originalParent = transforms[0].parent;
				transforms[0].parent = newParent;
				if (originalParent)
					newParent.parent = originalParent;
			} else {
				foreach (Transform t in transforms) {
					t.parent = newParent;
				}
			}
			return true;
		}
		[MenuItem("GameObject/+Add Parent")]
		static void MenuInsertParent()
		{
			Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel |
				SelectionMode.OnlyUserModifiable);

			if (transforms.Length == 0) {
				Debug.LogWarning("No objects selected to parent!");
				return;
			}
            
			GameObject newParent = new GameObject("_NewParent");
			Transform newParentTransform = newParent.transform;
            
           MoveTransformsUnderParent(newParentTransform, transforms);

            Selection.activeGameObject = newParent;
		}

		[MenuItem("Tools/Download Image from URL...")]
		static void MenuDownloadImageFromUrl()
		{
			MenuDownloadImageFromUrlClass.Open();
		}

		public class MenuDownloadImageFromUrlClass
		{
			static readonly int ModalGuiId = Animator.StringToHash("MenuDownloadImageFromUrlClass_ModalGui");
			static IEnumerator ienumerator;
			static WWW wwwData;

			public static void Open()
			{
				ModalTextWindow.OpenWindow(ModalGuiId, "Download image from url", "Enter url", "", OnModalFinished);
			}

			static void OnModalFinished(int id, string inputUrl, bool isOk)
			{
				if (!isOk) {
					return;
				}

				if (id == ModalGuiId) {
					// Create project file from url
					var path = "Download/" + Path.GetFileName(inputUrl);
					var pathFull = Path.Combine(Application.dataPath, path);

					// Create directory
					if (!Directory.Exists(Path.GetDirectoryName(pathFull))) {
						Directory.CreateDirectory(Path.GetDirectoryName(pathFull));
					}

					wwwData = new WWW(inputUrl);
					ienumerator = DoCoroutine(path, pathFull);
					EditorCoroutine.Start(ienumerator);
				}
			}

			static IEnumerator DoCoroutine(string path, string pathFull)
			{
				while (!wwwData.isDone) {
					yield return wwwData;
				}

				// Save to file
				File.WriteAllBytes(pathFull, wwwData.bytes);
				AssetDatabase.ImportAsset("Assets/" + path, ImportAssetOptions.ForceUpdate);
				AssetDatabase.Refresh();
			}
		}
		
    }
}