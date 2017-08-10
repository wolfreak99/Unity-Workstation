using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class TempWindow : EditorWindow
{
    string m_path = "UnifyGithub/Scripts/";
    string m_category;
    string m_subCategory;
    string m_lineText;
    string m_outputText;
    const string LbStr = "\n";

    [MenuItem("Window/Temp")]
    private static void CallCreateWindow()
    {
        // Get existing open window or if none, make a new one:
        TempWindow window = EditorWindow.GetWindow<TempWindow>();
        window.autoRepaintOnSceneChange = true;
        window.titleContent = new GUIContent("Temp");
        window.Show();
    }
    
    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope()) {
            m_path = EditorGUILayout.TextField(m_path, GUILayout.Width(200));
            m_category = EditorGUILayout.TextField(m_category);
            GUILayout.Label("/");
        }
        using (new EditorGUILayout.HorizontalScope()) {
            GUILayoutOption[] textOptions = new GUILayoutOption[] {
                GUILayout.Height(500f),
                GUILayout.Width((this.position.width / 2f) - 10f)
            };
            m_lineText = EditorGUILayout.TextArea(m_lineText, textOptions);
            m_outputText = EditorGUILayout.TextArea(m_outputText, textOptions);
        }

        if (GUILayout.Button("Generate", GUILayout.Height(40))) {
            m_subCategory = "";
            m_outputText = "";
            string[] multiLines = ToMultiLine(m_lineText);
            string[] convLines = ParseLines(multiLines);
            m_outputText = ToSingleLine(convLines);
            Repaint();
        }
    }

    string[] ParseLines(string[] lines)
    {
        string[] names = new string[lines.Length + 10];
        int count = 0;
        names[count++] = "private static bool TryGetSubCategory_" + m_category + "(string scriptName, out string subCatName)";
        names[count++] = "{";
        names[count++] = "    switch (scriptName) {";
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            // Skip empty lines
            if (IsStringEmpty(line, true)) {
                continue;
            }

            string split = " - ";
            if (!line.Contains(split)) {
                // Normal namesplit does not exist, see if an alternative one exists.
                if (line.Contains(" -")) {
                    split = " -";
                }
                else if (line.Contains("- ")) {
                    split = "- ";
                }
                /*
                // Only use this one if needed
                else if (line.Contains("-")) {
                    split = "-";
                }
                */
                else {
                    // No appropriate namesplits exist, so use category
                    m_subCategory = ToCamelCase(line);
                    continue;
                }
            }
            
            int scriptNameEnd = line.IndexOf(split);
            string name = line.Substring(0, scriptNameEnd).Replace(" ", "_");
            
            // Format to match functionality that will be used.
            //string path = MergeWithSlash(m_path, m_category, m_subCategory, name + ".cs");
            names[count++] = string.Format("        case \"{0}\": subCatName = \"{1}\"; return true;", name, m_subCategory);
        }
        names[count++] = "        default: subCatName = NoSubCategory; return false;";
        names[count++] = "    }";
        names[count++] = "}";
        return names;
    }


    static bool IsStringEmpty(string str, bool includeBreaks)
    {
        if (str == null || str == "" || str == " ")
            return true;
        else if (includeBreaks && str == LbStr)
            return true;
        else
            return false;
    }

    const int ToMultiLineFailAt = 500;
    static string[] ToMultiLine(string singleLine)
    {
        // I have a feeling if it's just "" or " ", it won't contain a line break..
        if (IsStringEmpty(singleLine, true) || !singleLine.Contains(LbStr)) {
            return new string[1] { singleLine };
        }

        List<string> result = new List<string>();
        int count = 0;
        string line, newSingleLine;
        while (true) {
            // If loop exceeds limit
            if (count > ToMultiLineFailAt) {
                Debug.LogWarningFormat("while loop exceeded {0} times, breaking early!", 
                    ToMultiLineFailAt.ToString());
                break;
            }
            if (!singleLine.Contains(LbStr)) {
                break;
            }
            int breakIndex = singleLine.IndexOf(LbStr);

            line = singleLine.Substring(0, breakIndex);
            line = ToSafeString(line, extraAllowedChars);
            if (!IsStringEmpty(line, true)) {
                result.Add(line);
            }
            newSingleLine = singleLine.Remove(0, breakIndex + 1);
            singleLine = newSingleLine;

            count++;
        }

        return result.ToArray();
    }
    static string ToSingleLine(string[] multiLine)
    {
        string result = "";
        foreach (string str in multiLine) {
            if (IsStringEmpty(str, true))
                continue;

            result += str + LbStr;
        }
        return result;
    }

    static string extraAllowedChars = "_- (),/";
    static string ToSafeString(string str, string additionalChars = "")
    {
        string res = "";
        foreach (char c in str) {
            if (char.IsLetterOrDigit(c) || additionalChars.Contains(c.ToString())) {
                res += c;
            }
            else if (c == '&') {
                res += "%26";
            }
        }
        return res;
    }
    
    static string ToCamelCase(string str)
    {
        if (!str.Contains(" "))
            return str;

        string res = "";
        string strUpper = str.ToUpper();

        for (int i = 0; i < str.Length; i++) {
            if (str[i] == ' ') {
                i++;
                if (i < str.Length)
                    res += strUpper[i];
            }
            else {
                res += str[i];
            }
        }

        return res;
    }

    static string MergeWithSlash(params string[] strings)
    {
        string result = "";
        foreach (string str in strings) {
            if (IsStringEmpty(str, true) || str == "/" || str == "//") {
                continue;
            }
            result += str.StartsWith("/") ? str.Remove(0, 1) : str;

            if (!str.EndsWith("/")) {
                result += "/";
            }
        }

        // Remove last "/" and return
        result = result.Remove(result.Length - 1, 1);
        return result;
    }

}
