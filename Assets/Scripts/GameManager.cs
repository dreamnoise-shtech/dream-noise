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
        private TMP_Text playerbufftext, enemybufftext;
        private MainBattleDataManager m_data;
        private RectTransform actionBarRectTransform;
        private float actionBarOriginalWidth;
        private HealthSystemForDummies playerhp, monsterhp;
        private UiCardUtils carddrawer;

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
            if ((m_data.enemy.isBoss) && (m_data.battleResult != 2))
            {
                m_data.battleResult = -1;
                monsterhp = GameObject.Find("Monster").GetComponent<HealthSystemForDummies>();
                monsterhp.ReviveWithMaximumHealth();
                return;
            }
            GameObject spriteGameObject = GameObject.Find("Monster");
            spriteGameObject.SetActive(false);
            if (m_data.battleResult != 2) m_data.battleResult = 1;
            SceneManager.LoadScene("Main");
        }

        private void UpdateBuffText(TMP_Text buffText, List<BuffState> buffs)
        {
            if (buffs.Count == 0) {
                buffText.text = "N/A";
                return;
            }
            else
            {
                buffText.text = "";
                foreach(var buff in buffs)
                {
                    switch (buff.buffId)
                    {
                        case 4: // sleepwalk, from card ID 4, enemy attack itself once.
                            buffText.text += "梦游中，下次攻击会攻击自己。 \n";
                            break;
                        case 5: // deep dream, from card ID 5, doubles the next attack.
                            buffText.text += "处于梦寐中，打出的下一张战斗牌伤害 * 2。\n";
                            break;
                        case 6: // dream net, from card ID 6, enemy cannot attack for two turns.
                            buffText.text += "被梦网困住，下 " + buff.buffRemainingTurn.ToString() + " 回合内的攻击无效。\n";
                            break;
                    }
                }
                return;
            }
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
            UpdateBuffText(playerbufftext, playerBuffs);
            UpdateBuffText(enemybufftext, enemyBuffs);
            // ---vv--- Enemy Action, action bar
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x = actionBarOriginalWidth * (turnCount % m_data.enemy.atkInterval) / (float)(m_data.enemy.atkInterval);
            if (turnCount % m_data.enemy.atkInterval == 0) // turn for enemy to attack
            {
                tmp.x = actionBarOriginalWidth;
                var sleepwalkBuff = enemyBuffs.Find(buff => buff.buffId == 4);
                var dreamNetBuff = enemyBuffs.Find(buff => buff.buffId == 6);
                if (dreamNetBuff != null) // dream net, from card ID 6, enemy cannot attack in 2 turns.
                {
                    // intentionally left blank, i.e. do nothing
                }
                else if (sleepwalkBuff != null) // sleepwalk, from card ID 4, enemy attack itself once.
                {
                    monsterhp.AddToCurrentHealth(-m_data.enemy.atk);
                    enemyBuffs.Remove(sleepwalkBuff);
                }
                else { playerhp.AddToCurrentHealth(-m_data.enemy.atk); }
            }
            actionBarRectTransform.transform.localScale = tmp; // Set action bar
            // ---^^---

            carddrawer.DrawCard(2);
        }

        void Awake()
        {
            Instance = this;
            carddrawer = GameObject.Find("Deck").GetComponent<UiCardUtils>();
        }

        void damageEnemy(int dmg)
        {
            var deepdreamBuff = playerBuffs.Find(buff => buff.buffId == 5); // deep dream, from card ID 5, 2x dmg
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
            playerbufftext = GameObject.Find("PlayerBuffText").GetComponent<TMP_Text>();
            enemybufftext = GameObject.Find("EnemyBuffText").GetComponent<TMP_Text>();
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
                void addOrRefreshBuff(List<BuffState> buffList, int buffId, int buffTurn = -1)
                {
                    buffList.RemoveAll(buff => buff.buffId == buffId);
                    buffList.Add(new BuffState(buffId, buffTurn));
                }
                switch (cardinfo.id)
                {
                    case 0:
                        currentFireProcess++;
                        fireprogress.text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();
                        if (currentFireProcess == neededFireProcess)
                        {
                            m_data.battleResult = 2; // fuel win, see MainBattleDataManager
                            monsterhp.Kill();
                        }
                        break;
                    case 1:
                        damageEnemy(4);
                        break;
                    case 2:
                        damageEnemy(3);
                        break;
                    case 3:
                        damageEnemy(6);
                        break;
                    case 4: // sleepwalk, enemy debuff ID=4
                        damageEnemy(2);
                        addOrRefreshBuff(enemyBuffs, 4);
                        break;
                    case 5: // deep dream, player buff ID=5
                        addOrRefreshBuff(playerBuffs, 5);
                        break;
                    case 6: // dream net, enemy debuff ID=6
                        damageEnemy(3);
                        addOrRefreshBuff(enemyBuffs, 6, 2);
                        break;
                    case 7:
                        carddrawer.DrawCard(3);
                        carddrawer.PlayCard();
                        break;
                    case 8:
                        carddrawer.spawnSpecificCardInGraveyard(9, 5);
                        break;
                    case 9:
                        carddrawer.DrawCard(1);
                        damageEnemy(2);
                        playerhp.AddToCurrentHealth(2);
                        break;
                    case 26:
                        playerhp.AddToCurrentHealth(4);
                        break;
                }
                UpdateBuffText(playerbufftext, playerBuffs);
                UpdateBuffText(enemybufftext, enemyBuffs);
            };
        }

    }

}