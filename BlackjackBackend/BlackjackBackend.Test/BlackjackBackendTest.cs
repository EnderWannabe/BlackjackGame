namespace BlackjackBackend
{
namespace Test
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ModelTests
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestNextCard()
        {
            BlackjackBackend.App.Deck d = new BlackjackBackend.App.Deck(1);
            System.Collections.Generic.HashSet<int> alreadySeen = new System.Collections.Generic.HashSet<int>();
            int card;
            for (int i = 0; i < 52; i++)
            {
                card = d.NextCard;
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(card >= 0 && card <= 51, "Expected the card to be an int between 0 and 51");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(alreadySeen.Contains(card), "expected only 1 of each card");
                alreadySeen.Add(card);
            }
            try 
            {
                int x = d.NextCard;
                throw new System.AccessViolationException("expected only 52 cards in the deck");
            }
            catch (System.Exception e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message == "Deck count not high enough");
            }
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDealer()
        {
            BlackjackBackend.App.Dealer d = new BlackjackBackend.App.Dealer(2);
            BlackjackBackend.App.Deck deck = new BlackjackBackend.App.Deck(1);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(d.Hand.Count == 0);
            System.Collections.Generic.List<int> ourHand = new System.Collections.Generic.List<int>();
            int card;
            for (int i = 0; i < 4; i++)
            {
                card = deck.NextCard;
                ourHand.Add(card);
                d.AddCard(card);
            }
            foreach (int c in d.Hand)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(ourHand.Contains(c));
            }
            foreach (int c in ourHand)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(d.Hand.Contains(c));
            }
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(d.Id == 2);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(d.IsNPC);
            d.ResetHand();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(d.Hand.Count == 0);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestPlayer()
        {
            BlackjackBackend.App.Player p = new BlackjackBackend.App.Player(true, 500, 2);
            p.SetBet(100);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(p.Money == 400);
            p.ResolveBet(true);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(p.Money == 600);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(p.SetBet(1000));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(p.Money == 600);
            p.SetBet(200);
            p.AddCard(1);
            p.AddCard(2);
            p.ResolveBet(false);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(p.Money == 400);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestBlackJackModel()
        {
            BlackjackBackend.App.BlackjackModel model = new BlackjackBackend.App.BlackjackModel(4, 200);
            System.Collections.Generic.List<int> ids = model.PlayerIds;
            System.Collections.Generic.Dictionary<int, int> idToBets = new System.Collections.Generic.Dictionary<int, int>();
            int nonNPCFound = 0;
            foreach(int id in ids)
            {
                try
                {
                    idToBets.Add(id, model.GetNPCBet(id));
                } 
                catch (System.ArgumentException)
                {
                    nonNPCFound++;
                }
            }
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nonNPCFound == 2, "welp " + nonNPCFound);
            System.Collections.Generic.List<System.Collections.Generic.List<int>> hands = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
            
            for(int j = 0; j < ids.Count; j++)
            {
                int id = ids[j];
                hands.Add(new System.Collections.Generic.List<int>());
                for (int i = 0; i < 3; i++)
                {
                    hands[j].Add(model.DealCardToPlayer(id));
                }
            }
            
            System.Collections.Generic.Dictionary<int, bool> res = model.ResolveGame();
            System.String dealersHand = "[";
            foreach(int c in hands[hands.Count-1])
            {
                dealersHand += CardToBlackJackValue(c) + ", ";
            }
            dealersHand = dealersHand.Substring(0, dealersHand.Length - 2);
            dealersHand += "]";
            for (int i = 0; i < ids.Count-1; i++)
            {
                System.String playersHand = "[";
                foreach(int c in hands[i])
                {
                    playersHand += CardToBlackJackValue(c) + ", ";
                }
                playersHand = playersHand.Substring(0, playersHand.Length - 2);
                playersHand += "]";
                System.Console.WriteLine(dealersHand + " " + playersHand + " " + res[i]);
            }
        }

        private int CardToBlackJackValue(int v)
        {
            int modded = v % 13 + 1;
            if (modded > 9)
            {
                return 10;
            }
            return modded;
        }
    }
}
}
