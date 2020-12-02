using UnityEngine;
using UnityEditor;
using System;

public class BsStandardEditor : ShaderGUI
{
    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    static GUIContent staticLabel = new GUIContent();

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;

        GUILayout.Label("Main", EditorStyles.boldLabel);
        DoMain();
        DoGlow();

        GUILayout.Label("Material Properties", EditorStyles.boldLabel);
        DoNormals();
        DoMetallic();
        DoSmoothness();
        
        GUILayout.Label("Lighting Parameters (Advanced)", EditorStyles.boldLabel);
        DoLighting();
    }

    void DoMain()
    {
        MaterialProperty mainTex = FindProperty("_MainTex");
        editor.TexturePropertySingleLine(
            MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Color")
        );
        if(mainTex.textureValue) editor.TextureScaleOffsetProperty(mainTex);
    }

    void DoMetallic()
    {
        EditorGUI.BeginChangeCheck();
        MaterialProperty metalMap = FindProperty("_MetalTex");
        editor.TexturePropertySingleLine(MakeLabel(metalMap, "Metallic (R)"), FindProperty("_MetalTex"), FindProperty("_Metallic"));
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_METALLIC_MAP", metalMap.textureValue);
        }
    }

    void DoSmoothness()
    {
        MaterialProperty slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider, "Smoothness (A)"));
        EditorGUI.indentLevel -= 2;
    }

    void DoGlow()
    {
        MaterialProperty map = FindProperty("_GlowTex");
        EditorGUI.BeginChangeCheck();

        bool glow = Array.IndexOf(target.shaderKeywords, "_GLOW") != -1;
        glow = EditorGUILayout.Toggle("Glow", glow);

        // only show the map if we need it
        editor.TexturePropertySingleLine(MakeLabel(map, "Glow (G)"), map);

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_GLOW", glow);
        }
    }

    void DoNormals()
    {
        MaterialProperty normalTex = FindProperty("_NormalTex");
        editor.TexturePropertySingleLine(
            MakeLabel(normalTex, "Bump Map (RGB)"), normalTex,
            normalTex.textureValue ? FindProperty("_BumpScale") : null
        );
    }

    void DoLighting()
    {
        MaterialProperty slider;
        slider = FindProperty("_LightDir");
        editor.ShaderProperty(slider, MakeLabel(slider, "default: (0, -1, -2, 0"));

        slider = FindProperty("_LightIntensity");
        editor.ShaderProperty(slider, MakeLabel(slider, "default: 1"));

        slider = FindProperty("_Ambient");
        editor.ShaderProperty(slider, MakeLabel(slider, "default: 0.5"));

        slider = FindProperty("_Scale");
        editor.ShaderProperty(slider, MakeLabel(slider));

        slider = FindProperty("_Power");
        editor.ShaderProperty(slider, MakeLabel(slider));
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }
    
    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            target.EnableKeyword(keyword);
        }
        else
        {
            target.DisableKeyword(keyword);
        }
    }
}