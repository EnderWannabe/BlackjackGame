using System.Collections.Generic;
using BlackjackBackend.App;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    enum GameState
    {
        BET,
        DEAL,
        PLAY,
        RESOLVE,
        CLEAR
    };

    enum BetState
    {
        PLAYER,
        AI,
        ANIMATE
    };

    enum DealState
    {
        GETCARDS,
        ANIMATE
    };

    enum PlayState 
    {
        WHOSETURN,
        NPC,
        PLAYER,
        DEALER,
        ANIMATE
    }

    public Canvas betUI;
    public GameObject peekHand;
    public GameObject playerSeat;
    public List<GameObject> aiSeats;
    public GameObject whiteChipPrefab;
    public GameObject redChipPrefab;
    public GameObject blueChipPrefab;
    public GameObject greenChipPrefab;
    public GameObject blakcChipPrefab;
    public GameObject pool;
    public List<GameObject> cardPrefabList;
    public List<GameObject> aiHandList;
    public GameObject playerHandMarker;
    public GameObject dealerHandMarker;
    public GameObject spawnMarker;
    public EndRoundPopUp endRoundPopUp;
    private BlackjackModel _blackjackModel;
    private GameState _gameState;
    private BetState _betState;
    private DealState _dealState;
    private PlayState _playState;
    private PlayState _previousPlayStateReference;
    private List<int> _betList;
    private List<System.Tuple<int, bool>> _cardList;

    private int _numNPCs;
    private int _userMoney;
    private int _currentPlayerID;
    private float _startTime;

    private List<ChipLERP> _chipManagerList;

    private List<GameObject> _activeSeats;
    private List<CardLERP> _cardManagerList;
    private bool _dealCardAnimateBool;
    private int _dealCardAnimateInt;
    private int _currentCard;
    private float _cardOffset;
    private bool _resolveBetsHelperBool;
    private bool _animateBetsHelperBool;
    private bool _pauseAfterWinHelperBool;
    private Dictionary<int,bool> wonLostDict;
    private bool _pauseAfterWinHelperBool2;

    void Start() 
    {
        _numNPCs = SharedData.NumberNPC;
        _userMoney = SharedData.Money;
        _blackjackModel = new BlackjackModel(_numNPCs, _userMoney);
        _gameState = GameState.BET;
        _betState = BetState.PLAYER;
        _dealState = DealState.GETCARDS;
        _playState = PlayState.WHOSETURN;
        _cardList = new List<System.Tuple<int,bool>>();
        _betList = new List<int>();
        _currentPlayerID = -1;
        foreach(int j in _blackjackModel.PlayerIds) 
        {
            _betList.Add(-1);
        }
        _activeSeats = new List<GameObject>();
        InitializeSeatList();
        _chipManagerList = new List<ChipLERP>();
        foreach(int i in _blackjackModel.PlayerIds)
        {
            if(i != _blackjackModel.DealerID)
            {
                _chipManagerList.Add(_activeSeats[i].GetComponent<ChipLERP>());
            }
        }

        for(int j = 0; j < aiSeats.Count; j++)
        {
            if(j < aiSeats.Count / 2)
            {
                AddCardManager(aiSeats[j], aiHandList[j], -60 + 20 * j);
            }
            else
            {
                AddCardManager(aiSeats[j], aiHandList[j], -60 + 20 *(j + 1));
            }
        }
        AddCardManager(playerSeat, playerHandMarker, 0);
        AddCardManager(dealerHandMarker, dealerHandMarker, 0);
        _dealCardAnimateBool = true;
        _dealCardAnimateInt = 0;
        _cardManagerList = new List<CardLERP>();
        foreach(int i in _blackjackModel.PlayerIds)
        {
            if(i != _blackjackModel.DealerID && i != _blackjackModel.UserID)
            {
                _cardManagerList.Add(_activeSeats[i].GetComponent<CardLERP>());
            }
            else if(i == _blackjackModel.UserID)
            {
                _cardManagerList.Add(playerSeat.GetComponent<CardLERP>());
            }
            else
            {
                _cardManagerList.Add(dealerHandMarker.GetComponent<CardLERP>());
            }
        }

        for(int j = 0; j < aiSeats.Count; j++)
        {
            if(j < aiSeats.Count / 2)
            {
                aiSeats[j].transform.Rotate(0f,(-60 + j * 20), 0f);
            }
            else
            {
                aiSeats[j].transform.Rotate(0f,(-60 +(j + 1) * 20), 0f);
            }
        }

        peekHand.SetActive(false);
        _cardOffset = 0f;

        _currentCard = 0;

        _resolveBetsHelperBool = true;

        _animateBetsHelperBool = true;

        _pauseAfterWinHelperBool = true;

        wonLostDict = new Dictionary<int, bool>();

        _pauseAfterWinHelperBool2 = true;
    }

    void Update()
    {
        if(Input.GetKey("escape"))
        {   
            // actually bring up menu and pause game instead of just quit
            Application.Quit();
        }
        PlayerActions cs = betUI.GetComponent<PlayerActions>();
        cs._playerTotalMoney.text = "Player Holdings: " + _blackjackModel.UserMoney.ToString();
        if (Input.GetKey("space"))
        {
            peekHand.SetActive(true);
        }
        else
        {
            peekHand.SetActive(false);
            switch(_gameState)
            {
            case GameState.BET:
                InitializeBets();
                break;
            case GameState.DEAL:
                DealCards();
                break;
            case GameState.PLAY:
                PlayRound();
                break;
            case GameState.RESOLVE:
                ResolveBets();
                break;
            case GameState.CLEAR:
                ClearTable();
                break;
            }
        }
    }

    void InitializeBets()
    {
        PlayerActions cs = betUI.GetComponent<PlayerActions>();
        int playerbet = cs._betAmount;
        switch(_betState)
        {
        case BetState.PLAYER:
            cs._betInput.gameObject.SetActive(true);
            cs._betButton.gameObject.SetActive(true);
            if(_blackjackModel.SetUserBet(playerbet))
            {
                _betList[_blackjackModel.UserID] = playerbet;
                _betState = BetState.AI;
                cs._betInput.gameObject.SetActive(false);
                cs._betButton.gameObject.SetActive(false);
                cs._betAmount = -1;
            }
            break;
        case BetState.AI:
            foreach(int i in _blackjackModel.PlayerIds)
            {
                // This is for animation state
                if(i != _blackjackModel.DealerID)
                {
                    if(i != _blackjackModel.UserID)
                    {
                        _betList[i] = _blackjackModel.GetNPCBet(i);
                    }
                    _chipManagerList[i].CalculateChipCluster(_betList[i], _blackjackModel.GetMoney(i)); //calculate AI bet cluster
                }
            }
            _betState = BetState.ANIMATE;
            _startTime = Time.time;
            break;
        case BetState.ANIMATE:
            if(_animateBetsHelperBool)
            {
                foreach(ChipLERP chipLerper in _chipManagerList)
                {
                    chipLerper.RestartAnimateBet();
                    _animateBetsHelperBool = false;
                }
            }
            else 
            {
                bool doneBool = true;
                foreach(ChipLERP chipLerper in _chipManagerList)
                {
                    doneBool = doneBool && !chipLerper.animateBetBool;
                }
                if(doneBool)
                {
                    _betState = BetState.PLAYER;
                    _gameState = GameState.DEAL;
                    _animateBetsHelperBool = true;
                }
            }
            break;
        default:
            throw new System.Exception("_betState state has been thrown off. It is currently: " + _betState);
        }
    }

    void DealCards()
    {
        switch(_dealState)
        {
        case DealState.GETCARDS:
            System.Tuple<int, bool> card;
            foreach(int i in _blackjackModel.PlayerIds)
            {
                for(int j = 0; j < 2; j++)
                {
                    card = _blackjackModel.DealCardToPlayer(i);
                    if (i == _blackjackModel.UserID)
                    {
                        AddCardToPeekHand(card.Item1);
                    }
                    _cardList.Add(card);
                }
            }
            _dealState = DealState.ANIMATE;
            break;
        case DealState.ANIMATE:
            int currentPlayer = _dealCardAnimateInt / 2; //every player gets two cards
            if(_dealCardAnimateBool)
            {
                _cardManagerList[currentPlayer].GiveCardToPlayer(cardPrefabList[_cardList[_dealCardAnimateInt].Item1], _cardList[_dealCardAnimateInt].Item2);
                _dealCardAnimateBool = false;
                _cardManagerList[currentPlayer].RestartDealCardAnimation();
            }
            else
            {
                if(!_cardManagerList[currentPlayer].dealCardAnimationEnabled)
                {
                    _dealCardAnimateBool = true;
                    _dealCardAnimateInt++;
                    if(_dealCardAnimateInt == _cardList.Count)
                    {
                        _dealState = DealState.GETCARDS;
                        _gameState = GameState.PLAY;
                        _dealCardAnimateInt = 0;
                        _cardList.Clear();
                    }
                }
            }
            break;
        default:
            throw new System.Exception("_dealState state has been thrown off. It is currently: " + _dealState);
        }
    }

    void PlayRound()
    {
        int result;
        switch(_playState)
        {
            case PlayState.WHOSETURN:
                List<int> ids = _blackjackModel.PlayerIds;
                if(_currentPlayerID == -1)
                {
                    _currentPlayerID = ids[0];
                }
                else
                {
                    int curIndex = ids.IndexOf(_currentPlayerID);
                    if(curIndex == ids.Count - 1)
                    {
                        _currentPlayerID = -1;
                        _gameState = GameState.RESOLVE;
                    }
                    else
                    {
                        _currentPlayerID = ids[curIndex + 1];
                        if(_currentPlayerID == _blackjackModel.UserID)
                        {
                            _playState = PlayState.PLAYER;
                        }
                        else if(_currentPlayerID == _blackjackModel.DealerID)
                        {
                            _playState = PlayState.DEALER;
                        }
                        else
                        {
                            _playState = PlayState.NPC;
                        }
                    }
                }
                break;
            case PlayState.NPC:
                result = _blackjackModel.NPCTakeTurn(_currentPlayerID);
                if(result == -1) // npc stands
                {
                    _playState = PlayState.WHOSETURN;
                    break;
                }
                _currentCard = result;
                _previousPlayStateReference = PlayState.NPC;
                _playState = PlayState.ANIMATE;
                break;
            case PlayState.DEALER:
                result = _blackjackModel.DealerTakeTurn();
                if(result == -1) // npc stands
                {
                    _playState = PlayState.WHOSETURN;
                    break;
                }
                _currentCard = result;
                _previousPlayStateReference = PlayState.DEALER;
                _playState = PlayState.ANIMATE;
                break;
            case PlayState.PLAYER:
                PlayerActions cs = betUI.GetComponent<PlayerActions>();
                if(_blackjackModel.UserBusted())
                {
                    cs.state = PlayerActions.PlayerHitStand.DONOTHING;
                    _playState = PlayState.WHOSETURN;
                    break;
                }
                cs._hitButton.gameObject.SetActive(true);
                cs._standButton.gameObject.SetActive(true);
                switch(cs.state)
                {
                    case PlayerActions.PlayerHitStand.HIT:
                        _currentCard = _blackjackModel.DealCardToPlayer(_blackjackModel.UserID).Item1;
                        cs.state = PlayerActions.PlayerHitStand.DONOTHING;
                        cs._hitButton.gameObject.SetActive(false);
                        cs._standButton.gameObject.SetActive(false);

                        AddCardToPeekHand(_currentCard);
                        peekHand.transform.Translate(0.05f, 0f, 0f);

                        _previousPlayStateReference = PlayState.PLAYER;
                        _playState = PlayState.ANIMATE;
                        break;
                    case PlayerActions.PlayerHitStand.STAND:
                        cs._hitButton.gameObject.SetActive(false);
                        cs._standButton.gameObject.SetActive(false);
                        cs.state = PlayerActions.PlayerHitStand.DONOTHING;
                        _playState = PlayState.WHOSETURN;
                        break;
                    case PlayerActions.PlayerHitStand.DONOTHING:
                        break;
                    default:
                        throw new System.Exception("PlayerActions.PlayerHitStand state has been thrown off. It is currently: " + cs.state);
                }
                break;
            case PlayState.ANIMATE:
                if(_dealCardAnimateBool)
                {
                    _cardManagerList[_currentPlayerID].GiveCardToPlayer(cardPrefabList[_currentCard],true);
                    _dealCardAnimateBool = false;
                    _cardManagerList[_currentPlayerID].RestartDealCardAnimation();
                }
                else
                {
                    if(!_cardManagerList[_currentPlayerID].dealCardAnimationEnabled)
                    {
                        _dealCardAnimateBool = true;
                        _playState = _previousPlayStateReference;
                    }
                }
                break;
            default:
                throw new System.Exception("_playState has been thrown off. It is currently: " + _playState);
        }
    }

    void ResolveBets()
    {
        if(_pauseAfterWinHelperBool)
        {
            if(_resolveBetsHelperBool)
            {
                foreach(CardLERP lerp in _cardManagerList)
                {
                    lerp.RestartFlipCards();
                }
                _resolveBetsHelperBool = false;
            }
            else
            {
                bool doneBool = true;
                foreach(CardLERP lerp in _cardManagerList)
                {
                    doneBool = doneBool && !lerp.flipCardAnimation;
                }
                if(doneBool)
                {
                    _resolveBetsHelperBool = true;
                    wonLostDict = _blackjackModel.ResolveGame();
                    _pauseAfterWinHelperBool = false;
                }
            }
        }
        else
        {
            if(_pauseAfterWinHelperBool2)
            {
                endRoundPopUp.gameObject.SetActive(true);
                if(wonLostDict[_blackjackModel.UserID])
                {
                    endRoundPopUp.displayString = "You won!";
                }
                else
                {
                    endRoundPopUp.displayString = "You lost :(";
                }
                _pauseAfterWinHelperBool2 = false;
            }
            if(endRoundPopUp.clickedOn)
            {
                endRoundPopUp.clickedOn = false;
                _gameState = GameState.CLEAR;
                _pauseAfterWinHelperBool = true;
                _pauseAfterWinHelperBool2 = true;
                endRoundPopUp.gameObject.SetActive(false);
            }
        }
    }

    void ClearTable()
    {
        foreach(int id in _blackjackModel.PlayerIds)
        {
            if(id != _blackjackModel.DealerID)
            {
                _chipManagerList[id].TotalReset(_blackjackModel.GetMoney(id));
            }
        }
        foreach(CardLERP lerp in _cardManagerList)
        {
            lerp.DeleteAllCards();
        }
        ResetPeekHand();
        _gameState = GameState.BET;
        if(_blackjackModel.UserMoney == 0)
        {
            Debug.Log("Mickey give a dialog here saying you lost GG with option to quit app or go back to main menu?");
            Application.Quit();
        }
    }

    void SpawnChips(GameObject seat, int money)
    {
        ChipLERP chipManager = seat.AddComponent<ChipLERP>() as ChipLERP;
        chipManager.whiteChip = whiteChipPrefab;
        chipManager.redChip = redChipPrefab;
        chipManager.blueChip = blueChipPrefab;
        chipManager.greenChip = greenChipPrefab;
        chipManager.blackChip = blakcChipPrefab;
        chipManager.pool = pool;
        chipManager.InitializeChipStack(money);
    }

    void AddCardManager(GameObject seat, GameObject destinationMarker, float rotation)
    {
        CardLERP cardManager = seat.AddComponent<CardLERP>() as CardLERP;
        cardManager.spawnMarker = spawnMarker;
        cardManager.destinationMarker = destinationMarker;
        cardManager.rotation = rotation;
    }

    void InitializeSeatList()
    {
        List<int> whichSeats;
        switch(_numNPCs)
        {
        case 1:
            whichSeats = new List<int>{1};
            break;
        case 2:
            whichSeats = new List<int>{1, 4};
            break;
        case 3:
            whichSeats = new List<int>{0, 2, 3};
            break;
        case 4:
            whichSeats = new List<int>{0, 2, 3, 5};
            break;
        case 5:
            whichSeats = new List<int>{0, 1, 2, 3, 5};
            break;
        case 6:
            whichSeats = new List<int>{0, 1, 2, 3, 4, 5};
            break;
        default:
            throw new System.ArgumentException("Num NPCs was set incorrectly.");
        }

        int currentPlayerIndex = 0, currentPlayerID;
        List<int> ids = _blackjackModel.PlayerIds;
        foreach (int aiSeatIndex in whichSeats)
        {
            currentPlayerID = ids[currentPlayerIndex];
            if (currentPlayerID == _blackjackModel.UserID) // skip over the user
            {
                currentPlayerIndex++;
                currentPlayerID = ids[currentPlayerIndex];
            }
            currentPlayerIndex++;

            _activeSeats.Add(aiSeats[aiSeatIndex]);
            SpawnChips(aiSeats[aiSeatIndex], _blackjackModel.GetMoney(currentPlayerID));
        }

        int playerSeatIndex = (int)System.Math.Ceiling((double)_numNPCs/(double)2);
        _activeSeats.Insert(playerSeatIndex, playerSeat);
        SpawnChips(playerSeat, _userMoney);
    }

    void AddCardToPeekHand(int card)
    {
        Vector3 cardPosition = peekHand.transform.position;
        cardPosition.x += _cardOffset;
        GameObject cardObject = Instantiate(cardPrefabList[card], cardPosition, Quaternion.identity, peekHand.transform);
        cardObject.transform.Rotate(-90f, 0f, 0f);
        _cardOffset -= 0.1f;
    }

    void ResetPeekHand()
    {
        peekHand.transform.SetPositionAndRotation(new Vector3(0.05f, 2.69f, 0.45f), Quaternion.identity);
        _cardOffset = 0;
        peekHand.SetActive(false);
        foreach(Transform child in peekHand.transform)
        {
            Destroy(child.gameObject);
        }
    }
}