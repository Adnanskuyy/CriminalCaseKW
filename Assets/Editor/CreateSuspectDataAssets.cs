#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CriminalCase2.Data;

public class CreateSuspectDataAssets : MonoBehaviour
{
    [MenuItem("CriminalCase2/Create Prototype Data")]
    public static void CreateAllData()
    {
        string suspectsPath = "Assets/Data/Suspects/";
        string levelPath = "Assets/Data/";

        AssetDatabase.CreateFolder("Assets", "Data");
        AssetDatabase.CreateFolder("Assets/Data", "Suspects");

        LoadSuspectSprites(out Texture2D manBlackTshirt, out Texture2D manDenimJacket, out Texture2D womanOrangeSweater, out Texture2D womanWhiteShirt, out Sprite background);

        SuspectData marcus = CreateSuspectData(
            suspectsPath + "Suspect_MarcusCole.asset",
            "Marcus Cole",
            manBlackTshirt,
            "Frequently seen around the area. Known to have erratic behavior and often appears disoriented during late hours.",
            "Found with suspicious paraphernalia in his vehicle.",
            null,
            DrugTestResult.Positive,
            SuspectRole.User,
            "Marcus was indeed struggling with substance abuse. Good detective work.",
            "Marcus was actually a drug user. The signs were there - erratic behavior and disorientation."
        );

        SuspectData derek = CreateSuspectData(
            suspectsPath + "Suspect_DerekStone.asset",
            "Derek Stone",
            manDenimJacket,
            "Runs a local business. Well-known in the community with many connections and frequent visitors.",
            "Large amounts of cash found during routine traffic stop.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Dealer,
            "Derek was running an operation right under everyone's nose. Excellent work.",
            "Derek was the dealer. His clean test was a red herring - dealers don't always use."
        );

        SuspectData sarah = CreateSuspectData(
            suspectsPath + "Suspect_SarahChen.asset",
            "Sarah Chen",
            womanOrangeSweater,
            "Local teacher. Active in community events and well-respected by neighbors.",
            "No suspicious activity found. Clean background check.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Normal,
            "Sarah was innocent. Sometimes the obvious choice is the right one.",
            "Sarah was a normal citizen. Her community involvement wasn't a cover - she was genuinely clean."
        );

        SuspectData emily = CreateSuspectData(
            suspectsPath + "Suspect_EmilyRoss.asset",
            "Emily Ross",
            womanWhiteShirt,
            "Works at the local cafe. Recently moved to the area and still getting to know people.",
            "No evidence linking to any illegal activity.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Normal,
            "Emily was completely innocent. Good call trusting the evidence.",
            "Emily was innocent. Being new to town doesn't make someone suspicious."
        );

        LevelConfig levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
        AssetDatabase.CreateAsset(levelConfig, levelPath + "Level_01.asset");

        SerializedObject levelSO = new SerializedObject(levelConfig);
        levelSO.FindProperty("_levelIndex").intValue = 1;
        levelSO.FindProperty("_levelName").stringValue = "Level 01 - The Street Corner";
        levelSO.FindProperty("_backgroundSprite").objectReferenceValue = background;
        levelSO.FindProperty("_maxDrugTestsPerLevel").intValue = 2;

        SerializedArrayHelper.SetObjectArray(levelSO, "_suspects", new Object[] { marcus, derek, sarah, emily });

        levelSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(levelConfig);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CreatePrototypeData] All SuspectData and LevelConfig assets created successfully!");
    }

    private static SuspectData CreateSuspectData(string path, string name, Texture2D portrait, string description, string evidenceText, Texture2D evidenceImage, DrugTestResult drugTestResult, SuspectRole correctRole, string feedbackCorrect, string feedbackWrong)
    {
        SuspectData suspect = ScriptableObject.CreateInstance<SuspectData>();
        AssetDatabase.CreateAsset(suspect, path);

        SerializedObject so = new SerializedObject(suspect);
        so.FindProperty("_suspectName").stringValue = name;
        so.FindProperty("_portrait").objectReferenceValue = portrait;
        so.FindProperty("_description").stringValue = description;
        so.FindProperty("_evidenceText").stringValue = evidenceText;
        so.FindProperty("_evidenceImage").objectReferenceValue = evidenceImage;
        so.FindProperty("_drugTestResult").enumValueIndex = (int)drugTestResult;
        so.FindProperty("_correctRole").enumValueIndex = (int)correctRole;
        so.FindProperty("_feedbackTextCorrect").stringValue = feedbackCorrect;
        so.FindProperty("_feedbackTextWrong").stringValue = feedbackWrong;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(suspect);
        Debug.Log($"[CreatePrototypeData] Created: {name}");
        return suspect;
    }

    private static void LoadSuspectSprites(out Texture2D manBlackTshirt, out Texture2D manDenimJacket, out Texture2D womanOrangeSweater, out Texture2D womanWhiteShirt, out Sprite background)
    {
        manBlackTshirt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Level_01/Man_BlackTshirt.png");
        manDenimJacket = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Level_01/Man_DenimJacket.png");
        womanOrangeSweater = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Level_01/Woman_OrangeSweater.png");
        womanWhiteShirt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Level_01/Woman_WhiteShirt.png");

        string[] bgGuids = AssetDatabase.FindAssets("Background_Level_01 t:Sprite");
        background = null;
        if (bgGuids.Length > 0)
        {
            string bgPath = AssetDatabase.GUIDToAssetPath(bgGuids[0]);
            background = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        }

        if (background == null)
        {
            Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Level_01/Background_Level_01.png");
            if (bgTex != null)
            {
                background = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}

public static class SerializedArrayHelper
{
    public static void SetObjectArray(SerializedObject so, string propertyName, Object[] values)
    {
        SerializedProperty arrayProp = so.FindProperty(propertyName);
        arrayProp.ClearArray();
        arrayProp.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
