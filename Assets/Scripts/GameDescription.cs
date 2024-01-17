using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.UI.Card
{
    public class GameDescription : MonoBehaviour
    {
        public GameObject PopupInstance;
        public void showDescription()
        {
            PopupInstance.SetActive(true);
        }
        public void closeDescription()
        {
            PopupInstance.SetActive(false);
        }

        void Start()
        {
        }
    }

}