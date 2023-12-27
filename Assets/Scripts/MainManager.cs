using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tools.UI.Card
{
    public class EnemyType
    {
        public int enemyId;
        public string enemyName;
        public int hp;
        public int atk;
        public int atkInterval;
        public int fuelToKill;
        public Sprite sprite;
    };
    public class MainManager : MonoBehaviour
    {
        private MainBattleDataManager m_data;
        private GameObject upgradepopup;
        public Sprite wolfSprite; // TODO: remove it, use code instead
        public Sprite bearSprite;
        public Sprite thiefSprite;
        public Sprite dryadSprite;
        public void fight1()
        {
            var emenyNow = new EnemyType();
            emenyNow.enemyId = 100;
            emenyNow.enemyName = "Wolf";
            emenyNow.hp = 500;
            emenyNow.atk = 20;
            emenyNow.atkInterval = 2;
            emenyNow.fuelToKill = 21;
            emenyNow.sprite = wolfSprite;
            m_data.enemy = emenyNow;
            SceneManager.LoadScene("Battle");
        }

        public void fight2()
        {
            var emenyNow = new EnemyType();
            emenyNow.enemyId = 101;
            emenyNow.enemyName = "Bear";
            emenyNow.hp = 700;
            emenyNow.atk = 40;
            emenyNow.atkInterval = 3;
            emenyNow.fuelToKill = 12;
            emenyNow.sprite = bearSprite;
            m_data.enemy = emenyNow;
            SceneManager.LoadScene("Battle");
        }

        public void fight3()
        {
            var emenyNow = new EnemyType();
            emenyNow.enemyId = 102;
            emenyNow.enemyName = "Thief";
            emenyNow.hp = 800;
            emenyNow.atk = 15;
            emenyNow.atkInterval = 1;
            emenyNow.fuelToKill = 30;
            emenyNow.sprite = thiefSprite;
            m_data.enemy = emenyNow;
            SceneManager.LoadScene("Battle");
        }

        public void fight4()
        {
            var emenyNow = new EnemyType();
            emenyNow.enemyId = 104;
            emenyNow.enemyName = "Dryad";
            emenyNow.hp = 1500;
            emenyNow.atk = 60;
            emenyNow.atkInterval = 3;
            emenyNow.fuelToKill = 15;
            emenyNow.sprite = dryadSprite;
            m_data.enemy = emenyNow;
            SceneManager.LoadScene("Battle");
        }

        public void regularupgrade()
        {
            m_data.cardHeapBonusCards.Add(13);
            upgradepopup.SetActive(false);
        }

        public void combatupgrade()
        {
            m_data.cardHeapBonusCards.Add(12);
            upgradepopup.SetActive(false);
        }

        public void fuelupgrade()
        {
            m_data.cardHeapBonusCards.Add(14);
            upgradepopup.SetActive(false);
        }

        public void rejectupgrade()
        {
            upgradepopup.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            upgradepopup = GameObject.Find("UpgradePopup"); // show the upgrade menu
            if (m_data.battleResult > 0) // Main scene is not at fresh start, but is just loaded by a battle scene and it won.
            {
                upgradepopup.SetActive(true);
                GameObject.Find("RegularUpgradeButton").GetComponent<Button>().enabled = true;
                if (m_data.battleResult == 1)
                {
                    // combat win
                    GameObject.Find("CombatUpgradeButton").GetComponent<Button>().enabled = true;
                    GameObject.Find("FuelUpgradeButton").GetComponent<Button>().enabled = false;
                }
                if (m_data.battleResult == 2)
                {
                    // fuel win
                    GameObject.Find("CombatUpgradeButton").GetComponent<Button>().enabled = false;
                    GameObject.Find("FuelUpgradeButton").GetComponent<Button>().enabled = true;
                }
            }
            else upgradepopup.SetActive(false);
            m_data.playerHp = 300;
            m_data.battleResult = -1;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
