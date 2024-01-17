using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.UI.Card
{
    public class MainBattleDataManager : MonoBehaviour
    {
        private static MainBattleDataManager instance;
        public EnemyType enemy;
        public float playerHp;
        public float playerMaxHp;
        public List<int> cardHeapBonusCards = new List<int>();
        public List<GameObject> prefabs = new List<GameObject>();
        public List<string> cardNames = new List<string> { "����", "�ζ�", "�ε�", "�λ�", "����", "����", "����", "����", "����", "�ζ�", "����֮צ", "�ξ�֮��", "�λ�֮��", "�λ�֮Դ", "����֮��", "����֮��", "����֮��", "�ζ�֮��", "��Բ֮ҹ", "����֮ӿ", "����֮Ϣ", "��ʼ֮ӡ", "����֮��", "����֮��", "�ζ�֮��", "��ʳ��", "���Ȱ�", "ҩˮ", "��ñ", "�η�", "��ʦ", "ҽ��", "��·��", "������", "����", "�����δ���" };
        public List<string> cardDescriptionText = new List<string>();
        public List<Sprite> cardInfoSprites = new List<Sprite>();
        public int battleResult = -1; // -1 for uninitalized, 0 for lost, 1 for combat win, 2 for fuel/heat win
        public int gameTowerLevel = 1; // tower level
        public string battleLog = ""; // record the log of battle
        public List<bool> rewardBuffs = new List<bool>(10); // battle reward buffs
        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                GetComponent<AudioSource>().Play();
                DontDestroyOnLoad(instance);
            }
        }
    }
}