using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CardAssets : MonoBehaviour
{
    [SerializeField] public GameObject CardPrefab;
    [Header("Sprites")]
    [SerializeField] Sprite[] clubs;
    [SerializeField] Sprite[] spades;
    [SerializeField] Sprite[] hearts;
    [SerializeField] Sprite[] diamonds;
    [SerializeField] Sprite joker;
    [SerializeField] Sprite cardBack;
    [Header("Sounds")]
    [SerializeField] AudioClip[] cardSounds;
    [Space]
    [SerializeField] public float DrawCardDelaySeconds = .3f;

    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

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

    /// <summary>
    /// Plays a random card sound.
    /// </summary>
    public void PlayCardSound()
    {
        AudioClip randomClip = cardSounds[Random.Range(0, cardSounds.Length)];
        audioSource.PlayOneShot(randomClip);
    }
}
