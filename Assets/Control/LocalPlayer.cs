using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
	//[SerializeField]
	//int playerIndex;

	Client client;

	void Start()
    {
		//model = FindFirstObjectByType<ModelHolder>().model;
		client = FindFirstObjectByType<Client>();
	}

	public void MakeChoice(int choice) {
		client.ChooseStepsRpc(choice);

	}
}
