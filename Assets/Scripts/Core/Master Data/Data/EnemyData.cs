using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Core.MasterData
{
    [Serializable]
    public class EnemyDataRecord : IMsterData
    {
        [field: SerializeField] public ulong Id { get; private set; }

        /// <summary>
        /// ìGÇÃñºëO
        /// </summary>
        [field: SerializeField] public string EnemyName { get; private set; }

        /// <summary>
        /// ç≈ëÂHP
        /// </summary>
        [field: SerializeField] public int MaxHp { get; private set; }

        /// <summary>
        /// à⁄ìÆë¨ìx
        /// </summary>
        [field: SerializeField] public float MoveSpeed { get; private set; }


    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Sriptable Objct/Enemy/Data")]
    public class EnemyData : ScriptableObject, MasterDataContainer<EnemyDataRecord>
    {
        [field: SerializeField] public List<EnemyDataRecord> Records { get; private set; }
    }
}
