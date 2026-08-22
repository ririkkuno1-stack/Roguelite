using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Core.MasterData;
using TPSRoguelite.InGame.Player;
using System;

namespace TPSRoguelite.InGame.Manager
{
    [Serializable]
    public class SkillButtonUI
    {
        public Button button;
        public TextMeshProUGUI nameToxt;
        public TextMeshProUGUI dectTxt;
    }

    public class LevelUpManager : MonoBehaviour
    {

        public static LevelUpManager Instance { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void OnLevelUp(Playerinputactions currentInput, PlayerController player)
        {
            
        }

        private void OnSkillSeleoted(SkillDataRecord SelectedSkill)
        {
            
        }
    }
}
