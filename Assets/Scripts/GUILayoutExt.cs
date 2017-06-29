using UnityEngine;

public class GUILayoutExt
{
	/// <summary>
	/// Use these to recycle instead of calling new ones.
	/// </summary>
	public static readonly GUILayoutOption ExpandWidth = GUILayout.ExpandWidth(true),
		ExpandHeight = GUILayout.ExpandHeight(true),
		NoExpandWidth = GUILayout.ExpandWidth(false),
		NoExpandHeight = GUILayout.ExpandHeight(false);
}