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
            public int buffDetail; // Some buffs have specific data to be stored
            public BuffState(int buffid, int buffturn = -1, int buffdetail = 0)
            {
                buffId = buffid;
                buffRemainingTurn = buffturn;
                buffDetail = buffdetail;
            }
        }
        private List<BuffState> playerBuffs = new List<BuffState>(), enemyBuffs = new List<BuffState>();
        private TMP_Text playerbufftext, enemybufftext, battleeventtext;
        private MainBattleDataManager m_data;
        private RectTransform actionBarRectTransform;
        private float actionBarOriginalWidth;
        private HealthSystemForDummies playerhp, monsterhp;
        private UiCardUtils carddrawer;

        private int currentFireProcess = 0, neededFireProcess = 20, turnCount = 0, cardsUsed = 0;
        public Animator playerOverlay, monsterOverlay;

        public void PlayerDie(bool isAlive)
        {
            if (isAlive) { return; }
            WriteBattleLog("玩家 HP 耗尽。");
            playerOverlay.Play("Base Layer.Player_Die");
            m_data.battleResult = 0;
            m_data.playerHp = m_data.playerMaxHp;
            SceneManager.LoadSceneAsync("Main");
        }

        public void MonsterDie(bool isAlive)
        {
            if (isAlive) { return; }
            if ((m_data.enemy.isBoss) && (m_data.battleResult != 2))
            {
                m_data.battleResult = -1;
                WriteBattleLog("敌方 Boss 被伤害击杀，借助梦境力量重生。");
                monsterhp = GameObject.Find("Monster").GetComponent<HealthSystemForDummies>();
                monsterhp.ReviveWithMaximumHealth();
                return;
            }
            WriteBattleLog("敌方被击败。");
            m_data.playerHp = playerhp.CurrentHealth;
            GameObject spriteGameObject = GameObject.Find("Monster");
            spriteGameObject.SetActive(false);
            if (m_data.battleResult != 2) m_data.battleResult = 1;
            SceneManager.LoadScene("Main");
        }

        public void PlayerHealthChange(CurrentHealth currentHealth)
        {
            WriteBattleLog($"玩家 HP 由 {currentHealth.previous} 变为 {currentHealth.current}。");
            if (currentHealth.current <= 0) return;
            if (currentHealth.previous > currentHealth.current)
                playerOverlay.Play("Base Layer.Slash_Play");
            if (currentHealth.previous < currentHealth.current)
            {
                playerOverlay.Play("Base Layer.Heal_Play");
            }
        }

        public void MonsterHealthChange(CurrentHealth currentHealth)
        {
            WriteBattleLog($"敌方 HP 由 {currentHealth.previous} 变为 {currentHealth.current}。");
            if (currentHealth.current <= 0) return;
            if (currentHealth.previous > currentHealth.current)
                monsterOverlay.Play("Base Layer.Slash_Play");
            if (currentHealth.previous < currentHealth.current)
            {
                playerOverlay.Play("Base Layer.Heal_Play");
            }
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
                        case 20: // countered, next card invalid.
                            buffText.text += "下一张打出的卡无效。\n";
                            break;
                        case 100: // reflect, reflecting player damage.
                            buffText.text += "反弹下次伤害。\n";
                            break;
                    }
                }
                return;
            }
        }

        public void WriteBattleLog(string blog)
        {
            m_data.battleLog += blog + "\n";
            battleeventtext.text = m_data.battleLog;
            return;
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
            if (playerbufftext.text != "N/A")
                WriteBattleLog($"第{turnCount}回合开始时，玩家状态如下：\n[\n{playerbufftext.text}]");
            UpdateBuffText(enemybufftext, enemyBuffs);
            if (enemybufftext.text != "N/A")
                WriteBattleLog($"第{turnCount}回合开始时，敌方状态如下：\n[\n{enemybufftext.text}]");
            // ---vv--- Enemy Action, action bar
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x = actionBarOriginalWidth * (turnCount % m_data.enemy.atkInterval) / (float)(m_data.enemy.atkInterval);
            if (turnCount % m_data.enemy.atkInterval == 0) // turn for enemy to attack
            {
                WriteBattleLog($"第{turnCount}回合，敌方进行一次攻击。");
                tmp.x = actionBarOriginalWidth;
                var sleepwalkBuff = enemyBuffs.Find(buff => buff.buffId == 4);
                var dreamNetBuff = enemyBuffs.Find(buff => buff.buffId == 6);
                if (dreamNetBuff != null) // dream net, from card ID 6, enemy cannot attack in 2 turns.
                {
                    // intentionally left blank, i.e. do nothing
                    WriteBattleLog($"敌方处于梦网中，攻击无效。");
                }
                else if (sleepwalkBuff != null) // sleepwalk, from card ID 4, enemy attack itself once.
                {
                    damageEnemy(m_data.enemy.atk);
                    WriteBattleLog($"敌方正在梦游，攻击自身。");
                    enemyBuffs.Remove(sleepwalkBuff);
                }
                else { damagePlayer(m_data.enemy.atk); }
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
            var reflectBuff = enemyBuffs.Find(buff => buff.buffId == 100); // reflect, reflecting player dmg
            bool targetEnemy = true; // 1: normal target; 0: reflected
            int finalDmg = dmg;
            if (reflectBuff != null)
            {
                targetEnemy = false;
                enemyBuffs.Remove(reflectBuff);
                WriteBattleLog("敌方反弹了伤害。");
            }
            if (deepdreamBuff != null)
            {
                finalDmg *= 2;
                playerBuffs.Remove(deepdreamBuff);
                WriteBattleLog($"玩家处于梦寐中，伤害力翻倍。");
            }
            if (targetEnemy)
            {
                WriteBattleLog($"玩家对敌方造成 {finalDmg} 伤害。");
                monsterhp.AddToCurrentHealth(-finalDmg);
                return;
            }
            else
            {
                damagePlayer(finalDmg);
                return;
            }
        }

        void damagePlayer(int dmg)
        {
            int finalDmg = dmg;
            WriteBattleLog($"玩家受到 {finalDmg} 伤害。");
            playerhp.AddToCurrentHealth(-finalDmg);
            return;
        }

        void fuelUsed(int fuels = 1)
        {
            var fireprogress = GameObject.Find("FireProgress").GetComponent<TMP_Text>();
            currentFireProcess += fuels;
            fireprogress.text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();
            WriteBattleLog($"玩家供应了 {fuels} 点燃料，现有 {currentFireProcess} 点燃料。");
            if (currentFireProcess >= neededFireProcess)
            {
                m_data.battleResult = 2; // fuel win, see MainBattleDataManager
                WriteBattleLog($"玩家供应了足量 ({neededFireProcess}) 燃料，点燃梦火获胜。");
                monsterhp.Kill();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            playerbufftext = GameObject.Find("PlayerBuffText").GetComponent<TMP_Text>();
            enemybufftext = GameObject.Find("EnemyBuffText").GetComponent<TMP_Text>();
            battleeventtext = GameObject.Find("BattleEventText").GetComponent<TMP_Text>();
            actionBarRectTransform = GameObject.Find("MonsterActionBar").GetComponent<RectTransform>();
            actionBarOriginalWidth = actionBarRectTransform.transform.localScale.x;
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x *= 0.1f;
            actionBarRectTransform.transform.localScale = tmp;
            turnCount = 0;
            cardsUsed = 0;

            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            GameObject.Find("Monster").GetComponent<SpriteRenderer>().sprite = m_data.enemy.sprite;
            playerhp = GameObject.Find("PlayerHp").GetComponent<HealthSystemForDummies>();
            monsterhp = GameObject.Find("Monster").GetComponent<HealthSystemForDummies>();
            monsterhp.AddToMaximumHealth(m_data.enemy.hp);
            monsterhp.ReviveWithMaximumHealth();
            playerhp.AddToMaximumHealth(m_data.playerMaxHp);
            playerhp.ReviveWithCustomHealth(m_data.playerHp);
            neededFireProcess = m_data.enemy.fuelToKill;
            GameObject.Find("FireProgress").GetComponent<TMP_Text>().text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();
            m_data.battleLog = "";


            var hand = GameObject.Find("Hand").GetComponent<IUiCardHand>();
            hand.OnCardPlayed += (card) =>
            {
                CardInfo cardinfo = card.gameObject.GetComponent<CardInfo>();
                WriteBattleLog($"第{turnCount}回合，玩家使用了一张 {m_data.cardNames[cardinfo.id]}。");
                var counteredDebuff = playerBuffs.Find(buff => buff.buffId == 20); // countered, next card invalid
                if (counteredDebuff != null)
                {
                    playerBuffs.Remove(counteredDebuff);
                    UpdateBuffText(playerbufftext, playerBuffs);
                    return;
                }

                cardsUsed++;
                void addOrRefreshBuff(List<BuffState> buffList, int buffId, int buffTurn = -1, int buffDetail = 0)
                {
                    switch(buffId) // some buff needs to be "additively refreshed"
                    {
                        default:
                            break;
                    }
                    buffList.RemoveAll(buff => buff.buffId == buffId);
                    buffList.Add(new BuffState(buffId, buffTurn, buffDetail));
                }
                switch (cardinfo.id)
                {
                    case 0:
                        fuelUsed();
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
                    case 25:
                        playerhp.AddToCurrentHealth(4);
                        break;
                    case 26:
                        playerhp.AddToCurrentHealth(8);
                        break;
                    case 27:
                        playerhp.AddToCurrentHealth(5);
                        break;
                }
                // vvv Boss reaction to player card played

                switch (m_data.enemy.enemyId)
                {
                    case 5: // dragon
                        if (cardsUsed % 3 == 0)
                        {
                            fuelUsed();
                            addOrRefreshBuff(enemyBuffs, 100);
                        }
                        break;
                    case 6: // warboss
                        if (cardsUsed %2 == 0)
                        {
                            addOrRefreshBuff(playerBuffs, 20);
                        }
                        break;
                }

                // ^^^

                UpdateBuffText(playerbufftext, playerBuffs);
                UpdateBuffText(enemybufftext, enemyBuffs);
            };
        }

    }

}