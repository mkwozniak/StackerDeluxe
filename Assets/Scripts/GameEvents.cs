using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wozware.StackerDeluxe
{
	public sealed partial class Game : MonoBehaviour
	{
		// Global Events //

		public static event Action OnCameraFinishLerp;
		public static event Action OnCameraFinishDamp;
		public static event Action OnGameOver;

		public static Action DoReduceThickness;
		public static Action<SoundIDs, float> DoCreateSFX;
		public static Action<ParticleSystem, Vector3, float> DoCreateParticleFX;
		public static Action<OneShotIDs, int> DoPlayOneShot;
		public static Action<bool> DoTriggerGameOver;
		public static Func<bool> DoCheckGameWin;
		public static Action DoContinueStackerSpawn;
		public static Action DoSaveToSaveFile;

		// Global Events

		private void LinkGameEvents()
		{
			DoReduceThickness = ReduceThickness;
			DoCreateParticleFX = CreateParticleFX;
			DoCreateSFX = CreateSFX;
			DoPlayOneShot = PlayOneShot;
			DoTriggerGameOver = TriggerGameOver;
			DoCheckGameWin = CheckGameWin;
			DoSaveToSaveFile = SaveToSaveFile;
			DoContinueStackerSpawn = ContinueNextStackerSpawn;

			_gameUpdates[GameStates.MainMenu] = UpdateMainMenu;
			_gameUpdates[GameStates.GameActive] = UpdateGameActive;
			_gameUpdates[GameStates.GameOver] = UpdateGameOver;
			_gameUpdates[GameStates.GameWin] = UpdateGameWin;
		}

		/// <summary>
		/// Links all default UI events through lambda listeners.
		/// </summary>
		private void LinkUIEvents()
		{
			// in editor events work fine
			// but linking events through script allows for easier management and finer control

			// normal difficulty button //

			_ui.DifficultyBtn_Normal.RootButton.onClick.AddListener(() => {
				EnterGameAtDifficulty(LevelDifficulties.Debug);
			});

			_ui.DifficultyBtn_Normal.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// normal difficulty button //

			// hard difficulty button //

			_ui.DifficultyBtn_Hard.RootButton.onClick.AddListener(() => {
				EnterGameAtDifficulty(LevelDifficulties.Hard);
			});

			_ui.DifficultyBtn_Hard.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// hard difficulty button //

			// expert difficulty button //

			_ui.DifficultyBtn_Expert.RootButton.onClick.AddListener(() => {
				EnterGameAtDifficulty(LevelDifficulties.Expert);
			});

			_ui.DifficultyBtn_Expert.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// expert difficulty button //

			// gameover restart button //

			_ui.RestartBtn_GameOver.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += EnterPregameMode;
				_ui.OnFadedIn += _ui.ExitGameOver;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.RestartBtn_GameOver.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// gameover restart button //

			// gameover exit button //

			_ui.ExitBtn_GameOver.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += EnterMenuMode;
				_ui.OnFadedIn += _ui.ExitGameOver;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ExitBtn_GameOver.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// gameover exit button //

			// win restart button //

			_ui.RestartBtn_Win.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += EnterPregameMode;
				_ui.OnFadedIn += _ui.ExitGameWin;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.RestartBtn_GameOver.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// win restart button //

			// win exit button //

			_ui.ExitBtn_Win.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += EnterMenuMode;
				_ui.OnFadedIn += _ui.ExitGameWin;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ExitBtn_Win.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// win exit button //

			// menu settings enter button //

			_ui.SettingsBtn_Menu.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += _ui.EnterSettings;
				_ui.OnFadedIn += _ui.ExitMainMenu;
				_toSettingsState = GameStates.MainMenu;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.SettingsBtn_Menu.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// menu settings enter button //

			// settings exit button //

			_ui.ExitSettingsBtn.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += _ui.ExitSettings;

				if(_toSettingsState == GameStates.MainMenu)
				{
					_ui.OnFadedIn += _ui.EnterMainMenu;
				}

				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ExitSettingsBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// settings exit button //

			// settings apply button //

			_ui.ApplySettingsBtn.RootButton.onClick.AddListener(() => {

				SaveToSaveFile();
				_ui.ShowSettingsAppliedText();
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ApplySettingsBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// settings apply button //


			// music volume slider //

			_ui.Slider_MusicVolume.onValueChanged.AddListener((val) =>
			{
				SetMusicVolume(val);
			});

			// music volume slider //

			// sfx volume slider //

			_ui.Slider_SFXVolume.onValueChanged.AddListener((val) =>
			{
				SetSFXVolume(val);
			});

			// sfx volume slider //

			// music mute toggle //

			_ui.Toggle_MusicMute.onValueChanged.AddListener((val) =>
			{
				MuteMusic(val);
				_ui.UpdateMusicMuteToggleVisual(val);
			});

			// music mute toggle //

			// sfx mute toggle //

			_ui.Toggle_SFXMuted.onValueChanged.AddListener((val) =>
			{
				MuteSFX(val);
				_ui.UpdateSFXMuteToggleVisual(val);
			});

			// sfx mute toggle //
		}
	}
}

