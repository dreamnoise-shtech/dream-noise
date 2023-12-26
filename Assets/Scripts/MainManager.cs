using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public Sprite wolfSprite; // TODO: remove it, use code instead
        public void fight1()
        {
            var wolf = new EnemyType();
            wolf.enemyId = 0;
            wolf.enemyName = "Wolf";
            wolf.hp = 500;
            wolf.atk = 20;
            wolf.atkInterval = 2;
            wolf.fuelToKill = 21;
            wolf.sprite = wolfSprite;
            m_data.enemy = wolf;
            SceneManager.LoadScene("Battle");
        }

        // Start is called before the first frame update
        void Start()
        {
            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            m_data.playerHp = 300;

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
