using UnityEngine;
using UnityEditor;
using CriminalCase2.Data;

namespace CriminalCase2.Editor
{
    public static class CreateClueDataAssets
    {
        [MenuItem("CriminalCase2/Create Clue Data Assets")]
        public static void Create()
        {
            string folder = "Assets/Data/Clues";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Clues");
            }

            CreateClue("Clue_Cigarette", "Rokok Bekas", "Sebuah puntung rokok yang ditemukan di lokasi kejadian. Rokok ini mungkin meninggalkan jejak DNA.", 0, false, folder);
            CreateClue("Clue_CoffeeCup", "Gelas Kopi", "Gelas kopi setengah diminum yang ditemukan di meja. Mungkin mengandung sidik jari.", 1, false, folder);
            CreateClue("Clue_DisposablePhone", "Telepon Sekali Pakai", "Sebuah telepon sekali pakai yang berisi pesan-pesan mencurigakan.", 2, false, folder);
            CreateClue("Clue_Receipt", "Struk Belanja", "Struk pembelian dari toko yang mencurigakan, ditemukan tersembunyi di bawah meja.", 3, false, folder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void CreateClue(string fileName, string clueName, string desc, int suspectIndex, bool isDrugTest, string folder)
        {
            var clue = ScriptableObject.CreateInstance<ClueData>();

            var so = new SerializedObject(clue);
            so.FindProperty("_clueName").stringValue = clueName;
            so.FindProperty("_description").stringValue = desc;
            so.FindProperty("_linkedSuspectIndex").intValue = suspectIndex;
            so.FindProperty("_isDrugTestClue").boolValue = isDrugTest;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(clue, $"{folder}/{fileName}.asset");
        }
    }
}