using UnityEngine;
using UnityEngine.UI;
using System;
public class GameSetup : MonoBehaviour
{
    public Button _backButton;
    public Text _startingAmount;
    public Text _presetStartingAmount;
    public Dropdown _NPCs;

    int _amount;
    int _numNPCs;
    // Start is called before the first frame update
    void Start()
    {
        //manually preseting values for the case that the player doesn't decide to change options
        SharedData.NumberNPC = 2;
        SharedData.Money = 200;
        _backButton.onClick.AddListener(OptionsHandler);
    }

    void OptionsHandler()
    {
        //we add 1 to NPCs.value as it is a list and indexes starting at 0;
        _numNPCs = _NPCs.value + 1;
        //checks if user entered input for starting players amounts
        if (_startingAmount.text != "")
        {
            //checks if player entered in number not letters
            if(!Int32.TryParse(_startingAmount.text, out _amount))
            {
                _amount = 200;
                // Int32.TryParse(_presetStartingAmount.text, out _amount);
            }
            // if user enters an int amount but it is less than zero, return preset amoount
            if(_amount < 0)
            {
                _amount = 200;
                // Int32.TryParse(_presetStartingAmount.text, out _amount);
            }
        }
        else
        {
            _amount = 200;
        }
        SharedData.NumberNPC = _numNPCs;
        SharedData.Money = _amount;

    }
}
