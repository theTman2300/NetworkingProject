using UnityEngine;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        if (SessionInfo.instance.IsHosting)
        {
            GetComponent<Server>().enabled = true;
        }
        GetComponent<Client>().enabled = true;
    }
}
