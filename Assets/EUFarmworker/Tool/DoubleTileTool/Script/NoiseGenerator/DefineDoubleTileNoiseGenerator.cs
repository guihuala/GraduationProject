using EUFarmworker.Tool.DoubleTileTool.Script.NoiseGenerator;
using UnityEngine;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    [CreateAssetMenu(fileName = "DefineDoubleTileNoiseGeneratorData", menuName = "EUTool/DoubleTile/DoubleTileNoiseGenerator/DefineDoubleTileNoiseGeneratorData")]
    public class DefineDoubleTileNoiseGenerator: DoubleTileNoiseGeneratorBase
    {
        private float _seed;
        [SerializeField]
        private float _scale = 0.1f;
        public override float OnGeneratorTile(int x, int y, int z)
        {
            return Mathf.PerlinNoise((x + _seed) * _scale, (y +_seed) * _scale);
        }

        public override void Init(int seed)
        {
            _seed = seed;
        }
    }
}
