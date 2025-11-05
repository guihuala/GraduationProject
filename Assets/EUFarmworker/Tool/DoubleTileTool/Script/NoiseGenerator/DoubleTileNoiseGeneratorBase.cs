using UnityEngine;

namespace EUFarmworker.Tool.DoubleTileTool.Script.NoiseGenerator
{
    public abstract class DoubleTileNoiseGeneratorBase:ScriptableObject
    {
        /// <summary>
        /// 用于生成瓦片的方法
        /// </summary>
        /// <param name="x">当前生成位置的x坐标</param>
        /// <param name="y">当前生成位置的y坐标</param>
        /// <param name="z">当前生成位置的z坐标</param>
        /// <returns>返回的是一个当前位置的0~1之间的参数(地图噪声)</returns>
        public abstract float OnGeneratorTile(int x, int y, int z);

        /// <summary>
        /// 会在开始生成地图的时候执行一次
        /// </summary>
        public abstract void Init(int seed);
    }
}