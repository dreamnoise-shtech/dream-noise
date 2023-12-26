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