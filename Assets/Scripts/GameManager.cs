using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Tools.UI.Card
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public class BuffState // including debuff
        {
            public int buffId;
            public int buffRemainingTurn; // -1 for infinite, may has special wokring scheme
            public BuffState(int buffid, int buffturn = -1)
            {
                buffId = buffid;
                buffRemainingTurn = buffturn;
            }
        }
        private List<BuffState> playerBuffs = new List<BuffState>(), enemyBuffs = new List<BuffState>();
        private MainBattleDataManager m_data;
        private RectTransform actionBarRectTransform;
        private float actionBarOriginalWidth;
        private HealthSystemForDummies playerhp, monsterhp;

        private int currentFireProcess = 0, neededFireProcess = 20, turnCount = 0;

        public void PlayerDie(bool isAlive)
        {
            if (isAlive) { return; }
            m_data.battleResult = 0;
            SceneManager.LoadScene("Main");
        }

        public void MonsterDie(bool isAlive)
        {
            if (isAlive) { return; }
            GameObject spriteGameObject = GameObject.Find("Monster");
            spriteGameObject.SetActive(false);
            if (m_data.battleResult != 2) m_data.battleResult = 1;
            SceneManager.LoadScene("Main");
        }

        public void TurnLogic()
        {
            turnCount++;

            // ---vv--- Buff affect and countdown
            foreach(var buff in playerBuffs)
            {
                switch (buff.buffId) { }
                buff.buffRemainingTurn--;
            }
            playerBuffs.RemoveAll(buff => buff.buffRemainingTurn == 0);
            foreach(var buff in enemyBuffs)
            {
                switch (buff.buffId) { }
                buff.buffRemainingTurn--;
            }
            enemyBuffs.RemoveAll(buff => buff.buffRemainingTurn == 0);
            // ---vv--- Show buffs
            var playerbufftext = GameObject.Find("PlayerBuffText").GetComponent<TMP_Text>();
            var enemybufftext = GameObject.Find("EnemyBuffText").GetComponent<TMP_Text>();
            if (playerBuffs.Count == 0) { playerbufftext.text = "N/A"; }
            else
            {
                playerbufftext.text = "";
                foreach(var buff in playerBuffs)
                {
                    switch (buff.buffId) {
                        case 13: // deep dream, from card ID 13, doubles the next attack.
                            playerbufftext.text += "In deep dream, dealing 2x damage once.\n";
                            break;
                    }
                }
            }
            if (enemyBuffs.Count == 0) { enemybufftext.text = "N/A"; }
            else
            {
                enemybufftext.text = "";
                foreach(var buff in enemyBuffs)
                {
                    switch (buff.buffId) {
                        case 12: // sleepwalk, from card ID 12, enemy attack itself once.
                            enemybufftext.text += "Sleepwalking, would attack itself once. \n";
                            break;
                        case 14: // dream net, from card ID 14, enemy cannot attack for two turns.
                            enemybufftext.text += "Captured by a dream net, cannot attack for " + buff.buffRemainingTurn.ToString() + " turns.\n";
                            break;
                    }
                }
            }
            // ---vv--- Enemy Action, action bar
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x = actionBarOriginalWidth * (turnCount % m_data.enemy.atkInterval) / (float)(m_data.enemy.atkInterval);
            if (turnCount % m_data.enemy.atkInterval == 0) // turn for enemy to attack
            {
                tmp.x = actionBarOriginalWidth;
                var sleepwalkBuff = enemyBuffs.Find(buff => buff.buffId == 12);
                var dreamNetBuff = enemyBuffs.Find(buff => buff.buffId == 14);
                if (dreamNetBuff != null) // dream net, from card ID 14, enemy cannot attack in 2 turns.
                {
                    // intentionally left blank, i.e. do nothing
                }
                else if (sleepwalkBuff != null) // sleepwalk, from card ID 12, enemy attack itself once.
                {
                    monsterhp.AddToCurrentHealth(-m_data.enemy.atk);
                    enemyBuffs.Remove(sleepwalkBuff);
                }
                else { playerhp.AddToCurrentHealth(-m_data.enemy.atk); }
            }
            actionBarRectTransform.transform.localScale = tmp; // Set action bar
            // ---^^---

            var carddrawer = GameObject.Find("Deck").GetComponent<UiCardUtils>();
            carddrawer.DrawCard(2);
        }

        void Awake()
        {
            Instance = this;
        }

        void damageEnemy(int dmg)
        {
            var deepdreamBuff = playerBuffs.Find(buff => buff.buffId == 13); // deep dream, from card ID 13, 2x dmg
            if (deepdreamBuff != null)
            {
                monsterhp.AddToCurrentHealth(-dmg * 2);
                playerBuffs.Remove(deepdreamBuff);
            }
            else monsterhp.AddToCurrentHealth(-dmg);
        }

        // Start is called before the first frame update
        void Start()
        {

            actionBarRectTransform = GameObject.Find("MonsterActionBar").GetComponent<RectTransform>();
            actionBarOriginalWidth = actionBarRectTransform.transform.localScale.x;
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x *= 0.1f;
            actionBarRectTransform.transform.localScale = tmp;

            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            GameObject.Find("Monster").GetComponent<SpriteRenderer>().sprite = m_data.enemy.sprite;
            playerhp = GameObject.Find("PlayerHp").GetComponent<HealthSystemForDummies>();
            monsterhp = GameObject.Find("Monster").GetComponent<HealthSystemForDummies>();
            monsterhp.AddToMaximumHealth(m_data.enemy.hp);
            monsterhp.ReviveWithMaximumHealth();
            playerhp.AddToMaximumHealth(m_data.playerHp);
            playerhp.ReviveWithMaximumHealth();
            neededFireProcess = m_data.enemy.fuelToKill;
            GameObject.Find("FireProgress").GetComponent<TMP_Text>().text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();

            var hand = GameObject.Find("Hand").GetComponent<IUiCardHand>();
            hand.OnCardPlayed += (card) =>
            {
                CardInfo cardinfo = card.gameObject.GetComponent<CardInfo>();

                var fireprogress = GameObject.Find("FireProgress").GetComponent<TMP_Text>();
                switch (cardinfo.id)
                {
                    case 0: // TODO: remove this old test card, introduce new card
                        playerhp.AddToCurrentHealth(40); break;
                    case 1: // TODO: remove this old test card, introduce new card
                        currentFireProcess++;
                        fireprogress.text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();
                        if (currentFireProcess == neededFireProcess)
                        {
                            m_data.battleResult = 2; // fuel win, see MainBattleDataManager
                            monsterhp.Kill();
                        }
                        break;
                    case 2: // TODO: remove this old test card, introduce new card
                        damageEnemy(60);
                        break;
                    case 12: // sleepwalk, enemy debuff ID=12
                        damageEnemy(20);
                        enemyBuffs.Add(new BuffState(12));
                        break;
                    case 13: // deep dream, player buff ID=13
                        playerBuffs.Add(new BuffState(13));
                        break;
                    case 14: // dream net, enemy debuff ID=14
                        damageEnemy(30);
                        enemyBuffs.Add(new BuffState(14));
                        break;

                }
            };
        }

    }

}