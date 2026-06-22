using Unity.VisualScripting;
using UnityEngine;

public class CardSprites : MonoBehaviour
{
    [SerializeField] public GameObject CardPrefab;
    [Space]
    [SerializeField] Sprite[] clubs;
    [SerializeField] Sprite[] spades;
    [SerializeField] Sprite[] hearts;
    [SerializeField] Sprite[] diamonds;
    [SerializeField] Sprite joker;
    [SerializeField] Sprite cardBack;
    [Space]
    [SerializeField] public float DrawCardDelaySeconds = .3f;


    public Sprite GetCardSpriteByString(string card)
    {
        switch (card[0])
        {
            case 'C':
                return clubs[int.Parse(card.Remove(0, 1)) - 1];
            case 'S':
                return spades[int.Parse(card.Remove(0, 1)) - 1];
            case 'H':
                return hearts[int.Parse(card.Remove(0, 1)) - 1];
            case 'D':
                return diamonds[int.Parse(card.Remove(0, 1)) - 1];
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
