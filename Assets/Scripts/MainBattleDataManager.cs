using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.UI.Card
{
    public class MainBattleDataManager : MonoBehaviour
    {
        private static MainBattleDataManager instance;
        public EnemyType enemy;
        public int playerHp;
        public List<int> cardHeapBonusCards = new List<int>();
        public int battleResult = -1; // -1 for uninitalized, 0 for lost, 1 for combat win, 2 for fuel/heat win
        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
        }
    }
}