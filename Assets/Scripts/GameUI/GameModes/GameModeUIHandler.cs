using Core;
using Core.Player;
using Core.Scores;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameUI.GameModes {
    public class GameModeUIHandler : MonoBehaviour {
        [FormerlySerializedAs("screenText")] [SerializeField]
        private GameModeUIText gameModeUIText;

        [SerializeField] private HoldBoostButtonText holdBoostButtonText;
        [SerializeField] private RaceResultsScreen raceResultsScreen;
        [SerializeField] private CanvasGroup levelDetailsCanvasGroup;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text musicNameText;
        public GameModeUIText GameModeUIText => gameModeUIText;
        public HoldBoostButtonText HoldBoostButtonText => holdBoostButtonText;
        public RaceResultsScreen RaceResultsScreen => raceResultsScreen;
        public CanvasGroup LevelDetailsCanvasGroup => levelDetailsCanvasGroup;
        public Text LevelNameText => levelNameText;
        public Text MusicNameText => musicNameText;

        private void Awake() {
            HideResultsScreen();
        }

        private void OnEnable() {
            Game.OnRestart += OnReset;
        }

        private void OnDisable() {
            Game.OnRestart -= OnReset;
        }

        private void OnReset() {
            HoldBoostButtonText.Reset();
        }

        // Show and hide whatever game mode result screen there is
        public void ShowResultsScreen(Score score, Score previousBest, bool isValid, string replayFilename, string replayFilepath) {
            var player = FdPlayer.FindLocalShipPlayer;
            if (player) {
                player.User.ShipCameraRig.SwitchToEndScreenCamera();

                // TODO: what the fuck does this have to do with UI?!
                player.User.DisableGameInput();
                player.User.pauseMenuEnabled = false;
                player.ShipPhysics.BringToStop();
            }

            raceResultsScreen.gameObject.SetActive(true);
            raceResultsScreen.Show(score, previousBest, isValid, replayFilename, replayFilepath);
        }

        public void HideResultsScreen() {
            raceResultsScreen.Hide();
        }
    }
}