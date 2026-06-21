using UnityEngine;
using UnityEngine.UI;

public class ResetGame : MonoBehaviour
{
	[SerializeField]
	Button resetButton;

    Client client;

	void Start() {
        client = FindFirstObjectByType<Client>();
        client.OnWin += EndGame;
	}
	private void OnDestroy() {
        client.OnWin -= EndGame;
	}

	void EndGame(int winner) {
		resetButton.gameObject.SetActive(true);
	}

	// Called by button event:
	public void Restart() {
		resetButton.gameObject.SetActive(false);
		client.ResetRpc();
	}
}
