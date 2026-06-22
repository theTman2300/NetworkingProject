using Unity.VisualScripting;
using UnityEngine;

public class CardSprites : MonoBehaviour
{
    [SerializeField] Sprite[] clubs;
    [SerializeField] Sprite[] spades;
    [SerializeField] Sprite[] hearts;
    [SerializeField] Sprite[] diamonds;
    [SerializeField] Sprite joker;
    [SerializeField] Sprite cardBack;
    [Space]
    [SerializeField] public float drawCardDelaySeconds = .3f;


    public Sprite GetCardSpriteByString(string card)
    {
        switch (card[0])
        {
            case 'C':
                return clubs[(int)card[1]];
            case 'S':
                return spades[(int)card[1]];
            case 'H':
                return hearts[(int)card[1]];
            case 'D':
                return diamonds[(int)card[1]];
            case 'J':
                return joker;
        }

        return null;
    }

    public Sprite GetCardBackSprite()
    {
        return cardBack;
    }
}
