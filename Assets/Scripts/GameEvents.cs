using System;
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

		// Global Events //

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
			_gameUpdates[GameStates.GamePaused] = UpdateGamePaused;
		}

		/// <summary>
		/// Links all default UI events through lambda listeners.
		/// </summary>
		private void LinkUIEvents()
		{
			// linking UI events through script is currently my preference //
			// as opposed to unity inspector //

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

			// challenger button //

			_ui.ChallengerBtn.RootButton.onClick.AddListener(() => {
				EnterChallengerWorld();
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ChallengerBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// challenger button //

			// challenger exit button //

			_ui.ChallengerExitBtn.RootButton.onClick.AddListener(() => {
				ExitChallengerWorld();
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.ChallengerExitBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// challenger exit button //

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

			// achievements button //

			_ui.AchievementsBtn.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += _ui.EnterAchievements;
				_ui.OnFadedIn += _ui.ExitMainMenu;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.AchievementsBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// achievements button //

			// achievements exit button //

			_ui.AchievementsExitBtn.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += _ui.ExitAchievements;
				_ui.OnFadedIn += _ui.EnterMainMenu;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.AchievementsExitBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// achievements exit button //

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

				if(_toSettingsState == GameStates.GamePaused)
				{
					_ui.OnFadedIn += _ui.EnterGamePaused;
					_ui.OnFadedIn += MoveCameraBackToStackerRow;
					_inSettingsFromPause = false;
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

			// screen mode dropdown //

			_ui.Dropdown_ScreenMode.onValueChanged.AddListener((val) =>
			{
				switch(val)
				{
					case 0:
						UI.CURRENT_SCREEN_MODE = FullScreenMode.ExclusiveFullScreen;
						break;
					case 1:
						UI.CURRENT_SCREEN_MODE = FullScreenMode.FullScreenWindow;
						break;
					case 2:
						UI.CURRENT_SCREEN_MODE = FullScreenMode.MaximizedWindow;
						break;
					case 3:
						UI.CURRENT_SCREEN_MODE = FullScreenMode.Windowed;
						break;
				}

				Screen.fullScreenMode = UI.CURRENT_SCREEN_MODE;
				_saveData.ScreenMode = UI.CURRENT_SCREEN_MODE;
				Debug.Log((int)UI.CURRENT_SCREEN_MODE);
			});

			// screen mode dropdown //

			// resolution dropdown //

			_ui.Dropdown_Resolutions.onValueChanged.AddListener((val) =>
			{
				UI.CURRENT_RESOLUTION = UI.RESOLUTIONS[val];
				UI.CURRENT_REFRESH_RATE = UI.RESOLUTIONS[val].refreshRateRatio;

				Screen.SetResolution(UI.CURRENT_RESOLUTION.width, 
					UI.CURRENT_RESOLUTION.height, 
					UI.CURRENT_SCREEN_MODE, 
					UI.CURRENT_REFRESH_RATE);

				_saveData.ResolutionWidth = UI.CURRENT_RESOLUTION.width;
				_saveData.ResolutionHeight = UI.CURRENT_RESOLUTION.height;
				_saveData.ScreenMode = UI.CURRENT_SCREEN_MODE;
				_saveData.RefreshRate = UI.CURRENT_REFRESH_RATE.numerator;
				_saveData.RefreshRateDenom = UI.CURRENT_REFRESH_RATE.denominator;
			});

			// resolution dropdown //

			// bloom slider //

			_ui.Slider_Bloom.onValueChanged.AddListener((val) =>
			{
				UnityEngine.Rendering.Universal.Bloom bloom;
				bool hasBloom = _ui.PPVolume.sharedProfile.TryGet(out bloom);
				if (!hasBloom)
					return;
				// override bloom
				float or = _ui.BloomIntensityMax * val;
				bloom.intensity.Override(or);
				_saveData.BloomIntensity = or;
			});

			// bloom slider //


			// pause resume btn //

			_ui.Pause_ResumeBtn.RootButton.onClick.AddListener(() => {

				ExitPause(toExit: false);
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.Pause_ResumeBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// pause resume btn //

			// pause settings btn //

			_ui.Pause_SettingsBtn.RootButton.onClick.AddListener(() => {

				_ui.StartFadeIn();
				_ui.OnFadedIn += _ui.EnterSettings;
				_ui.OnFadedIn += _ui.ExitGamePaused;
				_ui.OnFadedIn += MoveCameraToEmptyPoint;
				_toSettingsState = GameStates.GamePaused;
				_inSettingsFromPause = true;
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.Pause_SettingsBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// pause settings btn //


			// pause exit btn //

			_ui.Pause_ExitBtn.RootButton.onClick.AddListener(() => {

				ExitPause(toExit: true);
				_currStackerRow.Pause(true);
				TriggerGameOver();
				CreateSFX(SoundIDs.ButtonClick, 0);
			});

			_ui.Pause_ExitBtn.OnHoverEvent.AddListener(() =>
			{
				CreateSFX(SoundIDs.ButtonHover, 0);
			});

			// pause exit btn //
		}
	}
}

