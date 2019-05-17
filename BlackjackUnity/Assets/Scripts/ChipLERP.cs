using System.Collections.Generic;
using UnityEngine;

public class Chip 
{
    private int _chipStackAmt;
    private int _chipBetAmt;
    private int _chipValue;
    private float _chipJourneyLength;
    private Vector3 _chipPosition;
    private GameObject _chipPrefab;
    private GameObject _chipStackObj;
    private GameObject _chipBetObj;

    public Chip(int chipValue, Vector3 chipPosition, GameObject prefab)
    {
        _chipValue = chipValue;
        _chipPosition = chipPosition;
        _chipPrefab = prefab;
        _chipBetAmt = 0;
        _chipStackAmt = 0;
    }

    public int ChipStackAmt
    {
        get {return _chipStackAmt; }
        set {_chipStackAmt = value; }
    }

    public int ChipBetAmt
    {
        get {return _chipBetAmt; }
        set {_chipBetAmt = value; }
    }

    public int ChipValue
    {
        get {return _chipValue; }
    }

    public float ChipJourneyLength
    {
        get {return _chipJourneyLength; }
        set {_chipJourneyLength = value; }
    }

    public Vector3 ChipPosition
    {
        get {return _chipPosition; }
    }

    public GameObject ChipPrefab
    {
        get {return _chipPrefab; }
    }

    public GameObject ChipStackObj
    {
        get {return _chipStackObj; }
        set {_chipStackObj = value; }
    }

    public GameObject ChipBetObj
    {
        get {return _chipBetObj; }
        set {_chipBetObj = value; }
    }
}

public class ChipLERP : MonoBehaviour
{   
    public bool animateBetBool;
    private const int WHITECHIPVALUE = 1;
    private const int REDCHIPVALUE = 5;
    private const int BLUECHIPVALUE = 10;
    private const int GREENCHIPVALUE = 25;
    private const int BLACKCHIPVALUE = 100;
    private float _speed = 1.0f;
    public GameObject whiteChip;
    public GameObject redChip;
    public GameObject blueChip;
    public GameObject greenChip;
    public GameObject blackChip;
    public GameObject pool;
    private Chip _whiteChip;
    private Chip _redChip;
    private Chip _blueChip;
    private Chip _greenChip;
    private Chip _blackChip;
    private List<Chip> _chipList;
    private float startTime;

    public List<Chip> ChipList
    {
        get {return _chipList; }
    }

    public void Start()
    {
        animateBetBool = false;
    }

    ///<summary> animates moving the bet foward </summary>
    public void RestartAnimateBet()
    {
        startTime = Time.time;
        animateBetBool = true;
    }

    public void Update()
    {
        if(animateBetBool)
        {
            float distCovered;
            float fracJourney;
            bool doneBool = true;
            foreach(Chip chip in _chipList)
            {
                distCovered = (Time.time - startTime)*_speed;
                fracJourney = distCovered/chip.ChipJourneyLength;
                if(fracJourney < 0.25)
                {
                    chip.ChipBetObj.transform.position = Vector3.Lerp(chip.ChipPosition,pool.transform.position,fracJourney);
                    doneBool = false;
                }
            }
            if(doneBool)
            {
                animateBetBool = false;
            }
        }
    }

    ///<summary> deletes all bet chips and sets the players money to currentMoney </summary>
    ///<param name="currentMoney"> the amount of money the player currently has. Is used to calculate the hieght of chip stacks </param>
    public void TotalReset(int currentMoney)
    {
        foreach(Chip chip in _chipList)
        {
            chip.ChipBetAmt = 0;
            Destroy(chip.ChipBetObj);
            chip.ChipStackAmt = currentMoney / chip.ChipValue;
            currentMoney -= chip.ChipStackAmt * chip.ChipValue;
            chip.ChipStackObj.transform.localScale = new Vector3(2f, 2f, chip.ChipStackAmt);
            if(chip.ChipStackAmt == 0)
            {
                chip.ChipStackObj.SetActive(false);
            }
            else
            {
                chip.ChipStackObj.SetActive(true);
            }
        }
    }

    ///<summary> creates bet chip objects and resizes the chip stacks the user has </summary>
    ///<param name="betAmount"> the amount the player is betting </param>
    ///<param name="currentMoney"> the amount of money the player has </param>
    public void CalculateChipCluster(int betAmount, int currentMoney)
    {
        foreach(Chip chip in _chipList)
        {
            chip.ChipBetAmt = (int) betAmount / chip.ChipValue;
            betAmount -= chip.ChipBetAmt * chip.ChipValue;

            chip.ChipBetObj = Instantiate(chip.ChipPrefab, chip.ChipPosition, Quaternion.identity, this.transform);
            chip.ChipBetObj.transform.localScale = new Vector3(2f, 2f, chip.ChipBetAmt);
            chip.ChipBetObj.transform.Rotate(-90f, 0f, 0f, Space.Self);
            if(chip.ChipBetAmt <= 0)
            {
                chip.ChipBetObj.SetActive(false);
            }
        }
        foreach(Chip chip in _chipList)
        {
            chip.ChipStackAmt = currentMoney / chip.ChipValue;
            currentMoney -= chip.ChipStackAmt * chip.ChipValue;
            chip.ChipStackObj.transform.localScale = new Vector3(2f, 2f, chip.ChipStackAmt);
            if(chip.ChipStackAmt == 0)
            {
                chip.ChipStackObj.SetActive(false);
            }
            else
            {
                chip.ChipStackObj.SetActive(true);
            }
        }
        
    }

    ///<summary> creates the stacks of chips the player has </summary>
    ///<param name="initialAmount"> the amount of money the player starts out with </param>
    public void InitializeChipStack(int initialAmount)
    {
        _whiteChip = new Chip(WHITECHIPVALUE, new Vector3(0.04f, 0f, 0.02f) + this.transform.position, whiteChip);
        _redChip = new Chip(REDCHIPVALUE, new Vector3(-0.03f, 0f, 0.025f) + this.transform.position, redChip);
        _blueChip = new Chip(BLUECHIPVALUE, new Vector3(0.075f, 0f, 0.075f) + this.transform.position, blueChip);
        _greenChip = new Chip(GREENCHIPVALUE, new Vector3(-0.075f, 0f, 0.075f) + this.transform.position, greenChip);
        _blackChip = new Chip(BLACKCHIPVALUE, new Vector3(0f, 0f, 0.075f) + this.transform.position, blackChip);

        _chipList = new List<Chip>();
        _chipList.Add(_blackChip);
        _chipList.Add(_greenChip);
        _chipList.Add(_blueChip);
        _chipList.Add(_redChip);
        _chipList.Add(_whiteChip);

        int _initialAmount = initialAmount;

        // Initializing chip stacks to their proper values
        foreach(Chip chip in _chipList)
        {
            chip.ChipStackAmt = _initialAmount / chip.ChipValue;
            _initialAmount -= chip.ChipStackAmt * chip.ChipValue;

            chip.ChipStackObj = Instantiate(chip.ChipPrefab, chip.ChipPosition, Quaternion.identity, this.transform);
            chip.ChipStackObj.transform.localScale = new Vector3(2f, 2f, chip.ChipStackAmt);
            chip.ChipStackObj.transform.Rotate(-90f, 0f, 0f, Space.Self);

            chip.ChipJourneyLength = Vector3.Distance(chip.ChipPosition, pool.transform.position);
            if(chip.ChipStackAmt <= 0)
            {
                chip.ChipStackObj.SetActive(false);
            }
        }

    }
}
