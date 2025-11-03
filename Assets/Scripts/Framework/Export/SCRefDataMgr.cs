using GuiFramework.Utils;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// 配表数据管理器 在这里写所有的配表数据
    /// </summary>
    public class SCRefDataMgr : Singleton<SCRefDataMgr>
    {
        public SCRefDataList<ItemRefObj> itemRefList = new SCRefDataList<ItemRefObj>(ItemRefObj.assetPath, ItemRefObj.sheetName);

        public void Init()
        {
            itemRefList.readFromJson();
            Debug.Log("配表数据加载成功！");
            foreach (var item in itemRefList.refDataList)
            {
                Debug.Log($"ID: {item.ID}, Name: {item.Name}, Price: {item.Price}, Type: {item.Type}");
            }
        }
    }
}
