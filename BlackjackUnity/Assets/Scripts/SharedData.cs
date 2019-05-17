public class SharedData
{
    private static int _numNPCs, _userMoney;

    public static int NumberNPC
    {
        get { return _numNPCs; }
        set { _numNPCs = value; }
    }

    public static int Money
    {
        get { return _userMoney; }
        set { _userMoney = value; }
    }
}