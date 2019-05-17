namespace BlackjackBackend
{
namespace App
{
    public class Dealer 
    {
        protected System.Collections.Generic.List<int> _hand;
        protected int _id;
        protected bool _isNPC;

        public Dealer(int id)
        {
            _hand = new System.Collections.Generic.List<int>();
            _id = id;
            _isNPC = true;
        }

        public void AddCard(int newCard)
        {
            _hand.Add(newCard);
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
    }

    public class Player : Dealer
    {
        int _money; // floats later for cent bets?
        int _currentBet;

        public Player(bool isNPC, int startingMoney, int id) : base(id)
        {
            _money = startingMoney;
            _currentBet = 0;
            _hand = new System.Collections.Generic.List<int>();
            _isNPC = isNPC;
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
                _money += 2 * _currentBet;
            }
            _currentBet = 0;
        }

        public int Money
        {
            get { return _money; }
        }

        public int CurrentBet { get { return _currentBet; } }
    }
}
}