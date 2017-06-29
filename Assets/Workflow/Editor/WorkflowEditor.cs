using UnityEngine;
using UnityEditor;

namespace Workstation
{
	public class WorkflowEditor : EditorWindow
	{
		[MenuItem("Tools/GC Collect")]
		public static void GCCollect()
		{
			System.GC.Collect();
		}
	}
}