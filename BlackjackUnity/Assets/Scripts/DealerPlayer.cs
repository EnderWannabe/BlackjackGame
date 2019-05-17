namespace BlackjackBackend
{
namespace App
{
    public class Dealer 
    {
        protected System.Collections.Generic.List<int> _hand;
        protected int _id;
        protected bool _isNPC;
        protected System.Collections.Generic.List<int> _secondHand;
        protected bool _isSplitting;

        public Dealer(int id)
        {
            _hand = new System.Collections.Generic.List<int>();
            _id = id;
            _isNPC = true;
            _secondHand = new System.Collections.Generic.List<int>();
            _isSplitting = false;
        }

        public void AddCard(int newCard, int whichHand = 1)
        {
            if(whichHand == 1)
            {
                _hand.Add(newCard);
            }
            else
            {
                _secondHand.Add(newCard);
            }
        }

        public void ResetHand()
        {
            _hand = new System.Collections.Generic.List<int>();
        }

        public System.Collections.Generic.List<int> Hand
        {
            get { return new System.Collections.Generic.List<int>(_hand); }
        }

        public int Id
        {
            get { return _id; }
        }

        public bool IsNPC
        {
            get { return _isNPC; }
        }

        public bool IsSplitting {get {return _isSplitting;}}
    }

    public class Player : Dealer
    {
        int _money; // floats later for cent bets?
        int _currentBet;
        int _secondBet;

        public Player(bool isNPC, int startingMoney, int id) : base(id)
        {
            _money = startingMoney;
            _currentBet = 0;
            _hand = new System.Collections.Generic.List<int>();
            _isNPC = isNPC;
            _isSplitting = false;
            _secondHand = new System.Collections.Generic.List<int>();
            _secondBet = 0;
        }

        public bool SetBet(int bet)
        {
            if (_currentBet != 0)
            {
                throw new System.InvalidOperationException("current bet was not zeroed out before new round");
            }
            if (bet > _money || bet < 0)
            {
                return false;
            }
            _currentBet = bet;
            _money -= bet;
            return true;
        }

        public void ResolveBet(bool won)
        {
            if (won)
            {
                _money += 2 * (_currentBet + _secondBet);
            }
            _currentBet = 0;
            _secondBet = 0;
        }

        public void Split()
        {
            if(_hand.Count != 2)
            {
                throw new System.InvalidOperationException("player cannot split");
            }
            _secondHand.Add(_hand[1]);
            _hand.RemoveAt(1);
            _secondBet = _currentBet;
            _money -= _secondBet;
            _isSplitting = true;
        }

        public int Money
        {
            get { return _money; }
        }

        public int CurrentBet { get { return _currentBet; } }

    }
}
}