using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace AssetManagement
{
    public class AssetElement : TreeViewItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string IconPath { get; set; }

        // public string Description { get; set; }
        public int value { get; set; }
        public AssetElement Parent { get; private set; }
        private List<AssetElement> _children = new List<AssetElement>();

        public List<AssetElement> Children => _children;

        /// <summary>
        /// 子を追加する
        /// </summary>
        public void AddChild(AssetElement child)
        {
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }

            Children.Add(child);
            child.Parent = this;
        }

        /// <summary>
        /// 子を削除する
        /// </summary>
        private void RemoveChild(AssetElement child)
        {
            if (!Children.Contains(child)) return;
            Children.Remove(child);
            child.Parent = null;
        }
    }
}