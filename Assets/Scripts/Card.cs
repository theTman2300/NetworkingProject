using UnityEngine;

public class Card : MonoBehaviour
{
    public bool IsPlayerCard = false; //NOTE: is not set to true in loclPlayer yet
    public bool CanBeUsed = false;
    public string CardType;
    public int CardIndex;
}
