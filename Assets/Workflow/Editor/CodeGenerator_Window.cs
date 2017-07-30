﻿using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// Scenes was provided by TagsLayersSceneBuilder.cs

public class CodeGenerator_Window : EditorWindow
{
	private string folderPath = @"Standard Assets/Scripts/Auto/";
	private string axesPath = @"Axes.cs";
	private string tagsPath = @"Tags.cs";
	private string sortingLayersPath = @"SortingLayers.cs";
	private string layersPath = @"Layers.cs";
	private string scenesPath = @"Scenes.cs";

	private const int SELECTION_FLAG_AXES = 1 << 0,
		SELECTION_FLAG_TAGS = 1 << 1,
		SELECTION_FLAG_SORTING_LAYERS = 1 << 2,
		SELECTION_FLAG_LAYERS = 1 << 3,
		SELECTION_FLAG_SCENES = 1 << 4;

	private int m_selectionFlag;

	[MenuItem("Window/Code Generator")]
	private static void CallCreateWindow()
	{
		// Get existing open window or if none, make a new one:
		CodeGenerator_Window window = EditorWindow.GetWindow<CodeGenerator_Window>();
		window.autoRepaintOnSceneChange = true;
		window.titleContent = new GUIContent("Code Generator");
		window.Load();
		window.Show();
	}

	private void Load()
	{
		// Activate all the options
		m_selectionFlag = SELECTION_FLAG_AXES | SELECTION_FLAG_LAYERS | 
			SELECTION_FLAG_SCENES | SELECTION_FLAG_SORTING_LAYERS | SELECTION_FLAG_TAGS;
	}

	private void OnInspectorUpdate()
	{
		this.Repaint();
	}

	private void OnGUI()
	{
		using (new GUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins)) {
			EditorGUILayout.LabelField(new GUIContent("Folder Path", "Folder Path"), EditorStyles.boldLabel, 
			                           GUILayout.MaxWidth(100), GUILayoutExt.NoExpandWidth);

			using (new EditorGUILayout.HorizontalScope()) {
				GUILayout.Space(19);
				EditorGUILayout.LabelField(@"../Assets/", GUILayout.MaxWidth(60), GUILayoutExt.NoExpandWidth);
				folderPath = EditorGUILayout.TextField(folderPath, EditorStyles.textField, 
				                                       GUILayout.MinWidth(200), GUILayoutExt.ExpandWidth);
			}
		}
		EditorGUILayout.Separator();

		DrawGenerationGui(ref m_selectionFlag, SELECTION_FLAG_AXES, "Input", ref this.axesPath);
		DrawGenerationGui(ref m_selectionFlag, SELECTION_FLAG_TAGS, "Tags", ref this.tagsPath);
		DrawGenerationGui(ref m_selectionFlag, SELECTION_FLAG_SORTING_LAYERS, "Sorting layers", ref this.sortingLayersPath);
		DrawGenerationGui(ref m_selectionFlag, SELECTION_FLAG_LAYERS, "Layers", ref this.layersPath);
		DrawGenerationGui(ref m_selectionFlag, SELECTION_FLAG_SCENES, "Scenes", ref this.scenesPath);

		using (new EditorGUI.DisabledGroupScope(m_selectionFlag == 0)) {
			if (GUILayout.Button("Generate", GUILayoutExt.ExpandHeight)) {
				if ((m_selectionFlag & SELECTION_FLAG_AXES) == SELECTION_FLAG_AXES)
					Generate(Path.Combine(this.folderPath, this.axesPath), GetAllAxesNames);
				if ((m_selectionFlag & SELECTION_FLAG_TAGS) == SELECTION_FLAG_TAGS)
					Generate(Path.Combine(this.folderPath, this.tagsPath), GetAllTags);
				if ((m_selectionFlag & SELECTION_FLAG_SORTING_LAYERS) == SELECTION_FLAG_SORTING_LAYERS)
					Generate(Path.Combine(this.folderPath, this.sortingLayersPath), GetAllSortingLayers);
				if ((m_selectionFlag & SELECTION_FLAG_LAYERS) == SELECTION_FLAG_LAYERS)
					Generate(Path.Combine(this.folderPath, this.layersPath), GetAllLayers);
				if ((m_selectionFlag & SELECTION_FLAG_SCENES) == SELECTION_FLAG_SCENES)
					Generate(Path.Combine(this.folderPath, this.scenesPath), GetAllSceneNames);
			}
		}
	}

	private static void Generate(string path, System.Func<IEnumerable<string>> namesProvider)
	{
		try {
			GenerateAndForceImport(path, namesProvider);
			System.GC.Collect();
		}
		catch (System.Exception ex) {
			Debug.LogException(ex);
		}
	}
	
	private static void DrawGenerationGui(ref int selectionFlag, int select, string title, ref string path)
	{
		using (new GUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins)) {
			using (new EditorGUILayout.HorizontalScope()) {
				bool prevEnabled = (selectionFlag & select) == select;
				bool enabled = EditorGUILayout.Toggle(prevEnabled, GUILayout.Width(15));
				if (enabled != prevEnabled) {
					selectionFlag ^= select;
				}
				using (new EditorGUI.DisabledGroupScope(!enabled)) {
					EditorGUILayout.LabelField(new GUIContent(title, title), EditorStyles.boldLabel, GUILayout.MaxWidth(100), GUILayoutExt.NoExpandWidth);
					EditorGUILayout.LabelField(@"Filename: ", GUILayout.MaxWidth(60), GUILayoutExt.NoExpandWidth);
					path = EditorGUILayout.TextField(path, EditorStyles.textField, GUILayout.MinWidth(200), GUILayoutExt.ExpandWidth);
				}
			}
		}
		EditorGUILayout.Separator();
	}

	#region code generation
	private static void GenerateAndForceImport(string path, System.Func<IEnumerable<string>> namesProvider)
	{
		path = path.Replace("\\", "/");
		var fullPath = Path.Combine(Application.dataPath, path);

		var names = namesProvider();
		if (names.Any()) {
			GenerateNamesCodeFile(fullPath, names);
			AssetDatabase.ImportAsset("Assets/" + path, ImportAssetOptions.ForceUpdate);
			AssetDatabase.Refresh();
		}
		else {
			var name = Path.GetFileNameWithoutExtension(path);
			EditorUtility.DisplayDialog("No data", "No names found for " + name, "Close");
		}
	}

	private static void GenerateNamesCodeFile(string fullPath, IEnumerable<string> names)
	{
		var name = Path.GetFileNameWithoutExtension(fullPath);
		var constants = names.ToDictionary(s => ConvertToValidIdentifier(s), s => s);
		var code = CreateStringConstantsClass(name, constants);

		Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

		using (var stream = new StreamWriter(fullPath, append: false)) {
			var tw = new IndentedTextWriter(stream);
			var codeProvider = new CSharpCodeProvider();
			var options = new CodeGeneratorOptions() {
				BlankLinesBetweenMembers = false
			};
			codeProvider.GenerateCodeFromCompileUnit(code, tw, options);
		}
	}

	private static CodeCompileUnit CreateStringConstantsClass(
		string name,
		IDictionary<string, string> constants)
	{
		var compileUnit = new CodeCompileUnit();
		var @namespace = new CodeNamespace();

		var @class = new CodeTypeDeclaration(name);

		ImitateStaticClass(@class);

		foreach (var pair in constants) {
			var @const = new CodeMemberField(typeof(string), pair.Key);
			@const.Attributes &= ~MemberAttributes.AccessMask;
			@const.Attributes &= ~MemberAttributes.ScopeMask;
			@const.Attributes |= MemberAttributes.Public;
			@const.Attributes |= MemberAttributes.Const;

			@const.InitExpression = new CodePrimitiveExpression(pair.Value);
			@class.Members.Add(@const);
		}

		@namespace.Types.Add(@class);
		compileUnit.Namespaces.Add(@namespace);

		return compileUnit;
	}

	/// <summary>
	/// Marks class as sealed and adds private constructor to it.
	/// </summary>
	/// <remarks>
	/// It's not possible to create static class using CodeDom.
	/// Creating abstract sealed class instead leads to compilation error.
	/// This method can be used instead to make pseudo-static class.
	/// </remarks>
	private static void ImitateStaticClass(CodeTypeDeclaration type)
	{
		@type.TypeAttributes |= TypeAttributes.Sealed;

		@type.Members.Add(new CodeConstructor
		{
			Attributes = MemberAttributes.Private | MemberAttributes.Final
		});
	}

	private static string ConvertToValidIdentifier(string name)
	{
		var sb = new StringBuilder(name.Length + 1);

		// Prefix "_" to names starting with a number.
		if (!char.IsLetter(name[0]))
			sb.Append('_');

		var makeUpper = false;
		foreach (var ch in name) {
			if (char.IsLetterOrDigit(ch)) {
				sb.Append(makeUpper ? char.ToUpperInvariant(ch) : ch);
				makeUpper = false;
			}
			else if (char.IsWhiteSpace(ch)) {
				makeUpper = true;
			}
			else {
				sb.Append('_');
			}
		}

		return sb.ToString();
	}
	#endregion
	
	#region names providers
	private static IEnumerable<string> GetAllAxesNames()
	{
		var result = new StringCollection();

		var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		var axesProperty = serializedObject.FindProperty("m_Axes");

		axesProperty.Next(true);
		axesProperty.Next(true);

		while (axesProperty.Next(false)) {
			SerializedProperty axis = axesProperty.Copy();
			axis.Next(true);
			result.Add(axis.stringValue);
		}

		return result.Cast<string>().Distinct();
	}

	private static IEnumerable<string> GetAllTags()
	{
		return new ReadOnlyCollection<string>(InternalEditorUtility.tags);
	}

	private static IEnumerable<string> GetAllSortingLayers()
	{
		var t = typeof(InternalEditorUtility);
		var sortingLayersProperty = t.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		var sortingLayers = (string[])sortingLayersProperty.GetValue(null, new object[0]);

		return new ReadOnlyCollection<string>(sortingLayers);
	}

	private static IEnumerable<string> GetAllLayers()
	{
		return new ReadOnlyCollection<string>(InternalEditorUtility.layers);
	}

	private static IEnumerable<string> GetAllSceneNames()
	{
		var sceneNames = EditorBuildSettings.scenes.Select(s => Path.GetFileNameWithoutExtension(s.path)).ToList();

		return new ReadOnlyCollection<string>(sceneNames);
	}

	#endregion
}