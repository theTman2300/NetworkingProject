using System.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    Client client;
    CardSprites cardSprites;


    void Start()
    {
        client = FindFirstObjectByType<Client>();
        cardSprites = FindFirstObjectByType<CardSprites>();
    }

    public IEnumerator DrawCards(string[] cards)
    {
        foreach (string card in cards)
        {
            return null;

        }
        return null;
    }
}
