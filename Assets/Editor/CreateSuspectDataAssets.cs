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
            "Sering terlihat di sekitar area ini. Dikenal memiliki perilaku tidak menentu dan sering tampak bingung di malam hari.",
            "Ditemukan alat-alat mencurigakan di kendaraannya.",
            null,
            DrugTestResult.Positive,
            SuspectRole.User,
            "Marcus memang sedang berjuang melawan penyalahgunaan zat bosan. Kerja detektif yang bagus.",
            "Marcus sebenarnya seorang pecandu narkoba. Tanda-tandanya sudah ada - perilaku tidak menentu dan kebingungan."
        );

        SuspectData derek = CreateSuspectData(
            suspectsPath + "Suspect_DerekStone.asset",
            "Derek Stone",
            manDenimJacket,
            "Menjalankan bisnis lokal. Dikenal di komunitas dengan banyak koneksi dan pengunjung rutin.",
            "Jumlah uang tunai besar ditemukan saat pemeriksaan lalu lintas rutin.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Dealer,
            "Derek menjalankan operasi tepat di bawah hidung semua orang. Kerja luar biasa.",
            "Derek adalah bandar narkobanya. Tes bersihnya hanyalah pengecoh - bandar narkoba tidak selalu mengonsumsi."
        );

        SuspectData sarah = CreateSuspectData(
            suspectsPath + "Suspect_SarahChen.asset",
            "Sarah Chen",
            womanOrangeSweater,
            "Guru lokal. Aktif di acara komunitas dan dihormati oleh tetangga.",
            "Tidak ditemukan aktivitas mencurigakan. Pemeriksaan latar belakang bersih.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Normal,
            "Sarah tidak bersalah. Terkadang pilihan yang jelas adalah yang benar.",
            "Sarah adalah warga biasa. Keterlibatannya di komunitas bukan kedok - dia benar-benar bersih."
        );

        SuspectData emily = CreateSuspectData(
            suspectsPath + "Suspect_EmilyRoss.asset",
            "Emily Ross",
            womanWhiteShirt,
            "Bekerja di kafe lokal. Baru pindah ke daerah ini dan masih mengenal orang-orang.",
            "Tidak ada bukti yang menghubungkan dengan aktivitas ilegal.",
            null,
            DrugTestResult.Negative,
            SuspectRole.Normal,
            "Emily benar-benar tidak bersalah. Keputusan bagus mempercayai buktinya.",
            "Emily tidak bersalah. Baru pindah ke kota tidak membuat seseorang mencurigakan."
        );

        LevelConfig levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
        AssetDatabase.CreateAsset(levelConfig, levelPath + "Level_01.asset");

        SerializedObject levelSO = new SerializedObject(levelConfig);
        levelSO.FindProperty("_levelIndex").intValue = 1;
        levelSO.FindProperty("_levelName").stringValue = "Level 01 - Sudut Jalan";
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