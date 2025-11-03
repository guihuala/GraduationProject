using System;
using UnityEngine;

namespace GuiFramework
{
    public class ItemRefObj : SCRefDataCore
    {
        public static string assetPath = "Assets/Resources/RefData/ExportJson";
        public static string sheetName = "items";

        public string ID { get; private set; }
        public string Name { get; private set; }
        public int Price { get; private set; }
        public string Type { get; private set; }

        protected override void _parseFromString()
        {
            // 在这个示例中，数据已通过 JSON 自动解析，直接将值设置
            ID = getString("ID");
            Name = getString("Name");
            Price = getInt("Price");
            Type = getString("Type");
        }
    }
}