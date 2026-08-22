using Core.MasterData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MasterData
{
    [Serializable]
    public class SkillDataRecord : IMsterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string SkillName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }

        [field: SerializeField] public int SkillType { get; private set; }
        [field: SerializeField] public float Value { get; private set; }


        
    }

    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjectl/SkillData")]
    public class SkillData : ScriptableObject, MasterDataContainer<SkillDataRecord>
    {
        [field: SerializeField] public List<SkillDataRecord> Records { get; private set; }
    }
}

