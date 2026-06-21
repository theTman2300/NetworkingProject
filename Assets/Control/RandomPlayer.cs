using UnityEngine;

public class RandomPlayer : MonoBehaviour {
	[SerializeField]
	int playerIndex;

	//StrategySteps model;

	void Start() {
		//model = FindFirstObjectByType<ModelHolder>().model;
		//model.OnNextRound += QueueChoice;
		QueueChoice();
	}
	private void OnDestroy() {
		//model.OnNextRound -= QueueChoice;
	}

	void QueueChoice() {
		Invoke("MakeChoice", Random.value + 1);
	}
	void MakeChoice() {
		//model.ChooseSteps(playerIndex, Random.Range(0, 3) * 2 + 1);
	}
}
