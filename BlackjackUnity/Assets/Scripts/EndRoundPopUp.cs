using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundPopUp : MonoBehaviour
{
    public bool clickedOn;
    public string displayString;
    public Button okButton;
    public Text textDisplay;
    // Start is called before the first frame update
    void Start()
    {
        clickedOn = false;
        okButton.onClick.AddListener(ClickedOnHandler);
    }

    void ClickedOnHandler()
    {
        clickedOn = true;
    }

    void Update()
    {
        textDisplay.text = displayString;
    }
}
