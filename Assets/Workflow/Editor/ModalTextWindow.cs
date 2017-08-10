using UnityEngine;
using UnityEditor;

namespace Workstation
{
    public class ModalTextWindow : EditorWindow
    {
        public delegate void OnModalTextWindowFinished(int id, string modalText, bool isOk);
        OnModalTextWindowFinished onFinishedCallback;
        string text;
        string description;
        int id;

        public static ModalTextWindow OpenWindow(int id, string title, string description, string defaultText, OnModalTextWindowFinished onFinishedFunc)
        {
            var w = GetWindow<ModalTextWindow>(true, title);
            w.id = id;
            w.description = description;
            w.text = defaultText;
            w.onFinishedCallback = onFinishedFunc;
            
            return w;
        }

        private void OnInspectorUpdate()
        {
            if (onFinishedCallback == null) {
				CloseWindow();
            }
        }

		private void OnDestroy()
		{
			// This is for if we close the window manually, because our CloseWindow wipes onFinishedCallback.
			if (onFinishedCallback != null) {
				onFinishedCallback(id, "", false);
				onFinishedCallback = null;
			}
		}

		void OnGUI()
        {
            GUILayout.Label(description);
            text = EditorGUILayout.TextArea(text);

            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("OK")) {
                    if (onFinishedCallback != null) {
						onFinishedCallback(id, text, true);
						CloseWindow();
					}
                }
                if (GUILayout.Button("Cancel")) {
					if (onFinishedCallback != null) {
						onFinishedCallback(id, "", false);
						CloseWindow();
					}
                }
            }
        }

		private void CloseWindow()
		{
			if (onFinishedCallback != null) {
				onFinishedCallback = null;
			}
			this.Close();
		}
	}
}
