using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    private Vector3 _spawnPosition;
    private GameObject _cardObject;
    public bool faceUp;
    public bool animationEnabled;
    public Quaternion startingRotation;


    public Card(Vector3 spawnPosition, GameObject cardObject, bool faceUp)
    {
        _spawnPosition = spawnPosition;
        _cardObject = cardObject;
        this.faceUp = faceUp;
        animationEnabled = false;
        startingRotation = cardObject.transform.rotation;
    }

    public Vector3 StartPosition
    {
        get {return _spawnPosition; }
    }

    public GameObject CardObject
    {
        get {return _cardObject; }
    }
}


public class CardLERP : MonoBehaviour
{
    public GameObject spawnMarker;

    public GameObject destinationMarker;
    public float rotation;
    public bool dealCardAnimationEnabled;
    private List<Card> _cardList;
    private float _startTimeCardDeal;
    private Card _cardToMove;
    private Vector3 _offset;
    private Vector3 _destination;
    private float _journeyLength;
    private float _speed = 5f;
    public bool flipCardAnimation;
    private float _startTimeFlipCard;
    private Quaternion _flipRotation;

    void Start()
    {
        _cardList = new List<Card>();
        dealCardAnimationEnabled = false;
        flipCardAnimation = false;
        _flipRotation = Quaternion.Euler(-90,0,0) * Quaternion.Euler(0,0,rotation);
    }

    ///<summary> moves the card that was last given to the player
    /// to the playes hand location, destinationMarker</summary>
    public void RestartDealCardAnimation()
    {
        dealCardAnimationEnabled = true;
        _startTimeCardDeal = Time.time;

        _cardToMove = _cardList[_cardList.Count-1];

        _offset = new Vector3((float) 0.05*(_cardList.Count-1),0f,0f);
        _offset = Quaternion.Euler(0f,rotation,3f) * _offset;
        _destination = destinationMarker.transform.position + _offset;
        _journeyLength = Vector3.Distance(_cardToMove.StartPosition,_destination);
    }

    ///<summary> restarts an animation to flip all cards in the hand faceup </summary>
    public void RestartFlipCards()
    {
        flipCardAnimation = true;
        _startTimeFlipCard = Time.time;
    }

    ///<summary> instantiates a new card object associated to the player. card is instantiated 
    /// at the location of spawn marker </summary>
    ///<param name="cardPrefab"> an instance of the card to be instantiated</param>
    ///<param name="faceUp">true if card should be face up, otherwise false</param>
    public void GiveCardToPlayer(GameObject cardPrefab, bool faceUp)
    {
        GameObject tempCardObject = Instantiate(cardPrefab, this.transform, true);
        tempCardObject.transform.position = spawnMarker.transform.position;
        if(!faceUp)
        {
            tempCardObject.transform.Rotate(0f,180,0f,Space.Self);
            tempCardObject.transform.Rotate(0f, 0f, -rotation);
        }
        else
        {
            tempCardObject.transform.Rotate(0f, 0f, rotation);
        }

        Card newCard = new Card(spawnMarker.transform.position,tempCardObject, faceUp);

        _cardList.Add(newCard);
    }

    public void Update()
    {
        if(dealCardAnimationEnabled)
        {
            float distCovered = (Time.time - _startTimeCardDeal) * _speed;
            float fracJourney = (float) (distCovered / _journeyLength);

            _cardToMove.CardObject.transform.position = Vector3.Lerp(spawnMarker.transform.position,_destination, fracJourney);
            
            if(fracJourney >= 1)
            {
                dealCardAnimationEnabled = false;
            }
        }
        if(flipCardAnimation)
        {
            foreach(Card card in _cardList)
            {
                float timePassed = (Time.time - _startTimeFlipCard);
                card.CardObject.transform.rotation = Quaternion.Lerp(card.startingRotation,_flipRotation,timePassed);
                if(timePassed >= 1)
                {
                    flipCardAnimation = false;
                }
            }
        }
    }  

    ///<summary> clears the cards being displayed, deleting all card objects</summary> 
    public void DeleteAllCards()
    {
        foreach(Card i in _cardList)
        {
            Destroy(i.CardObject);
        }
        _cardList.Clear();
    }
}
