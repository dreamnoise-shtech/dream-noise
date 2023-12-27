using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.UI.Card
{
    public class UiCardUtils : MonoBehaviour
    {
        //--------------------------------------------------------------------------------------------------------------

        #region Fields

        private System.Random rng;

        public List<IUiCard> CardHeap; // Cards player can draw in combat.

        private int Count { get; set; }

        /*        [SerializeField] [Tooltip("Prefab of the Card C#")]
                private GameObject cardPrefabCs;*/

        [SerializeField] [Tooltip("World point where the deck is positioned")]
        private Transform deckPosition;

        [SerializeField]
        [Tooltip("Reference to Graveyard")]
        private UiCardGraveyard m_graveyard;

        /*        [SerializeField] [Tooltip("Game view transform")]
                private Transform gameView;*/
        [SerializeField]
        [Tooltip("Card Heap transform (out of window now)")]
        private Transform CardHeapView;

        private UiCardHand CardHand { get; set; }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Unitycallbacks

        private void Awake()
        {
            rng = new System.Random();
            CardHand = transform.parent.GetComponentInChildren<UiCardHand>();
            CardHeap = new List<IUiCard>();
        }

        private void spawnSpecificCard(GameObject prefab, int id, int num = 1)
        {
            for (int i = 0; i < num; i++)
            {
                var cardGo = Instantiate(prefab, CardHeapView);
                cardGo.name = "Card_" + Count;
                var card = cardGo.GetComponent<IUiCard>();
                var cardinfo = cardGo.GetComponent<CardInfo>();
                cardinfo.prefab = prefab;
                cardinfo.id = id;
                //card.transform.position = deckPosition.position;
                Count++;
                CardHeap.Add(card);
                //UnityEngine.Debug.Log(card.gameObject.GetComponent<OriginalPrefab>().prefab);
            }
        }

        private void Start()
        {
            //starting cards
            /*for (var i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(0.2f);
                DrawCard();
            }*/
            spawnSpecificCard((GameObject)Resources.Load("CardHealPrefab"), 0, 3);
            spawnSpecificCard((GameObject)Resources.Load("CardFuelPrefab"), 1, 3);
            spawnSpecificCard((GameObject)Resources.Load("CardAttackPrefab"), 2, 4);
            var bonuscards = GameObject.Find("MainDataManager").GetComponent<MainBattleDataManager>().cardHeapBonusCards;
            foreach (int cardid in bonuscards)
            {
                switch(cardid)
                {
                    case 12: spawnSpecificCard((GameObject)Resources.Load("CardSleepwalkPrefab"), 12); break;
                    case 13: spawnSpecificCard((GameObject)Resources.Load("CardDeepDreamPrefab"), 13); break;
                    case 14: spawnSpecificCard((GameObject)Resources.Load("CardDreamNetPrefab"), 14); break;
                }
            }
            DrawCard(2);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Operations

        [Button]
        public void DrawCard(int drawNum = 1)
        {
            //TODO: Consider replace Instantiate by an Object Pool Pattern
            /*var cardGo = Instantiate(cardPrefabCs, gameView);
            cardGo.name = "Card_" + Count;
            var card = cardGo.GetComponent<IUiCard>();
            card.transform.position = deckPosition.position;
            Count++;
            CardHand.AddCard(card);*/
            for (var i = 0; i < drawNum; i++)
            {
                if (CardHeap.Count == 0)
                {
                    /*                spawnSpecificCard(cardHealPrefab, 3);
                                    spawnSpecificCard(cardFirePrefab, 3);
                                    spawnSpecificCard(cardAttackPrefab, 4);*/
                    foreach (var checkingcard in m_graveyard.Cards)
                    {
                        //TODO!!! Wrong implementation.
                        //UnityEngine.Debug.Log(checkingcard.gameObject.GetComponent<OriginalPrefab>().prefab);
                        var cardinfo = checkingcard.gameObject.GetComponent<CardInfo>();
                        spawnSpecificCard(cardinfo.prefab, cardinfo.id);
                    }
                    m_graveyard.ClearCards();
                    if (CardHeap.Count == 0)
                        return;
                }
                CardHeap = CardHeap.OrderBy(_ => rng.Next()).ToList();
                var card = CardHeap.First();
                card.transform.position = deckPosition.position;
                CardHeap.Remove(card);
                CardHand.AddCard(card);
            }
        }

        [Button]
        public void PlayCard()
        {
            if (CardHand.Cards.Count > 0)
            {
                var randomCard = CardHand.Cards.RandomItem();
                CardHand.PlayCard(randomCard);
            }
        }

        private void Update()
        {
/*            if (Input.GetKeyDown(KeyCode.Tab)) DrawCard();
            if (Input.GetKeyDown(KeyCode.Space)) PlayCard();
            if (Input.GetKeyDown(KeyCode.Escape)) Restart();*/
        }

        public void Restart()
        {
            SceneManager.LoadScene(0);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
    }
}