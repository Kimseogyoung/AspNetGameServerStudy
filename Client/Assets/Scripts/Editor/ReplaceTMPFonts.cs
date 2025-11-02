using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class TMPFontReplacerWindow : EditorWindow
{
    [Header("교체할 TMP 폰트 에셋")]
    public TMP_FontAsset newFont;

    [Header("선택 (미지정 시 newFont의 기본 머티리얼 적용)")]
    public Material overrideMaterial;

    [Header("옵션")]
    public bool includeInactive = true;

    public static void ShowWindow()
    {
        GetWindow<TMPFontReplacerWindow>("TMP Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshPro 폰트 일괄 교체기", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("신규 TMP_FontAsset", newFont, typeof(TMP_FontAsset), false);
        overrideMaterial = (Material)EditorGUILayout.ObjectField("머티리얼(선택)", overrideMaterial, typeof(Material), false);
        includeInactive = EditorGUILayout.ToggleLeft("비활성 오브젝트 포함", includeInactive);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(newFont == null))
        {
            if (GUILayout.Button("실행: 씬 + 프리팹 전체 교체"))
            {
                ReplaceAll();
            }
        }

        EditorGUILayout.Space();
    }

    private void ReplaceAll()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("오류", "신규 TMP_FontAsset을 지정해 주세요.", "확인");
            return;
        }

        var changedCount = 0;
        try
        {
            AssetDatabase.StartAssetEditing();

            // 1) 열려있는 씬 대상
            changedCount += ProcessOpenScenes();

            // 2) 모든 프리팹 에셋 대상
            changedCount += ProcessAllPrefabs();

            // 변경된 씬 저장(사용자 동의 없이 강제 저장은 하지 않음)
            // 여기서는 더티 표시만. 사용자가 저장할지 결정하도록 둠.
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[TMPFontReplacer] 완료: 총 {changedCount}개 컴포넌트 변경");
        EditorUtility.DisplayDialog("완료", $"교체 완료!\n변경된 컴포넌트: {changedCount}","확인");
    }

    private int ProcessOpenScenes()
    {
        int changed = 0;

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            LOG.I($"sceneName({scene.name}) Isloaded({scene.isLoaded})");
            if (!scene.isLoaded) continue;

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive);
                foreach (var t in tmps)
                {
                    changed += TryReplaceTMP(t, "Scene",
                        string.IsNullOrEmpty(scene.path) ? scene.name : scene.path,
                        GetHierarchyPath(t.transform));
                }
            }

            // 더티 표시 (저장은 유저가)
            if (changed > 0)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private int ProcessAllPrefabs()
    {
        int changed = 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            // PrefabContents로 열어서 안전하게 수정
            var prefabRoot = PrefabUtility.LoadPrefabContents(path);
            bool modified = false;

            try
            {
                var tmps = prefabRoot.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in tmps)
                {
                    int delta = TryReplaceTMP(t, "Prefab", path, GetHierarchyPath(t.transform));
                    if (delta > 0) modified = true;
                    changed += delta;
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            if (i % 200 == 0)
            {
                EditorUtility.DisplayProgressBar("프리팹 처리 중", $"{i}/{guids.Length} {path}", (float)i / guids.Length);
            }
        }

        EditorUtility.ClearProgressBar();
        return changed;
    }

    private int TryReplaceTMP(TMP_Text t, string target, string sceneOrAssetPath, string hierarchyPath)
    {
        if (t == null) return 0;

        var oldFont = t.font;
        var newMat = overrideMaterial != null ? overrideMaterial : newFont.material;
        var oldMat = t.fontSharedMaterial;

        bool fontChanged = oldFont != newFont;
        bool matChanged = oldMat != newMat;

        if (!fontChanged && !matChanged)
            return 0;

        Undo.RecordObject(t, "Replace TMP Font");

        if (fontChanged)
        {
            t.font = newFont;
        }
        if (matChanged)
        {
            // fontSharedMaterial을 교체 (Renderer의 material 교체 대신 TMP 권장 방식)
            t.fontSharedMaterial = newMat;
        }

        EditorUtility.SetDirty(t);

        LOG.I($"ReplaceFont Target({target}) SceneOrAssetPath({sceneOrAssetPath}) HierarchyPath({hierarchyPath}) Component({t.GetType().Name})" +
            $"OldFont({oldFont?.name}) NewFont({newFont?.name}) OldMat({oldMat?.name}) NewMat({newMat?.name})");
        return 1;
    }

    private static string GetHierarchyPath(Transform tr)
    {
        var stack = new Stack<string>();
        var cur = tr;
        while (cur != null)
        {
            stack.Push(cur.name);
            cur = cur.parent;
        }
        return string.Join("/", stack);
    }
}
