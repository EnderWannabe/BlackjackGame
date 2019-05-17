namespace BlackjackBackend
{
namespace App
{
    public class Deck 
    {
        System.Collections.Generic.List<int> _deck;
        int pointerToCurrentCard = 0;

        public Deck(int numDecks)
        {
            _deck = new System.Collections.Generic.List<int>();
            for (int i = 0; i < numDecks; i++)
            {
                for (int j = 0; j < 52; j++)
                {
                    _deck.Add(j);
                }
            }
            Shuffle(_deck);
        }

        void Shuffle(System.Collections.Generic.List<int> lst)
        {
            System.Random rng = new System.Random();
            int n = lst.Count;
            int value, k;
            while (n > 1) {
                n--;
                k = rng.Next(n + 1);
                value = lst[k];
                lst[k] = lst[n];
                lst[n] = value;
            }
        }

        public int NextCard
        {
            get 
            {
                if (pointerToCurrentCard > _deck.Count-1)
                {
                    throw new System.Exception("Deck count not high enough");
                }
                int card = _deck[pointerToCurrentCard];
                pointerToCurrentCard++;
                return card;
            }
        }
    }


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

    public class BlackjackModel
    {
        Dealer _currentPlayer;
        System.Collections.Generic.List<Dealer> _players;
        Deck _deck;
        System.Collections.Generic.Dictionary<int, Dealer> _idToDealer;

        int _userID;
        int _dealerID;

        ///<summary> Builds a new blackjack model </summary>
        ///<param name="numNPCS"> number of non human players, not including the dealer </param>
        ///<param name="userMoney"> amount of money the human player starts with</param>
        public BlackjackModel(int numNPCs, int userMoney)
        {
            _players = new System.Collections.Generic.List<Dealer>();
            _idToDealer = new System.Collections.Generic.Dictionary<int, Dealer>();
            _deck = new Deck(2 + numNPCs / 5);
            int i;
            System.Random rng = new System.Random();
            Dealer newPlayer;
            for (i = 0; i < numNPCs / 2 + 1; i++)
            {
                newPlayer = new Player(true, rng.Next(100, 500), i);
                _players.Add(newPlayer);
                _idToDealer.Add(i, newPlayer);
            }
            newPlayer = new Player(false, userMoney, i);
            _players.Add(newPlayer);
            _idToDealer.Add(i, newPlayer);
            _userID = i;
            for (int j = i; j < numNPCs; j++)
            {
                newPlayer = new Player(true, rng.Next(75, 500), j+1);
                _players.Add(newPlayer);
                _idToDealer.Add(j+1, newPlayer);
            }
            Dealer d = new Dealer(numNPCs + 1);
            _players.Add(d);
            _idToDealer.Add(numNPCs + 1, d);
            _dealerID = numNPCs+1;

            _currentPlayer = _players[0];
        }
        ///<summary> Deals a card to the Player with id "id".</summary>
        ///<param name="id"> A valid PlayerID</param>
        ///<returns> int value representing the card, 0-51 </returns> 
        public int DealCardToPlayer(int id)
        {
            Dealer p = _idToDealer[id];
            int card = _deck.NextCard;
            p.AddCard(card);
            return card;
        }

        public System.Collections.Generic.List<int> PlayerIds 
        { 
            get 
            { 
                return new System.Collections.Generic.List<int>(_idToDealer.Keys);
            }
        }

        public int UserID
        {
            get { return _userID; }
        }

        public int DealerID
        {
            get { return _dealerID; }
        }

        public int UserMoney
        {
            get { Player p = _idToDealer[_userID] as Player; return p.Money; }
        }

        ///<param name="id">A valid PlayerID</param>
        ///<returns> int value of how much money player with id id has </returns>
        public int GetMoney(int id)
        {
            Player p = _idToDealer[id] as Player; 
            return p.Money;
        }

        ///<summary>Gets the int value that Player ID will bet this round.</summary>
        ///<param name="id">A valid PlayerID</param>
        ///<exception>Throws ArgumentException if the PlayerID is valid or if the PlayerID corresponds to a human Player</exception>
        ///<exception>Throws ArgumentException if the PlayerID is not valid or if the PlayerID corresponds to a human Player</exception>
        ///<returns>int value of the PlayerID bet this round</returns>     
        public int GetNPCBet(int id)
        {
            Player npc = _idToDealer[id] as Player;
            if (npc == null)
            {
                throw new System.ArgumentException("id indicates its the dealer, who does not bet");
            }
            if (!npc.IsNPC)
            {
                throw new System.ArgumentException("id indicates user, not an NPC");
            }
            npc.SetBet((int) (npc.Money / 10));
            return npc.CurrentBet;
        }

        ///<param name="id">the user's id - will throw errors if npc id</param>
        ///<param name="bet">the user's bet for this round</param>
        ///<exception>Throws ArgumentException if the id does not point to the user</exception>
        ///<returns>true if the user bets that amount, false if the user cannot bet that amount</returns>     
        public bool SetUserBet(int bet)
        {
            Player user = _idToDealer[_userID] as Player;
            return user.SetBet(bet);
        }
        
        ///<summary> Ends the current round of the game, determining winners, resetting hands, and reshuffling deck</summary>
        ///<returns> Dictionary of Player IDs to if that player won/lost. true is won, false is lost </returns>  
        public System.Collections.Generic.Dictionary<int, bool> ResolveGame()
        {
            System.Collections.Generic.Dictionary<int, bool> returnDict = new System.Collections.Generic.Dictionary<int, bool>();
            int dealerCardsValue = DetermineHandValue(_players[_players.Count - 1].Hand);
            int playerCardsValue;
            foreach(int id in _idToDealer.Keys)
            {
                Dealer d = _idToDealer[id];
                Player p = d as Player;
                if (p == null) // it's the dealer, not a player
                {
                    d.ResetHand();
                }
                else // it's a player, figure out if they win
                {
                    playerCardsValue = DetermineHandValue(p.Hand);
                    bool won = playerCardsValue > dealerCardsValue;
                    returnDict.Add(id, won);
                    p.ResolveBet(won);
                    p.ResetHand();
                }
            }
            return returnDict;
        }

        // if they bust, returns -1
        private int DetermineHandValue(System.Collections.Generic.List<int> hand)
        {
            int total = 0;
            bool wentWithEleven = false;
            foreach(int card in hand)
            {
                int rank = (card % 13) + 1;
                if (rank > 9)
                {
                    total += 10;
                }
                else if (rank == 1)
                {
                    if (total + 11 <= 21)
                    {
                        total += 11;
                        wentWithEleven = true;
                    }
                    else 
                    {
                        total += 1;
                    }
                }
                else 
                {
                    total += rank;
                }
            }
            if (total > 21 && wentWithEleven)
            {
                total -= 10;
            }
            if (total > 21)
            {
                total = -1;
            }
            return total;

        }
        public static void Main()
        {
            
        }
    }
}
}