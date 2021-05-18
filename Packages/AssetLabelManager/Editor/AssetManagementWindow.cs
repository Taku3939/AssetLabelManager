using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetManagement
{
    class AssetManagementWindow : EditorWindow
    {
        private const string path = "./Assets/AssetLabelManager/settings.json";　//configファイルのパス
        private TreeViewState _treeViewState; 
        private AssetManagementView _treeView;
        private SearchField _searchField;

        [MenuItem("Window/AssetManagementWindow")]
        private static void Open() => GetWindow<AssetManagementWindow>(ObjectNames.NicifyVariableName(nameof(AssetManagementWindow)));
        private void OnEnable() => this.CreateTreeView();
        
        private void CreateTreeView()
        {
            _treeViewState ??= new TreeViewState();
            
            var nameColumn = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 350,
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = false
            };

            var iconColumn = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType"), "Asset type"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 40,
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = false
            };

            var descriptionColumn = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent("Description"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 200,
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = false
            };

            var headerState = new MultiColumnHeaderState(new[] {nameColumn, iconColumn, descriptionColumn});
            var multiColumnHeader = new MultiColumnHeader(headerState);
            // カラムヘッダーとともにTreeViewを作成
            _treeView = new AssetManagementView(_treeViewState, multiColumnHeader);
            
            //Assetのpathの取得
            string str = "";
            
            if (!File.Exists(path))
            {
                var fs = File.Create(path);
                using (StreamWriter sr = new StreamWriter(fs)) {sr.WriteAsync("{\"dir\": \"Assets\"}"); }
            }
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (StreamReader sr = new StreamReader(fs)) { str = sr.ReadToEnd(); }
            var settings = JsonUtility.FromJson<Settings>(str);
            if (settings.dir == null) return;
            
            //取得
            var l = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith(settings.dir)).ToList();
            var prefab = l.Where(p => p.EndsWith(".prefab"));
            var mat = l.Where(p => p.EndsWith(".mat"));
            var matRoot = BuildRoot("Material", mat.ToArray());
            var prefabRoot = BuildRoot("Prefab", prefab.ToArray());
            
            // TreeViewを初期化
            _treeView.Setup(new List<AssetElement> {prefabRoot, matRoot}.ToArray());

            // SearchFieldを初期化
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }
        private int Id;
        AssetElement BuildRoot(string header, string[] elements)
        {
            var root = new AssetElement() {Id = Id++, Name = header, IconPath = ""};
            
            foreach (var path in elements)
            {
                var element = new AssetElement {Id = Id++, Name = path, IconPath = path};
                root.AddChild(element);
            }
            
            return root;
        }

        private void OnGUI()
        {
            // 検索窓を描画
            using (var s = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
                _treeView.searchString = _searchField.OnToolbarGUI(_treeView.searchString);
            }

            DrawHeader();
            // TreeViewを描画

            var rect = EditorGUILayout.GetControlRect(false, this.position.height);
            _treeView.OnGUI(rect);
        }

        /// <summary>
        /// ヘッダーの描画
        /// </summary>
        private void DrawHeader()
        {
            if (_treeView == null) return;
            var defaultColor = GUI.backgroundColor;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
                    OnReload();
                
                GUI.backgroundColor = defaultColor;
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
            }

            GUI.backgroundColor = defaultColor;
        }
        
        /// <summary>
        /// リロードしたときに呼ばれる
        /// </summary>
        private void OnReload()
        {
            this.CreateTreeView();
        }
    }

    [Serializable]
    public class Settings
    {
        public string dir;
    }
}