using Microsoft.CSharp;
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
	private string axesPath = @"Standard Assets/Scripts/Auto/Axes.cs";
	private string tagsPath = @"Standard Assets/Scripts/Auto/Tags.cs";
	private string sortingLayersPath = @"Standard Assets/Scripts/Auto/SortingLayers.cs";
	private string layersPath = @"Standard Assets/Scripts/Auto/Layers.cs";
	private string scenesPath = @"Standard Assets/Scripts/Auto/Scenes.cs";

	[MenuItem("Window/Code Generator")]
	private static void CallCreateWindow()
	{
		// Get existing open window or if none, make a new one:
		CodeGenerator_Window window = EditorWindow.GetWindow<CodeGenerator_Window>();
		window.autoRepaintOnSceneChange = true;
		window.titleContent = new GUIContent("Code Generator");
		window.Show();
	}

	private void OnInspectorUpdate()
	{
		this.Repaint();
	}

	private void OnGUI()
	{
		if (DrawGenerationGui("Input", ref this.axesPath)) {
			Generate(this.axesPath, GetAllAxisNames);
		}
		EditorGUILayout.Separator();
		if (DrawGenerationGui("Tags", ref this.tagsPath)) {
			Generate(this.tagsPath, GetAllTags);
		}
		EditorGUILayout.Separator();
		if (DrawGenerationGui("Sorting layers", ref this.sortingLayersPath)) {
			Generate(this.sortingLayersPath, GetAllSortingLayers);
		}
		EditorGUILayout.Separator();
		if (DrawGenerationGui("Layers", ref this.layersPath)) {
			Generate(this.layersPath, GetAllLayers);
		}
		EditorGUILayout.Separator();
		if (DrawGenerationGui("Scenes", ref this.scenesPath)) {
			Generate(this.scenesPath, GetAllSceneNames);
		}
		EditorGUILayout.Separator();
		if (GUILayout.Button("Generate all", GUILayoutExt.ExpandHeight)) {
			Generate(this.axesPath, GetAllAxisNames);
			Generate(this.tagsPath, GetAllTags);
			Generate(this.sortingLayersPath, GetAllSortingLayers);
			Generate(this.layersPath, GetAllLayers);
			Generate(this.scenesPath, GetAllSceneNames);
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

	private static bool DrawGenerationGui(string title, ref string path)
	{
		bool output = false;
		using (new EditorGUILayout.VerticalScope(EditorStyles.inspectorFullWidthMargins)) {
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel, GUILayout.MaxWidth(100), GUILayoutExt.NoExpandWidth);
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(@"Path: /Assets/... + ", GUILayout.MaxWidth(120), GUILayoutExt.NoExpandWidth);
				path = EditorGUILayout.TextField(path, EditorStyles.textField, GUILayoutExt.ExpandWidth, GUILayout.MinWidth(200));
			}

			using (new EditorGUILayout.HorizontalScope()) {
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Generate", EditorStyles.miniButton)) {
					output = true;
				}
				GUILayout.FlexibleSpace();
			}

		}

		return output;
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
	private static IEnumerable<string> GetAllAxisNames()
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