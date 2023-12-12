using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    public int progress;
    public TextMeshProUGUI progtext;
    // Start is called before the first frame update
    void Start()
    {
        progress = 0;
    }

    // Update is called once per frame
    void Update()
    {
        progtext.text = progress + "/20";
        if(progress >= 20)
        {
            progtext.text = "Win!!!";
        }
    }

    public void Increase()
    {
        progress++;
    }
}
