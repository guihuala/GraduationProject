using System.Collections.Generic;
using UnityEngine;


    [CreateAssetMenu(fileName = "New EnemyBaseData", menuName = "JunkGameSO/Enemy/EnemyBaseData")]
    public class EnemyBaseData : ScriptableObject
    {
        public string EnemyId;
        public string EnemyName;
        public float MaxSanity = 100f;

        public float MoveSpeed = 2.0f;
        public float ChaseSpeed = 3.0f;
        public float Resistance = 0.5f;

        public float VisionRange = 8f;
        public float VisionAngle = 120f;

        public float AttackRange = 0.5f;
    }
