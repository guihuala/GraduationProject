using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.Script.Data
{
    [Serializable]
    public class DoubleTileData
    {
        public string TileName;//瓦片名称
        public int FrameRate = 1;//帧率
        public Sprite TagTexture;//用于标记的瓦片纹理
        public List<List<Object>> SpriteList = new();//动态瓦片每帧的纹理
        public TileObjectType TileObjectType = TileObjectType.Sprite;
    }

    public enum TileObjectType
    {
        Sprite,
        GameObject,
    }
}