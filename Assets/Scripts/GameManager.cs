using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tools.UI.Card
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private MainBattleDataManager m_data;
        private RectTransform actionBarRectTransform;
        private float actionBarOriginalWidth;

        private int currentFireProcess = 0, neededFireProcess = 20, turnCount = 0;

        public void MonsterDie(bool isAlive)
        {
            if (isAlive) { return; }
            GameObject spriteGameObject = GameObject.Find("Monster");
            spriteGameObject.SetActive(false);
        }

        public void TurnLogic()
        {
            turnCount++;
            var tmp = actionBarRectTransform.transform.localScale;
            tmp.x = actionBarOriginalWidth * (turnCount % m_data.enemy.atkInterval) / (float)(m_data.enemy.atkInterval);
            var playerhp = GameObject.Find("PlayerHp").GetComponent<HealthSystemForDummies>();
            var carddrawer = GameObject.Find("Deck").GetComponent<UiCardUtils>();
            if (turnCount % m_data.enemy.atkInterval == 0)
            {
                tmp.x = actionBarOriginalWidth;
                playerhp.AddToCurrentHealth(-m_data.enemy.atk);
            }
            actionBarRectTransform.transform.localScale = tmp;
            carddrawer.DrawCard(2);
        }

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            actionBarRectTransform = GameObject.Find("MonsterActionBar").GetComponent<RectTransform>();
            actionBarOriginalWidth = actionBarRectTransform.transform.localScale.x;
            m_data = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>();
            GameObject.Find("Monster").GetComponent<SpriteRenderer>().sprite = m_data.enemy.sprite;
            var playerhp = GameObject.Find("PlayerHp").GetComponent<HealthSystemForDummies>();
            var monsterhp = GameObject.Find("Monster").GetComponent<HealthSystemForDummies>();
            monsterhp.AddToMaximumHealth(m_data.enemy.hp);
            monsterhp.ReviveWithMaximumHealth();
            playerhp.AddToMaximumHealth(m_data.playerHp);
            playerhp.ReviveWithMaximumHealth();
            var hand = GameObject.Find("Hand").GetComponent<IUiCardHand>();
            hand.OnCardPlayed += (card) =>
            {
                CardInfo cardinfo = card.gameObject.GetComponent<CardInfo>();

                var fireprogress = GameObject.Find("FireProgress").GetComponent<TMP_Text>();
                switch (cardinfo.id)
                {
                    case 0:
                        playerhp.AddToCurrentHealth(100); break;
                    case 1:
                        currentFireProcess++;
                        fireprogress.text = "Heat: " + currentFireProcess.ToString() + " / " + neededFireProcess.ToString();
                        if (currentFireProcess == neededFireProcess) monsterhp.Kill();
                        break;
                    case 2:
                        monsterhp.AddToCurrentHealth(-100); break;
                }
            };
        }

    }

}