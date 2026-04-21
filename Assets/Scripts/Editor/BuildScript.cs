using UnityEditor;
using System.IO;

namespace CriminalCase2.Editor
{
    public static class BuildScript
    {
        [MenuItem("Build/Build WebGL")]
        public static void BuildWebGL()
        {
            string buildPath = "build/WebGL";
            
            // Ensure build directory exists
            Directory.CreateDirectory(buildPath);
            
            string[] scenes = new string[]
            {
                "Assets/Scenes/SampleScene.unity"
            };
            
            BuildPipeline.BuildPlayer(
                scenes,
                buildPath,
                BuildTarget.WebGL,
                BuildOptions.None
            );
        }
    }
}
