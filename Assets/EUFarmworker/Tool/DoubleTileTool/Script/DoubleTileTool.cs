using System.Collections.Generic;
using UnityEngine;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    public class DoubleTileTool
    {
        private readonly Dictionary<Vector3Int, TileType> _changeTileType = new();//改变的瓦片的类型信息
        public static void Init()//初始化瓦片工具
        {
        
        }
    
        public void LoadTile(Vector3 position)//加载瓦片
        {
        
        }

        public void RecycleTile(Vector3 position)//回收瓦片
        {
        
        }

        public void SetTile(Vector3 position,TileType tileType)//设置瓦片类型
        {
        
        }

        public Dictionary<Vector3Int, TileType> GetChangeTileType()//获取已经改变的瓦片类型信息
        {
            return _changeTileType;
        }

        // public string GetTileType(Vector3 position)//获取指定位置的瓦片类型信息
        // {
        //     先判断这个位置是否有改变,再去拿类型
        //     return 
        // }
    }
}
