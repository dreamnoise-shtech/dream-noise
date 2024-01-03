using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq.Expressions;

namespace Tools.UI.Card
{
    [System.Serializable]
    public class EnemyType
    {
        public int enemyId;
        public bool isBoss;
        public string enemyName;
        public int hp;
        public int atk;
        public int atkInterval;
        public int fuelToKill;
        public Sprite sprite;
        public List<int> rewards = new List<int>();
    };
    public class MainManager : MonoBehaviour
    {
        private MainBattleDataManager m_data;
        private GameObject upgradepopup;
        private System.Random m_random = new System.Random();
        public List<EnemyType> allEnemy = new List<EnemyType>(20);
        private int reward1, reward2, reward3;
        public int gameTowerLevel = 1;
        public void fightEnemy()
        {
            m_data.enemy = allEnemy[m_random.Next(1, 4)];
            SceneManager.LoadScene("Battle");
        }

        public void fightBoss()
        {
            m_data.enemy = allEnemy[6];
            SceneManager.LoadScene("Battle");
        }

        public void upgrade1()
        {
            m_data.cardHeapBonusCards.Add(reward1);
            updateCardHeapScroll();
            upgradepopup.SetActive(false);
        }

        public void upgrade2()
        {
            m_data.cardHeapBonusCards.Add(reward2);
            updateCardHeapScroll();
            upgradepopup.SetActive(false);
        }

        public void upgrade3()
        {
            m_data.cardHeapBonusCards.Add(reward3);
            updateCardHeapScroll();
            upgradepopup.SetActive(false);
        }

        public void rejectupgrade()
        {
            upgradepopup.SetActive(false);
        }

        public void updateCardHeapScroll()
        {
            m_data.cardHeapBonusCards.Sort();
            string CountDistinctIDs(List<int> sortedIDList)
            {
                string result = "";
                int currentID = sortedIDList[0];
                int count = 1;

                for (int i = 1; i < sortedIDList.Count; i++)
                {
                    if (sortedIDList[i] == currentID)
                    {
                        count++;
                    }
                    else
                    {
                        result += $"卡名: {m_data.cardNames[currentID]}, 数量: {count}\n";
                        currentID = sortedIDList[i];
                        count = 1;
                    }
                }

                // Output the last ID and count
                result += $"卡名: {m_data.cardNames[currentID]}, 数量: {count}";
                return result;
            }
            var cardScrollText = GameObject.Find("CardScrollText").GetComponent<TMP_Text>();
            cardScrollText.text = CountDistinctIDs(m_data.cardHeapBonusCards); // TODO: Output it.
        }

        // Start is called before the first frame update
        void Start()
        {
            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            upgradepopup = GameObject.Find("UpgradePopup"); // show the upgrade menu
            if (m_data.battleResult > 0) // Main scene is not at fresh start, but is just loaded by a battle scene and it won.
            {
                if (m_data.enemy.isBoss) gameTowerLevel++;
                List<int> rewardsList = m_data.enemy.rewards;
                int m_randomPicker()
                {
                    return rewardsList[m_random.Next(0, rewardsList.Count)];
                }
                reward1 = m_randomPicker();
                reward2 = m_randomPicker();
                reward3 = m_randomPicker();
                var upgrade1Text = GameObject.Find("Upgrade1Text").GetComponent<TMP_Text>();
                var upgrade2Text = GameObject.Find("Upgrade2Text").GetComponent<TMP_Text>();
                var upgrade3Text = GameObject.Find("Upgrade3Text").GetComponent<TMP_Text>();
                upgrade1Text.text = $"将一张 {m_data.cardNames[reward1]} 加入牌堆";
                upgrade2Text.text = $"将一张 {m_data.cardNames[reward2]} 加入牌堆";
                upgrade3Text.text = $"将一张 {m_data.cardNames[reward3]} 加入牌堆";
                upgradepopup.SetActive(true);
            }
            else upgradepopup.SetActive(false);

            updateCardHeapScroll();
            GameObject.Find("TowerLevel").GetComponent<TMP_Text>().text = $"当前梦境层数：{gameTowerLevel}";

            m_data.playerHp = 30;
            m_data.battleResult = -1;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
