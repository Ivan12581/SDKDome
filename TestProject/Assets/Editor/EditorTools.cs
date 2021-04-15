﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
public static class EditorTools
{
    static Texture2D mBackdropTex;
    static Texture2D mContrastTex;
    static Texture2D mGradientTex;
    static GameObject mPrevious;

    /// <summary>
    /// Returns a blank usable 1x1 white texture.
    /// </summary>

    static public Texture2D blankTexture
    {
        get
        {
            return EditorGUIUtility.whiteTexture;
        }
    }
    static public Rect ConvertToTexCoords(Rect rect, int width, int height)
    {
        Rect final = rect;

        if (width != 0f && height != 0f)
        {
            final.xMin = rect.xMin / width;
            final.xMax = rect.xMax / width;
            final.yMin = 1f - rect.yMax / height;
            final.yMax = 1f - rect.yMin / height;
        }
        return final;
    }
    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text) { return DrawHeader(text, text, false, false); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, string key) { return DrawHeader(text, key, false, false); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, bool detailed) { return DrawHeader(text, text, detailed, !detailed); }

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    static public void DrawPadding()
    {
        GUILayout.Space(18f);
    }
    /// <summary>
    /// Begin drawing the content area.
    /// </summary>

    static public void BeginContents() { BeginContents(false); }

    static bool mEndHorizontal = false;

    /// <summary>
    /// Begin drawing the content area.
    /// </summary>

    static public void BeginContents(bool minimalistic)
    {
        if (!minimalistic)
        {
            mEndHorizontal = true;
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        }
        else
        {
            mEndHorizontal = false;
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(10f);
        }
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }


    /// <summary>
    /// End drawing the content area.
    /// </summary>

    static public void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (mEndHorizontal)
        {
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(3f);
    }

    /// <summary>
    /// Returns a usable texture that looks like a dark checker board.
    /// </summary>

    static public Texture2D backdropTexture
    {
        get
        {
            if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
                new Color(0.1f, 0.1f, 0.1f, 0.5f),
                new Color(0.2f, 0.2f, 0.2f, 0.5f));
            return mBackdropTex;
        }
    }

    /// <summary>
    /// Returns a usable texture that looks like a high-contrast checker board.
    /// </summary>

    static public Texture2D contrastTexture
    {
        get
        {
            if (mContrastTex == null) mContrastTex = CreateCheckerTex(
                new Color(0f, 0.0f, 0f, 0.5f),
                new Color(1f, 1f, 1f, 0.5f));
            return mContrastTex;
        }
    }

    /// <summary>
    /// Gradient texture is used for title bars / headers.
    /// </summary>

    static public Texture2D gradientTexture
    {
        get
        {
            if (mGradientTex == null) mGradientTex = CreateGradientTex();
            return mGradientTex;
        }
    }

    /// <summary>
    /// Create a white dummy texture.
    /// </summary>

    static Texture2D CreateDummyTex()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.name = "[Generated] Dummy Texture";
        tex.hideFlags = HideFlags.DontSave;
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Create a checker-background texture
    /// </summary>

    static Texture2D CreateCheckerTex(Color c0, Color c1)
    {
        Texture2D tex = new Texture2D(16, 16);
        tex.name = "[Generated] Checker Texture";
        tex.hideFlags = HideFlags.DontSave;

        for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
        for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
        for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
        for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    /// <summary>
    /// Create a gradient texture
    /// </summary>

    static Texture2D CreateGradientTex()
    {
        Texture2D tex = new Texture2D(1, 16);
        tex.name = "[Generated] Gradient Texture";
        tex.hideFlags = HideFlags.DontSave;

        Color c0 = new Color(1f, 1f, 1f, 0f);
        Color c1 = new Color(1f, 1f, 1f, 0.4f);

        for (int i = 0; i < 16; ++i)
        {
            float f = Mathf.Abs((i / 15f) * 2f - 1f);
            f *= f;
            tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    /// <summary>
    /// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
    /// </summary>

    static public void DrawTiledTexture(Rect rect, Texture tex)
    {
        GUI.BeginGroup(rect);
        {
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            for (int y = 0; y < height; y += tex.height)
            {
                for (int x = 0; x < width; x += tex.width)
                {
                    GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                }
            }
        }
        GUI.EndGroup();
    }

    /// <summary>
    /// Draw a single-pixel outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = contrastTexture;
            GUI.color = Color.white;
            DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
            DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
            DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
            DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
        }
    }

    /// <summary>
    /// Draw a single-pixel outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = blankTexture;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Draw a selection outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect relative, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            // Calculate where the outer rectangle would be
            float x = rect.xMin + rect.width * relative.xMin;
            float y = rect.yMax - rect.height * relative.yMin;
            float width = rect.width * relative.width;
            float height = -rect.height * relative.height;
            relative = new Rect(x, y, width, height);

            // Draw the selection
            DrawOutline(relative, color);
        }
    }

    /// <summary>
    /// Draw a selection outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect relative)
    {
        if (Event.current.type == EventType.Repaint)
        {
            // Calculate where the outer rectangle would be
            float x = rect.xMin + rect.width * relative.xMin;
            float y = rect.yMax - rect.height * relative.yMin;
            float width = rect.width * relative.width;
            float height = -rect.height * relative.height;
            relative = new Rect(x, y, width, height);

            // Draw the selection
            DrawOutline(relative);
        }
    }

    /// <summary>
    /// Draw a 9-sliced outline.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect outer, Rect inner)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Color green = new Color(0.4f, 1f, 0f, 1f);

            DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
            DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
            DrawOutline(rect, outer, green);
        }
    }

    /// <summary>
    /// Draw a checkered background for the specified texture.
    /// </summary>

    static public Rect DrawBackground(Texture2D tex, float ratio)
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 0f);
        rect.width = Screen.width - rect.xMin;
        rect.height = rect.width * ratio;
        GUILayout.Space(rect.height);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D blank = blankTexture;
            Texture2D check = backdropTexture;

            // Lines above and below the texture rectangle
            GUI.color = new Color(0f, 0f, 0f, 0.2f);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
            GUI.color = Color.white;

            // Checker background
            DrawTiledTexture(rect, check);
        }
        return rect;
    }

    /// <summary>
    /// Draw a visible separator in addition to adding some padding.
    /// </summary>

    static public void DrawSeparator()
    {
        GUILayout.Space(12f);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = blankTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string DrawList(string field, string[] list, string selection, params GUILayoutOption[] options)
    {
        if (list != null && list.Length > 0)
        {
            int index = 0;
            if (string.IsNullOrEmpty(selection)) selection = list[0];

            // We need to find the sprite in order to have it selected
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < list.Length; ++i)
                {
                    if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }

            // Draw the sprite selection popup
            index = string.IsNullOrEmpty(field) ?
                EditorGUILayout.Popup(index, list, options) :
                EditorGUILayout.Popup(field, index, list, options);

            return list[index];
        }
        return null;
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string DrawAdvancedList(string field, string[] list, string selection, params GUILayoutOption[] options)
    {
        if (list != null && list.Length > 0)
        {
            int index = 0;
            if (string.IsNullOrEmpty(selection)) selection = list[0];

            // We need to find the sprite in order to have it selected
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < list.Length; ++i)
                {
                    if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }

            // Draw the sprite selection popup
            index = string.IsNullOrEmpty(field) ?
                DrawPrefixList(index, list, options) :
                DrawPrefixList(field, index, list, options);

            return list[index];
        }
        return null;
    }




    /// <summary>
    /// Helper function that checks to see if this action would break the prefab connection.
    /// </summary>

    static public bool WillLosePrefab(GameObject root)
    {
        if (root == null) return false;

        if (root.transform != null)
        {
            // Check if the selected object is a prefab instance and display a warning
            PrefabType type = PrefabUtility.GetPrefabType(root);

            if (type == PrefabType.PrefabInstance)
            {
                return EditorUtility.DisplayDialog("Losing prefab",
                    "This action will lose the prefab connection. Are you sure you wish to continue?",
                    "Continue", "Cancel");
            }
        }
        return true;
    }


    /// <summary>
    /// Helper function that returns the folder where the current selection resides.
    /// </summary>

    static public string GetSelectionFolder()
    {
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

            if (!string.IsNullOrEmpty(path))
            {
                int dot = path.LastIndexOf('.');
                int slash = Mathf.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
                if (slash > 0) return (dot > slash) ? path.Substring(0, slash + 1) : path + "/";
            }
        }
        return "Assets/";
    }

    /// <summary>
    /// Struct type for the integer vector field below.
    /// </summary>

    public struct IntVector
    {
        public int x;
        public int y;
    }
    static public void SetLabelWidth(float width)
    {
        EditorGUIUtility.labelWidth = width;
    }
    /// <summary>
    /// Integer vector field.
    /// </summary>

    static public IntVector IntPair(string prefix, string leftCaption, string rightCaption, int x, int y)
    {
        GUILayout.BeginHorizontal();

        if (string.IsNullOrEmpty(prefix))
        {
            GUILayout.Space(82f);
        }
        else
        {
            GUILayout.Label(prefix, GUILayout.Width(74f));
        }

        SetLabelWidth(48f);

        IntVector retVal;
        retVal.x = EditorGUILayout.IntField(leftCaption, x, GUILayout.MinWidth(30f));
        retVal.y = EditorGUILayout.IntField(rightCaption, y, GUILayout.MinWidth(30f));

        SetLabelWidth(80f);

        GUILayout.EndHorizontal();
        return retVal;
    }

    /// <summary>
    /// Integer rectangle field.
    /// </summary>

    static public Rect IntRect(string prefix, Rect rect)
    {
        int left = Mathf.RoundToInt(rect.xMin);
        int top = Mathf.RoundToInt(rect.yMin);
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        IntVector a = IntPair(prefix, "Left", "Top", left, top);
        IntVector b = IntPair(null, "Width", "Height", width, height);

        return new Rect(a.x, a.y, b.x, b.y);
    }

    /// <summary>
    /// Integer vector field.
    /// </summary>

    static public Vector4 IntPadding(string prefix, Vector4 v)
    {
        int left = Mathf.RoundToInt(v.x);
        int top = Mathf.RoundToInt(v.y);
        int right = Mathf.RoundToInt(v.z);
        int bottom = Mathf.RoundToInt(v.w);

        IntVector a = IntPair(prefix, "Left", "Top", left, top);
        IntVector b = IntPair(null, "Right", "Bottom", right, bottom);

        return new Vector4(a.x, a.y, b.x, b.y);
    }

    /// <summary>
    /// Find all scene components, active or inactive.
    /// </summary>

    static public List<T> FindAll<T>() where T : Component
    {
        T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

        List<T> list = new List<T>();

        foreach (T comp in comps)
        {
            if (comp.gameObject.hideFlags == 0)
            {
                string path = AssetDatabase.GetAssetPath(comp.gameObject);
                if (string.IsNullOrEmpty(path)) list.Add(comp);
            }
        }
        return list;
    }

    static public bool DrawPrefixButton(string text)
    {
        return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
    }

    static public bool DrawPrefixButton(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, "DropDown", options);
    }

    static public int DrawPrefixList(int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(index, list, "DropDown", options);
    }

    static public int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(text, index, list, "DropDown", options);
    }



    /// <summary>
    /// Draw the specified sprite.
    /// </summary>

    public static void DrawTexture(Texture2D tex, Rect rect, Rect uv, Color color)
    {
        DrawTexture(tex, rect, uv, color, null);
    }

    /// <summary>
    /// Draw the specified sprite.
    /// </summary>

    public static void DrawTexture(Texture2D tex, Rect rect, Rect uv, Color color, Material mat)
    {
        int w = Mathf.RoundToInt(tex.width * uv.width);
        int h = Mathf.RoundToInt(tex.height * uv.height);

        // Create the texture rectangle that is centered inside rect.
        Rect outerRect = rect;
        outerRect.width = w;
        outerRect.height = h;

        if (outerRect.width > 0f)
        {
            float f = rect.width / outerRect.width;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (rect.height > outerRect.height)
        {
            outerRect.y += (rect.height - outerRect.height) * 0.5f;
        }
        else if (outerRect.height > rect.height)
        {
            float f = rect.height / outerRect.height;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (rect.width > outerRect.width) outerRect.x += (rect.width - outerRect.width) * 0.5f;

        // Draw the background
        DrawTiledTexture(outerRect, backdropTexture);

        // Draw the sprite
        GUI.color = color;

        if (mat == null)
        {
            GUI.DrawTextureWithTexCoords(outerRect, tex, uv, true);
        }
        else
        {
            // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
            // using BeginGroup/EndGroup, and there is no way to specify a UV rect... le'suq.
            UnityEditor.EditorGUI.DrawPreviewTexture(outerRect, tex, mat);
        }
        GUI.color = Color.white;

        // Draw the lines around the sprite
        Handles.color = Color.black;
        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMin, outerRect.yMax));
        Handles.DrawLine(new Vector3(outerRect.xMax, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMax));
        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMin));
        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMax), new Vector3(outerRect.xMax, outerRect.yMax));

        // Sprite size label
        string text = string.Format("Texture Size: {0}x{1}", w, h);
        EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), text);
    }
    static public void SetDirty(Object obj)
    {
#if UNITY_EDITOR
        if (obj)
        {
            UnityEditor.EditorUtility.SetDirty(obj);
        }
#endif
    }

    static public bool WriteFileWithCode(string filepath, string data, Encoding code)
    {
        try
        {
            string path = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (code != null)
            {
                File.WriteAllText(filepath, data, code);
            }
            else
            {
                File.WriteAllText(filepath, data);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("writeFIle fail. " + filepath);
            throw e;
        }
    }

    static public void SaveObjectToJsonFile<T>(T data, string path)
    {
        string jsonStr = LitJson.JsonMapper.ToJson(data);
        WriteFileWithCode(path, jsonStr, null);
    }

    static public T LoadObjectFromJsonFile<T>(string path) where T : new()
    {
        if (!File.Exists(path))
            return new T();
        string str = File.ReadAllText(path);
        if (string.IsNullOrEmpty(str))
        {
            Debug.Log("Cannot find " + path);
            return new T();
        }
        T data = LitJson.JsonMapper.ToObject<T>(str);
        if (data == null)
        {
            Debug.Log("Cannot read data from " + path);
        }

        return data;
    }
}
