using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

namespace RustedCode.Scripts.Editor.AssetManagement
{
    public class AssetManagementView : TreeView
    {
        /// <summary>
        /// This dictionary reference is https://github.com/halak/unity-editor-icons
        /// </summary>
        private readonly Dictionary<string, string> iconDictionary = new Dictionary<string, string>()
        {
            {".scene", "SceneAsset On Icon"},
            {".asset", "ScriptableObject Icon"},
            {".mat", "Material Icon"},
            {".prefab", "PrefabModel Icon"},
            {".tex", "Texture Icon"},
            {".cs", "cs Script Icon"},
            {".js", "Js Script Icon"},
            {".asmdef", "AssemblyDefinitionAsset Icon"},
            {".txt", "TextScriptImporter Icon"},
            // {"", "DefaultAsset Icon"},
        };


        class ExampleTreeViewItem : TreeViewItem
        {
            public AssetElement Data { get; set; }
        }

        private AssetElement[] _baseElements;

        // 初期化時にMultiColumnHeaderを渡す
        public AssetManagementView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(
            treeViewState,
            multiColumnHeader)
        {
            multiColumnHeader.sortingChanged += header => { };
        }

        public void Setup(AssetElement[] baseElements)
        {
            _baseElements = baseElements;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            foreach (var baseElement in _baseElements)
            {
                var baseItem = CreateTreeViewItem(baseElement);
                root.AddChild(baseItem);
                rows.Add(baseItem);
                if (baseElement.Children.Count >= 1)
                {
                    if (IsExpanded(baseItem.id))
                    {
                        AddChildrenRecursive(baseElement, baseItem, rows);
                    }
                    else
                    {
                        baseItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }

            SetupDepthsFromParentsAndChildren(root);

            return rows;
        }

        private void AddChildrenRecursive(AssetElement element, TreeViewItem item, IList<TreeViewItem> rows)
        {
            foreach (var childElement in element.Children)
            {
                var childItem = CreateTreeViewItem(childElement);
                item.AddChild(childItem);
                rows.Add(childItem);
                if (childElement.Children.Count >= 1)
                {
                    if (IsExpanded(childElement.Id))
                    {
                        AddChildrenRecursive(childElement, childItem, rows);
                    }
                    else
                    {
                        childItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        private ExampleTreeViewItem CreateTreeViewItem(AssetElement model)
            => new ExampleTreeViewItem {id = model.Id, displayName = model.Name, Data = model};


        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (ExampleTreeViewItem) args.item;

            // 表示されているカラム毎に処理
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                // いまのセルのRect
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);

                // セルのRectを上下センタリングするユーティリティメソッド（不要なら使わなくていい）
                CenterRectUsingSingleLineHeight(ref cellRect);

                if (columnIndex == 0)
                {
                    // デフォルトのセル描画
                    base.RowGUI(args);
                }
                else if (columnIndex == 1)
                {
                    //Iconの描画
                    var content = GetIcon(item.Data.Name);
                    GUI.Box(cellRect, content, new GUIStyle {alignment = TextAnchor.MiddleCenter});
                }
                else if (columnIndex == 2)
                {
                    // テキストフィールド（モデルを書き換える）
                    if (string.IsNullOrEmpty(item.Data.IconPath)) return;

                    // ゲームオブジェクトのロード
                    var gameObject = AssetDatabase.LoadAssetAtPath<Object>(item.Data.IconPath);

                    //  現在のラベルを取得する
                    item.Data.value = GetValue(gameObject, options);

                    // ラベル取得用のフィールドの作成
                    var val = EditorGUI.MaskField(cellRect, item.Data.value, options);

                    // ラベルの作成
                    var option = CreateOption(val);

                    // ラベルの登録
                    AssetDatabase.SetLabels(gameObject, option);
                }
            }
        }

        private readonly string[] options = {"Material", "UI", "prefab"};


        /// <summary>
        /// Optionの作成
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string[] CreateOption(int value)
        {
            var dist = new List<string>();
            
            // Everything
            if (value == -1) return options;

            // None
            if (value == 0) return dist.ToArray();

            dist.AddRange(options.Where((t, i) => (value & 1 << i) != 0));
            return dist.ToArray();
        }

        /// <summary>
        /// ラベルに基づいた値の取得
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static int GetValue(Object gameObject, string[] options)
        {
            int v = 0;
            var labels = AssetDatabase.GetLabels(gameObject);

            if (labels.Length == 0) return 0;


            int count = 0;
            foreach (var t in labels)
                for (var j = 0; j < options.Length; j++)
                {
                    //同じラベルの確認
                    if (t != options[j]) continue;
                    count++;
                    v += 1 << j;
                }


            if (count == options.Length) return -1;

            return v;
        }

        /// <summary>
        /// Iconの作成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private GUIContent GetIcon(string path)
        {
            if (!iconDictionary.TryGetValue(Path.GetExtension(path), out var value))
            {
                return GUIContent.none;
            }

            var tex = EditorGUIUtility.IconContent(value);
            if (tex == null) return GUIContent.none;
            return tex;
        }

        static Texture2D GetPrefabPreview(string path, int TextureSize)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            var editor = UnityEditor.Editor.CreateEditor(asset);
            Texture2D tex = editor.RenderStaticPreview(path, null, TextureSize, TextureSize);
            GameObject.DestroyImmediate(editor);
            return tex;
        }
    }
}