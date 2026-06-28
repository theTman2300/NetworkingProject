using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Disconnect")]
    [SerializeField] GameObject disconnectUI;
    [SerializeField] GameObject otherPlayerDisconnectUI;
    [SerializeField] GameObject GameFullScreen;

    private void Start()
    {
        if (SessionInfo.instance.HasDisconnected)
        {
            disconnectUI.SetActive(true);
            SessionInfo.instance.HasDisconnected = false;
        }
        if (SessionInfo.instance.OtherPlayerDisconnected)
        {
            otherPlayerDisconnectUI.SetActive(true);
            SessionInfo.instance.OtherPlayerDisconnected = false;
        }
        if (SessionInfo.instance.GameWasFull)
        {
            GameFullScreen.SetActive(true);
            SessionInfo.instance.GameWasFull = false;
        }
    }

    public void HostGameButton()
    {
        SessionInfo.instance.IsHosting = true;
        SceneManager.LoadScene("GameScene");
    }
    
    public void JoinGameButton()
    {
        SessionInfo.instance.IsHosting = false;
        SceneManager.LoadScene("GameScene");
    }
}
