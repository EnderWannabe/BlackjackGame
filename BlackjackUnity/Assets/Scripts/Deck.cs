namespace BlackjackBackend
{
namespace App
{
    public class Deck 
    {
        System.Collections.Generic.List<int> _deck;
        int _pointerToCurrentCard = 0;

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
                if (_pointerToCurrentCard > _deck.Count-1)
                {
                    throw new System.Exception("Deck count not high enough");
                }
                int card = _deck[_pointerToCurrentCard];
                _pointerToCurrentCard++;
                return card;
            }
        }

        public void Reset()
        {
            Shuffle(_deck);
            _pointerToCurrentCard = 0;
        }
    }
}
}