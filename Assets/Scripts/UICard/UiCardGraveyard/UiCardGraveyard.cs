using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.UI.Card
{
    //------------------------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Card graveyard holds a register with cards played by the player.
    /// </summary>
    public class UiCardGraveyard : UiCardPile
    {
        [SerializeField] [Tooltip("World point where the graveyard is positioned")]
        private Transform graveyardPosition;

        //--------------------------------------------------------------------------------------------------------------

        private IUiCardHand CardHand { get; set; }


        //--------------------------------------------------------------------------------------------------------------

        #region Unitycallbacks

        protected override void Awake()
        {
            base.Awake();
            CardHand = transform.parent.GetComponentInChildren<UiCardHand>();
            CardHand.OnCardPlayed += AddCard;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Operations

        /// <summary>
        ///     Adds a card to the graveyard or discard pile.
        /// </summary>
        /// <param name="card"></param>
        public override void AddCard(IUiCard card)
        {
            if (card == null)
                throw new ArgumentNullException("Null is not a valid argument.");

            Cards.Add(card);
            card.transform.SetParent(graveyardPosition);
            card.Discard();
            NotifyPileChange();
        }


        /// <summary>
        ///     Removes a card from the graveyard or discard pile.
        /// </summary>
        /// <param name="card"></param>
        public override void RemoveCard(IUiCard card)
        {
            if (card == null)
                throw new ArgumentNullException("Null is not a valid argument.");

            Cards.Remove(card);
            NotifyPileChange();
        }

        public void ClearCards() {
            foreach(var card in Cards)
            {
                UnityEngine.Object.Destroy(card.gameObject);
            }
            Cards.Clear();
            NotifyPileChange();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
    }
}