using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
public class PlayerActions : MonoBehaviour
{
    public InputField _betInput;
    public Text _playerTotalMoney;
    public Button _betButton;
    public Button _hitButton;
    public Button _standButton;
    public bool show = false;
    public string error;
    public Rect windowRect = new Rect (1000, 1000, 400, 300);
    public enum PlayerHitStand
    {
        HIT, 
        STAND,
        DONOTHING
    };

    int _playerHoldings;
    public int _betAmount;
    public PlayerHitStand state;

    // Start is called before the first frame update
    void Start()
    {
        _playerTotalMoney = transform.Find("BetUI/Text").gameObject.GetComponent<Text>(); 
        _betButton.onClick.AddListener(BetHandler);
        _playerTotalMoney.gameObject.SetActive(true);
        _standButton.onClick.AddListener(StandHandler);
        _hitButton.onClick.AddListener(HitHandler);
        state = PlayerHitStand.DONOTHING;
    }

    void BetHandler()
    {
        if (Int32.TryParse(_betInput.text, out _betAmount))
        {
            if (_betAmount <= 0)
            {
                _betAmount = -1;
                error = "Please enter an amount greater than zero.";
                show = true;
                // UnityEditor.EditorUtility.DisplayDialog("Invalid input","Please enter an amount greater than zero.", "Ok");
            }
            else 
            {
                // We use split to seperate the string between the statement 'Player Holdings' and the player holding amount
                // index [1] contains the string that represents the players actual holdings
                Int32.TryParse(_playerTotalMoney.text.Split(':')[1], out _playerHoldings);
                if (_betAmount > _playerHoldings)
                {
                    _betAmount = -1;
                    error = "Please enter a number less than player holdings.";
                    show = true;
                    // UnityEditor.EditorUtility.DisplayDialog("Invalid input","Please enter a number less than player holdings.", "Ok");
                }
            }
        }
        //if player enters an incorrect input, displays an error dialog box
        else
        {
            _betAmount = -1;
            // EditorUtility.DisplayDialog("Invalid input","Please enter a valid number amount.", "Ok");
        }
    }
    void OnGUI () 
    {
        if(show)
            windowRect = GUI.Window (0, windowRect, DialogWindow, error);
    }

    // This is the actual window.
    void DialogWindow (int windowID)
    {
        if(GUI.Button(new Rect(5,20, windowRect.width - 10, 20), "Ok"))
        {
           show = false;
        }
    }

    void StandHandler()
    {
        state = PlayerHitStand.STAND;
    }

    void HitHandler()
    {
        state = PlayerHitStand.HIT;
    }
}