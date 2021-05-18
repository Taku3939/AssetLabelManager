using UnityEditor;

namespace RustedCode.Scripts.Editor.AssetManagement
{

    internal static class CustomUI
    {
        public static int RowCount { get; set; } = 1;

        public static void DisplayProgressLoadTexture()
        {
            EditorUtility.DisplayProgressBar("テクスチャ表示ウィンドウを初期化しています", "テクスチャ収集中", 0f);
        }
    }
}