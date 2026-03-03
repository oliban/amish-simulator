using UnityEngine;

namespace AmishSimulator
{
    /// <summary>
    /// Auto-starts the game when the Homestead scene loads directly
    /// (bypassing the main menu). Safe to leave in the scene; if the
    /// game is already Playing (came from main menu) it does nothing.
    /// </summary>
    public class HomesteadBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing)
                GameManager.Instance.StartGame(Gender.Male);
        }
    }
}
