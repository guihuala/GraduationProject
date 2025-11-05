using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script.NoiseGenerator;
using UnityEngine;

namespace EUFarmworker.Tool.DoubleTileTool.Script.Data
{
    [CreateAssetMenu(fileName = "DoubleTileConfigData", menuName = "EUTool/DoubleTile/DoubleTileConfigData")]
    public class DoubleTileScriptableObject:ScriptableObject
    {
        public string ScriptPath;//脚本生成的路径
        public string TilePath;//瓦片生成的路径
        public DoubleTileNoiseGeneratorBase DoubleTileNoiseGenerator;//噪点生成算法
        public List<string> TileNames = new ();//瓦片名称设置
        public Dictionary<string,DoubleTileData> TileDatas = new();//瓦片数据
    }
}