using TMPro;
using UnityEngine;

public class OtherPlayerCard : MonoBehaviour
{
    [SerializeField] TextMeshPro playerText;
    [SerializeField] TextMeshPro cardCounter;
    [SerializeField] int startCardAmount = 7;

    string cardCounterFormat = "{0}";
    int cards = 0;

    void Start()
    {
        cards = startCardAmount;
        cardCounterFormat = cardCounter.text;
        cardCounter.text = string.Format(cardCounterFormat, cards);
    }

    public void SetPlayerName(string name)
    {
        playerText.text = name;
    }

    public void ChangeCardCount(int change)
    {
        cards += change;
        cardCounter.text = string.Format(cardCounterFormat, cards);
    }
}
