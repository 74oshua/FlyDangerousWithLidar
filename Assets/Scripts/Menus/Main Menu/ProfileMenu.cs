﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Player;
using Core.ShipModel;
using Den.Tools;
using FdUI;
using Menus.Main_Menu.Components;
using Misc;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Menus.Main_Menu {
    public class ProfileMenu : MenuBase {
        [SerializeField] private SpriteAtlas flagSpriteAtlas;

        [SerializeField] private InputField playerNameTextField;
        [SerializeField] private Dropdown countryDropdown;

        [SerializeField] private ShipSelectionRenderer shipSelectionRenderer;
        [SerializeField] private Text shipName;
        [SerializeField] private Text shipDescription;

        [SerializeField] private Text shipCounter;
        [SerializeField] private UIButton nextButton;
        [SerializeField] private UIButton prevButton;

        [SerializeField] private Thruster thruster;
        [SerializeField] private Image trailPreview;
        [SerializeField] private Image lightPreview;

        [SerializeField] private Button cancelButton;

        [SerializeField] private FlexibleColorPicker playerShipPrimaryColorPicker;
        [SerializeField] private FlexibleColorPicker playerShipAccentColorPicker;
        [SerializeField] private FlexibleColorPicker playerShipThrusterColorPicker;
        [SerializeField] private FlexibleColorPicker playerShipTrailColorPicker;
        [SerializeField] private FlexibleColorPicker playerShipHeadLightsColorPicker;
        [SerializeField] private FlexibleColorPicker playerHUDColorPicker;
        private string _playerShipPrimaryColor;
        private string _playerShipAccentColor;
        private string _playerShipThrusterColor;
        private string _playerShipTrailColor;
        private string _playerShipHeadLightsColor;
        private string _playerHUDLightsColor;

        private bool _ready;
        private ShipMeta _selectedShip;

        private List<ShipMeta> _ships;

        private void FixedUpdate() {
            // wait for color pickers to be set god damnit
            if (_ready) {
                var shouldUpdate = false;

                var playerShipPrimaryColor = $"#{ColorUtility.ToHtmlStringRGB(playerShipPrimaryColorPicker.color)}";
                var playerShipAccentColor = $"#{ColorUtility.ToHtmlStringRGB(playerShipAccentColorPicker.color)}";
                var playerShipThrusterColor = $"#{ColorUtility.ToHtmlStringRGB(playerShipThrusterColorPicker.color)}";
                var playerShipTrailColor = $"#{ColorUtility.ToHtmlStringRGB(playerShipTrailColorPicker.color)}";
                var playerShipHeadLightsColor =
                    $"#{ColorUtility.ToHtmlStringRGB(playerShipHeadLightsColorPicker.color)}";
                var playerHUDColor = $"#{ColorUtility.ToHtmlStringRGB(playerHUDColorPicker.color)}";

                if (playerShipPrimaryColor != _playerShipPrimaryColor) {
                    _playerShipPrimaryColor = playerShipPrimaryColor;
                    shouldUpdate = true;
                }

                if (playerShipAccentColor != _playerShipAccentColor) {
                    _playerShipAccentColor = playerShipAccentColor;
                    shouldUpdate = true;
                }

                if (playerShipThrusterColor != _playerShipThrusterColor) {
                    _playerShipThrusterColor = playerShipThrusterColor;
                    shouldUpdate = true;
                }

                if (playerShipTrailColor != _playerShipTrailColor) {
                    _playerShipTrailColor = playerShipTrailColor;
                    shouldUpdate = true;
                }

                if (playerShipHeadLightsColor != _playerShipHeadLightsColor) {
                    _playerShipHeadLightsColor = playerShipHeadLightsColor;
                    shouldUpdate = true;
                }

                if (playerHUDColor != _playerHUDLightsColor) {
                    _playerHUDLightsColor = playerHUDColor;
                    shouldUpdate = true;
                }

                if (shouldUpdate) RefreshColors();

                // Only show the 3d models if the country dropdown isn't showing
                // Slow but who cares
                var shouldHideModels = countryDropdown.gameObject.transform.FindChildRecursive("Dropdown List") != null;
                shipSelectionRenderer.gameObject.SetActive(!shouldHideModels);
                thruster.gameObject.SetActive(!shouldHideModels);
            }
        }

        protected override void OnOpen() {
            FdEnum.PopulateDropDown(Flag.List(), countryDropdown, null, flag => flagSpriteAtlas.GetSprite(flag.Filename));
            _ships = ShipMeta.List().ToList();
            LoadFromPreferences();
            cancelButton.interactable = false;
        }

        public void Apply() {
            Preferences.Instance.SetString("playerName", playerNameTextField.text);
            Preferences.Instance.SetString("playerFlag", FdEnum.FromDropdownId(Flag.List(), countryDropdown.value).Filename);
            Preferences.Instance.SetString("playerShipDesign", _selectedShip.Name);
            Preferences.Instance.SetString("playerShipPrimaryColor", _playerShipPrimaryColor);
            Preferences.Instance.SetString("playerShipAccentColor", _playerShipAccentColor);
            Preferences.Instance.SetString("playerShipThrusterColor", _playerShipThrusterColor);
            Preferences.Instance.SetString("playerShipTrailColor", _playerShipTrailColor);
            Preferences.Instance.SetString("playerShipHeadLightsColor", _playerShipHeadLightsColor);
            Preferences.Instance.SetString("playerHUDIndicatorColor", _playerHUDLightsColor);
            Preferences.Instance.Save();

            // grab the main menu and update the ship if available
            var mainMenu = FindObjectOfType<MainMenu>();
            mainMenu.SetShipFromPreferences();

            // we're going backward but with a positive apply sound so don't set the call chain in the previous menu
            Progress(caller, false, false);
        }

        public void NextShip() {
            var index = _ships.FindIndex(ship => ship.Id == _selectedShip.Id);
            if (_ships.Count > index + 1) SetShip(_ships[index + 1]);
        }

        public void PrevShip() {
            var index = _ships.FindIndex(ship => ship.Id == _selectedShip.Id);
            if (index > 0) SetShip(_ships[index - 1]);
        }

        public void SetShipPrimaryColor(string htmlColor) {
            _playerShipPrimaryColor = htmlColor;
            shipSelectionRenderer.SetShipPrimaryColor(htmlColor);
        }

        public void SetShipAccentColor(string htmlColor) {
            _playerShipAccentColor = htmlColor;
            shipSelectionRenderer.SetShipAccentColor(htmlColor);
        }

        public void SetThrusterColor(string htmlColor) {
            _playerShipThrusterColor = htmlColor;
            thruster.ThrustColor = ParseColor(htmlColor);
        }

        public void SetTrailColor(string htmlColor) {
            _playerShipTrailColor = htmlColor;
            trailPreview.color = ParseColor(htmlColor);
        }

        public void SetShipLightColor(string htmlColor) {
            _playerShipHeadLightsColor = htmlColor;
            lightPreview.color = ParseColor(htmlColor);
        }

        public void SetHUDColor(string htmlColor) {
            _playerHUDLightsColor = htmlColor;
        }

        public void SetCancelButtonEnabled(bool active) {
            cancelButton.interactable = active;
        }

        private void LoadFromPreferences() {
            IEnumerator Load() {
                // color picker has some weird nonsense logic if called before it's start (which is 2 frames from now)
                // and it sometimes fails to set the color because god knows why but it really doesn't matter
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                // if using online services, grab the name from there.
                playerNameTextField.interactable = !Player.IsUsingOnlineName;

                // load details from prefs
                playerNameTextField.text = Player.LocalPlayerName;
                countryDropdown.value = FdEnum.ToDropdownId(Flag.List(), Flag.FromFilename(Preferences.Instance.GetString("playerFlag")));
                _playerShipPrimaryColor = Preferences.Instance.GetString("playerShipPrimaryColor");
                _playerShipAccentColor = Preferences.Instance.GetString("playerShipAccentColor");
                _playerShipThrusterColor = Preferences.Instance.GetString("playerShipThrusterColor");
                _playerShipTrailColor = Preferences.Instance.GetString("playerShipTrailColor");
                _playerShipHeadLightsColor = Preferences.Instance.GetString("playerShipHeadLightsColor");
                _playerHUDLightsColor = Preferences.Instance.GetString("playerHUDIndicatorColor");

                playerShipPrimaryColorPicker.color = ParseColor(_playerShipPrimaryColor);
                playerShipAccentColorPicker.color = ParseColor(_playerShipAccentColor);
                playerShipThrusterColorPicker.color = ParseColor(_playerShipThrusterColor);
                playerShipTrailColorPicker.color = ParseColor(_playerShipTrailColor);
                playerShipHeadLightsColorPicker.color = ParseColor(_playerShipHeadLightsColor);
                playerHUDColorPicker.color = ParseColor(_playerHUDLightsColor);

                SetShip(ShipMeta.FromString(Preferences.Instance.GetString("playerShipDesign")));
                _ready = true;
            }

            StartCoroutine(Load());
        }

        private void SetShip(ShipMeta ship) {
            _selectedShip = ship;
            shipSelectionRenderer.SetShip(ship);
            shipName.text = ship.FullName;
            shipDescription.text = ship.Description;

            UpdateShipSelectionButtonState();
            UpdateShipCounter();
            RefreshColors();
        }

        private void RefreshColors() {
            SetShipPrimaryColor(_playerShipPrimaryColor);
            SetShipAccentColor(_playerShipAccentColor);
            SetThrusterColor(_playerShipThrusterColor);
            SetTrailColor(_playerShipTrailColor);
            SetShipLightColor(_playerShipHeadLightsColor);
            SetHUDColor(_playerHUDLightsColor);
        }

        // if there's no more ships to the left or right of the data structure, disable those buttons
        private void UpdateShipSelectionButtonState() {
            prevButton.button.interactable = true;
            nextButton.button.interactable = true;
            if (_selectedShip.Id == 0) {
                prevButton.button.interactable = false;
                nextButton.button.Select();
            }

            if (_selectedShip.Id == _ships.Count - 1) {
                nextButton.button.interactable = false;
                prevButton.button.Select();
            }
        }

        private void UpdateShipCounter() {
            shipCounter.text = $"{_selectedShip.Id + 1} of {_ships.Count}";
        }

        private Color ParseColor(string htmlColor) {
            if (!ColorUtility.TryParseHtmlString(htmlColor, out var color)) {
                color = Color.red;
                Debug.LogWarning("Failed to parse html color " + htmlColor);
            }

            return color;
        }
    }
}