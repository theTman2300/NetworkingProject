using UnityEngine;

public class SessionInfo : MonoBehaviour
{
    public bool IsHosting;

    public bool HasDisconnected;
    public bool OtherPlayerDisconnected;
    public bool GameWasFull;

    public static SessionInfo instance;

    private void Awake()
    {
        if (instance != null) return;

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
