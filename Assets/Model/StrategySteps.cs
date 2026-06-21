using UnityEngine; // Just for Debug.Log
using System;

public class StrategySteps {
	public event Action OnNextRound;
	public event Action<int> OnWin;
	public event Action<int, int> OnChoiceReveal; // player, choice
	public event Action<int, int> OnMove; // player, actual steps made

	public const int NumPlayers = 4;
	public const int NumSteps = 10;

	// Game state:
	int[] steps; // avatar positions
	int[] choice; // latest choices
	int choicesMade = 0;

	bool gameInProgress = false;

	public void Initialize() {
		steps = new int[NumPlayers];
		choice = new int[NumPlayers];
		OnNextRound?.Invoke();
	}

	public void ChooseSteps(int player, int steps) {
		if (!gameInProgress) return;
		// Only number of steps allowed: 1,3,5
		if (player >= 0 && player < NumPlayers && steps % 2 == 1 && steps > 0 && steps < 6) {
			Debug.Log($"Player {player} chooses {steps}");
			if (choice[player] == 0) choicesMade++;
			choice[player] = steps;
			if (choicesMade == NumPlayers) {
				Evaluate();
			}
		}
	}
	public void Reset() {
		if (gameInProgress) {
			Debug.Log("Cannot reset an active game");
			return;
		}
		gameInProgress = true;
		Initialize();
		choicesMade = 0;
	}

	void Evaluate() {
		// Determine who gets to move:
		int[] choiceCount = new int[3];
		for (int i = 0; i < NumPlayers; i++) {
			choiceCount[choice[i] / 2]++;
			OnChoiceReveal?.Invoke(i, choice[i]);
		}

		int max = 0;
		// Move those players:
		for (int i = 0; i < NumPlayers; i++) {
			if (choiceCount[choice[i] / 2] == 1) {
				// Visually, we should not move beyond the top of the stairs:
				int stepsMade = Mathf.Min(choice[i], NumSteps - steps[i]);
				// This value can however go higher than the total number of steps - necessary for tie breakers!
				steps[i] += choice[i]; 
				if (steps[i] > max) max = steps[i];
				Debug.Log($"Player {i} moves {stepsMade} steps to (virtual) position {steps[i]}");
				OnMove?.Invoke(i, stepsMade);
			}
		}
		// Check for winner:
		if (max >= NumSteps) {
			DetermineWinner();
		} else {
			choice = new int[NumPlayers]; // lazy programming
			choicesMade = 0;
			OnNextRound?.Invoke();
			Debug.Log("Next round!");
		}
	}

	void DetermineWinner() {
		// Tie breaker 1: who moves first over the finish
		// Tie breaker 2: who moves the most steps
		int winner = 0;
		int bestStepsNeeded = 6;
		int bestStepsMade = 0;
		for (int i=0;i<NumPlayers;i++) {
			if (steps[i]>=NumSteps) { // This player finishes this round
				int stepsNeeded = NumSteps - (steps[i] - choice[i]);
				if (stepsNeeded<bestStepsNeeded) { // closer to finish than others
					bestStepsNeeded = stepsNeeded;
					bestStepsMade = steps[i];
					winner = i;
				} else if (stepsNeeded == bestStepsNeeded && steps[i]>bestStepsMade) { // bigger overshoot
					bestStepsMade = steps[i];
					winner = i;
				}
			}
		}
		Debug.Log($"Player {winner} wins");
		gameInProgress = false;
		OnWin?.Invoke(winner);
	}
}