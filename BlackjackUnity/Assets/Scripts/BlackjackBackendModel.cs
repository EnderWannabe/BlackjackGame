namespace BlackjackBackend
{
namespace App
{
    public class BlackjackModel
    {
        Dealer _currentPlayer;
        System.Collections.Generic.List<Dealer> _players;
        Deck _deck;
        System.Collections.Generic.Dictionary<int, Dealer> _idToDealer;

        int _userID;
        int _dealerID;

        ///<summary>Builds a new blackjack model </summary>
        ///<param name="numNPCS">number of non human players, not including the dealer </param>
        ///<param name="userMoney">amount of money the human player starts with</param>
        public BlackjackModel(int numNPCs, int userMoney)
        {
            _players = new System.Collections.Generic.List<Dealer>();
            _idToDealer = new System.Collections.Generic.Dictionary<int, Dealer>();
            _deck = new Deck(2 + numNPCs / 5);
            int i;
            System.Random rng = new System.Random();
            Dealer newPlayer;
            for (i = 0; i < (numNPCs +1) / 2; i++)
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

        ///<summary>Deals a card to the Player with id "id".</summary>
        ///<param name="id">A valid PlayerID</param>
        ///<exception>throws System.Exception if the player to hit has already busted</exception>
        ///<returns>a tuple between the card, 0-51, and a bool representing if the card is faceup </returns> 
        public System.Tuple<int, bool> DealCardToPlayer(int id)
        {
            Dealer p = _idToDealer[id];
            if (DetermineHandValue(p.Hand) > 21)
            {
                throw new System.Exception("We should not be dealing cards to player's who have already busted");
            }
            int card = _deck.NextCard;
            bool faceup = true;
            if (p.Hand.Count == 0)
            {
                faceup = false;
            }
            p.AddCard(card);
            return System.Tuple.Create(card, faceup);
        }

        ///<param name="id">A valid PlayerID</param>
        ///<returns>how much money player with id "id" has </returns>
        public int GetMoney(int id)
        {
            Player p = _idToDealer[id] as Player; 
            return p.Money;
        }

        ///<summary>Gets the int value that Player ID will bet this round.</summary>
        ///<param name="id">A valid PlayerID</param>
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
        ///<returns>true if the user bets that amount, false if the user cannot bet that amount</returns>     
        public bool SetUserBet(int bet)
        {
            Player user = _idToDealer[_userID] as Player;
            return user.SetBet(bet);
        }

        /// <param name="id">the id of the NPC</param>
        /// <exception>throws argumentexception if the given id is not an npc's</exception>
        /// <returns>a card if npc hits, -1 if npc stands</returns>
        public int NPCTakeTurn(int id)
        {
            if (id == _dealerID || id == _userID)
            {
                throw new System.ArgumentException("Given ID does not represent an NPC.");
            }
            Player npc = _idToDealer[id] as Player;
            int handValue = DetermineHandValue(npc.Hand);
            if (handValue >= 16 || handValue == -1)
            {
                return -1;
            }
            int card = _deck.NextCard;
            npc.AddCard(card);
            return card;
        }

        /// <returns>a card if the dealer hits, -1 if dealer stands</returns>
        public int DealerTakeTurn()
        {
            Dealer dealer = _idToDealer[_dealerID];
            int handValue = DetermineHandValue(dealer.Hand);
            if (handValue >= 16 || handValue == -1)
            {
                return -1;
            }
            int card = _deck.NextCard;
            dealer.AddCard(card);
            return card;
        }

        public bool UserBusted()
        {
            Player user = _idToDealer[_userID] as Player;
            return DetermineHandValue(user.Hand) == -1;
        }
        
        /// <summary>Ends the current round of the game, determining winners, resetting hands, and reshuffling deck</summary>
        /// <returns>Dictionary of Player IDs to if that player won/lost. true is won, false is lost </returns>  
        public System.Collections.Generic.Dictionary<int, bool> ResolveGame()
        {
            System.Collections.Generic.Dictionary<int, bool> returnDict = new System.Collections.Generic.Dictionary<int, bool>();
            int dealerCardsValue = DetermineHandValue(_idToDealer[_dealerID].Hand);
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
            _deck.Reset();
            return returnDict;
        }

        public System.Collections.Generic.List<int> GetCardsInHand(int id)
        {
            return _idToDealer[id].Hand;
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