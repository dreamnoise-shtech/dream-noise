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
        public List<GameObject> prefabs = new List<GameObject>();
        public List<string> cardNames = new List<string> { "梦征", "梦断", "梦刀", "梦击", "梦游", "梦寐", "梦网", "梦语", "梦撒", "梦饵", "梦魇之爪", "梦境之门", "梦幻之盾", "梦火之源", "梦想之书", "梦碎之刃", "梦醒之光", "梦断之刺", "梦圆之夜", "梦力之涌", "梦续之息", "梦始之印", "梦连之链", "梦醒之响", "梦断之韵", "炖食锅", "急救包", "药水", "梦帽", "梦服", "厨师", "医生", "引路者", "捕梦人", "机工", "白日梦大王" };
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