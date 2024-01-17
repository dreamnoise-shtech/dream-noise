using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

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
        public GameObject upgradePopup;
        public GameObject battleLogPopup;
        private System.Random m_random = new System.Random();
        public List<EnemyType> allEnemy = new List<EnemyType>(20);
        private int reward1, reward2, reward3;
        public void fightEnemy()
        {
            List<int> enemy_id_list = new List<int>();
            switch(m_data.gameTowerLevel)
            {
                case 1:
                    enemy_id_list = new List<int> { 1, 2 };
                    break;
                case 2:
                    enemy_id_list = new List<int> { 2, 3, 4 };
                    break;
                case 3:
                    enemy_id_list = new List<int> { 2, 3, 4, 5 };
                    break;
            }
            m_data.enemy = allEnemy[enemy_id_list[m_random.Next(enemy_id_list.Count)]];
            SceneManager.LoadScene("Battle");
        }

        public void fightBoss()
        {
            List<int> boss_id_list = new List<int>() { 5, 6, 7, 8 };
            m_data.enemy = allEnemy[boss_id_list[m_data.gameTowerLevel]];
            SceneManager.LoadScene("Battle");
        }

        const int rewardBuffStartID = 28;

        public void upgrade1()
        {
            if (reward1 < rewardBuffStartID)
            {
                m_data.cardHeapBonusCards.Add(reward1);
            }
            else
            {
                int buff_reward = reward1 - rewardBuffStartID;
                m_data.rewardBuffs[buff_reward] = true;
            }
            updateCardHeapScroll();
            upgradePopup.SetActive(false);
        }

        public void upgrade2()
        {
            if (reward2 < rewardBuffStartID)
            {
                m_data.cardHeapBonusCards.Add(reward2);
            }
            else
            {
                int buff_reward = reward2 - rewardBuffStartID;
                m_data.rewardBuffs[buff_reward] = true;
            }
            updateCardHeapScroll();
            upgradePopup.SetActive(false);
        }

        public void upgrade3()
        {
            if (reward3 < rewardBuffStartID)
            {
                m_data.cardHeapBonusCards.Add(reward3);
            }
            else
            {
                int buff_reward = reward3 - rewardBuffStartID;
                m_data.rewardBuffs[buff_reward] = true;
            }
            updateCardHeapScroll();
            upgradePopup.SetActive(false);
        }

        public void rejectupgrade()
        {
            upgradePopup.SetActive(false);
        }

        public void closebattlelog()
        {
            m_data.battleLog = "";
            battleLogPopup.SetActive(false);
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
                        result += $"����: {m_data.cardNames[currentID]}, ����: {count}��Ч����{m_data.cardDescriptionText[currentID]}\n";
                        currentID = sortedIDList[i];
                        count = 1;
                    }
                }

                // Output the last ID and count
                result += $"����: {m_data.cardNames[currentID]}, ����: {count}��Ч����{m_data.cardDescriptionText[currentID]}\n";
                return result;
            }
            var cardScrollText = GameObject.Find("CardScrollText").GetComponent<TMP_Text>();
            cardScrollText.text = CountDistinctIDs(m_data.cardHeapBonusCards); // TODO: Output it.
            for (int i=0;i<10;i++)
            {
                if (m_data.rewardBuffs[i])
                {
                    switch(i)
                    {
                        case 0:
                            cardScrollText.text += "�������ñ�������� 1��\n";
                            break;
                        case 1:
                            cardScrollText.text += "�Ѵ����η��������� 2��\n";
                            break;
                        case 2:
                            cardScrollText.text += "����ļ��ʦ����ʳ���ָ��� +6��\n";
                            break;
                        case 3:
                            cardScrollText.text += "����ļҽʦ��ÿ�ν���ս��ʱ�ָ� 10 ������\n";
                            break;
                        case 4:
                            cardScrollText.text += "����ļ�����ˣ����� 20% �˺���\n";
                            break;
                        case 5:
                            cardScrollText.text += "����ļ������ȼ��Ϊ�λ��ṩ�ṩ 20% ����������\n";
                            break;
                    }
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            if (m_data.battleResult != -1) // Main scene is not fresh start, either lost or won
            {
                battleLogPopup.SetActive(true);
                GameObject.Find("BattleLogText").GetComponent<TMP_Text>().text = m_data.battleLog;
                if (m_data.battleResult > 0) // Main scene is not at fresh start, but is just loaded by a battle scene and it won.
                {
                    if (m_data.enemy.isBoss) m_data.gameTowerLevel++;
                    List<int> rewardsList = m_data.enemy.rewards;
                    int m_randomPicker()
                    {
                        return rewardsList[m_random.Next(0, rewardsList.Count)];
                    }
                    reward1 = m_randomPicker();
                    reward2 = m_randomPicker();
                    reward3 = m_randomPicker();
                    upgradePopup.SetActive(true);
                    var upgrade1sprite = GameObject.Find("Upgrade1Sprite").GetComponent<UnityEngine.UI.Image>();
                    var upgrade2sprite = GameObject.Find("Upgrade2Sprite").GetComponent<UnityEngine.UI.Image>();
                    var upgrade3sprite = GameObject.Find("Upgrade3Sprite").GetComponent<UnityEngine.UI.Image>();
                    upgrade1sprite.sprite = m_data.cardInfoSprites[reward1];
                    upgrade2sprite.sprite = m_data.cardInfoSprites[reward2];
                    upgrade3sprite.sprite = m_data.cardInfoSprites[reward3];
                }
                else upgradePopup.SetActive(false);
            }
            else
            {
                upgradePopup.SetActive(false);
                battleLogPopup.SetActive(false);
            }
            updateCardHeapScroll();

            GameObject.Find("TowerLevel").GetComponent<TMP_Text>().text = $"��ǰ�ξ�������{m_data.gameTowerLevel}";
            GameObject.Find("CurrentHp").GetComponent<TMP_Text>().text = $"��ǰ HP��{m_data.playerHp}";
            if (m_data.gameTowerLevel == 4)
            {
                GameObject.Find("TowerLevel").GetComponent<TMP_Text>().text = "ͨ�أ��ξ�����������Ϊ 1";
                m_data.gameTowerLevel = 1;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
