using System;
using System.Collections.Generic;
using UnityEngine;
using CanvasText = TMPro.TextMeshProUGUI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
using Dropdown = TMPro.TMP_Dropdown;
using System.Collections;

namespace wozware.StackerDeluxe
{
	public class UI : MonoBehaviour
	{
		public static Resolution[] RESOLUTIONS;
		public static Resolution CURRENT_RESOLUTION;
		public static RefreshRate CURRENT_REFRESH_RATE;
		public static FullScreenMode CURRENT_SCREEN_MODE;

		public Action OnFadedIn;
		public Action OnFadedOut;

		[Header("Settings")]
		public float FadeSpeed;
		public float NoticePanelSpeed;
		public float NoticePanelMoveDistance;
		public float NoticePanelLingerTime;
		public float WinAchievementsDelay;

		[Header("References")]
		public ButtonVisualModifier DifficultyBtn_Normal;
		public ButtonVisualModifier DifficultyBtn_Hard;
		public ButtonVisualModifier DifficultyBtn_Expert;
		public ButtonVisualModifier RestartBtn_GameOver;
		public ButtonVisualModifier ExitBtn_GameOver;
		public ButtonVisualModifier RestartBtn_Win;
		public ButtonVisualModifier ExitBtn_Win;
		public ButtonVisualModifier ContinueBtn_Win;
		public ButtonVisualModifier SettingsBtn_Menu;
		public ButtonVisualModifier ExitSettingsBtn;
		public ButtonVisualModifier ApplySettingsBtn;
		public ButtonVisualModifier ChallengerBtn;
		public ButtonVisualModifier ChallengerExitBtn;
		public ButtonVisualModifier Pause_ResumeBtn;
		public ButtonVisualModifier Pause_RestartBtn;
		public ButtonVisualModifier Pause_SettingsBtn;
		public ButtonVisualModifier Pause_ExitBtn;
		public ButtonVisualModifier AchievementsBtn;
		public ButtonVisualModifier AchievementsExitBtn;
		public ButtonVisualModifier LevelStartBtn;
		public ButtonVisualModifier LevelCancelBtn;
		public ButtonVisualModifier GameExitBtn;

		public Slider Slider_MusicVolume;
		public Slider Slider_SFXVolume;
		public Slider Slider_Bloom;

		public Toggle Toggle_MusicMute;
		public Toggle Toggle_SFXMuted;
		public Toggle Toggle_ShortCountdown;

		public Dropdown Dropdown_Resolutions;
		public Dropdown Dropdown_ScreenMode;

		public UnityEngine.Rendering.Volume PPVolume;
		public float BloomIntensityMax = 0.5f;

		public Dictionary<string, UIAchievement> UIAchievements = new();

		public CustomTextProperties MusicMuteToggleText { get { return _musicMuteText; } }
		public CustomTextProperties SFXMuteToggleText { get { return _sfxMuteText; } }
		public CustomTextProperties ShortCountdownToggleText { get { return _shortCountdownText; } }
		public int NumCompletedAchievements { get { return _completedAchievements; } set { _completedAchievements = value; } }
		public int CurrentAchievementWinIndex { set { _currAchievementWinIndex = value; } }

		[SerializeField] Image _fadeScreen;
		[SerializeField] CanvasText _timeEntryPrefab;
		[SerializeField] UIAchievement _achievementPrefab;

		[Header("UI Parents")]
		[SerializeField] GameObject _menuParent;
		[SerializeField] GameObject _gameParent;
		[SerializeField] GameObject _gameOverParent;
		[SerializeField] GameObject _getReadyParent;
		[SerializeField] GameObject _countdownParent;
		[SerializeField] CanvasText _countdownText;
		[SerializeField] GameObject _winParent;
		[SerializeField] Transform _timeParent;
		[SerializeField] Transform _timeEntryParent;
		[SerializeField] GameObject _settingsParent;
		[SerializeField] GameObject _finishParent;
		[SerializeField] GameObject _challengerParent;
		[SerializeField] GameObject _pauseParent;
		[SerializeField] GameObject _achievementParent;
		[SerializeField] GameObject _timeoutParent;
		[SerializeField] GameObject _pregameParent;
		[SerializeField] RectTransform _achievementContentParent;
		[SerializeField] GameObject _winAchievementParent;
		[SerializeField] RectTransform _achievementWinContentParent;

		[Header("Time Panel")]
		[SerializeField] CanvasText _timeTextMin_D0;
		[SerializeField] CanvasText _timeTextMin_D1;
		[SerializeField] CanvasText _timeTextSec_D0;
		[SerializeField] CanvasText _timeTextSec_D1;
		[SerializeField] CanvasText _timeTextMs_D0;
		[SerializeField] CanvasText _timeTextMs_D1;
		[SerializeField] CustomTextProperties _timeGainSign;
		[SerializeField] CustomTextProperties _timeGainText_D0;
		[SerializeField] CustomTextProperties _timeGainText_Dec;
		[SerializeField] CustomTextProperties _timeGainText_D1;
		[SerializeField] CustomTextProperties _timeGainText_Unit;
		[SerializeField] Color _timeGainAddColor;
		[SerializeField] Color _timeGainLoseColor;
		[SerializeField] Color _greyedColor;
		[ColorUsage(true, true), SerializeField] Color _timeGainAddGlowColor;
		[ColorUsage(true, true), SerializeField] Color _timeGainLoseGlowColor;
		[SerializeField] CustomTextProperties _pregameLevelTitleText;
		[SerializeField] CustomTextProperties _pregameLevelTimeText;

		[Header("Settings Panel")]
		[SerializeField] CustomTextProperties _settingsAppliedText;
		[SerializeField] CustomTextProperties _musicMuteText;
		[SerializeField] CustomTextProperties _sfxMuteText;
		[SerializeField] CustomTextProperties _shortCountdownText;

		[Header("Notice Panel")]
		[SerializeField] CustomTextProperties _noticePanelTextProps;
		[SerializeField] RectTransform _noticePanel;
		[SerializeField] Image _noticePanelBorderImage;
		[SerializeField] CustomTextProperties _creditsText;
		[SerializeField] float _creditsFloatSpeed = 10f;
		[SerializeField] int _creditsFlickerLimit = 3;

		[Header("Game Over / Finish")]
		[SerializeField] CustomTextProperties _finishTextProps;
		[SerializeField] Image _finishParentBorder;
		[SerializeField] CustomTextProperties _finishLevelTitleText;
		[SerializeField] CustomTextProperties _finishLevelTimeText;
		[SerializeField] CustomTextProperties _finishLevelMissesText;
		[SerializeField] CustomTextProperties _finishLevelPerfectsText;
		[SerializeField] CustomTextProperties _finishLevelNoticeText;
		[SerializeField] CustomTextProperties _finishLevelRecordTimeText;
		[SerializeField] CustomTextProperties _finishNewRecordLabel;
		[SerializeField] CustomTextProperties _finishPerfectScoreLabel;
		[SerializeField] CustomTextProperties _achievementNumberText;

		[Header("States")]
		[SerializeField] float _currFadeLerp = 0f;
		[SerializeField] byte _fadeScreenState = 0;
		[SerializeField][Multiline] List<string> _credits;
		[SerializeField] int _numAchievements;
		[SerializeField] int _completedAchievements;

		// local members //

		Color _currFadeColor;
		Color _fadeColorIn = new Color(0, 0, 0, 1);
		Color _fadeColorOut = new Color(0, 0, 0, 0);
		byte _noticePanelState;
		Vector3 _noticePanelOriginalPos;
		Vector3 _noticePanelDestPos;
		float _noticePanelLingerTimer;
		float _noticePanelLerp;
		List<GameObject> _currTimeEntries = new();
		int _currCreditIndex = 0;
		RectTransform _creditsTransform;
		Vector2 _creditsOriginalPos;
		int _currAchievementWinIndex = 0;

		// local members //

		#region Unity Methods

		private void Start()
		{
			Vector3 origPos = _noticePanel.anchoredPosition;
			_noticePanelOriginalPos = origPos;
			_noticePanelDestPos = new Vector3(origPos.x, _noticePanel.anchoredPosition.y - NoticePanelMoveDistance, origPos.z);
			_creditsTransform = _creditsText.GetComponent<RectTransform>();
			_creditsOriginalPos = _creditsTransform.anchoredPosition;
			_currCreditIndex = 0;
			_creditsText.SetText(_credits[_currCreditIndex]);
		}

		private void Update()
		{
			UpdateScreenFade();
			UpdateNoticePanel();
		}

		private void LateUpdate()
		{
			LateUpdateScreenFade();
		}

		#endregion

		#region Menu

		public void EnterMainMenu()
		{
			OnFadedIn -= EnterMainMenu;
			_menuParent.SetActive(true);
			StartFadeOut();
		}

		public void ExitMainMenu()
		{
			_menuParent.SetActive(false);
			ShowPregame(false);
			OnFadedIn -= ExitMainMenu;
		}

		public void UpdateMenuCredits()
		{
			_creditsTransform.anchoredPosition += new Vector2(0, _creditsFloatSpeed * Time.deltaTime);

			if(_creditsText.FlickerCount >= _creditsFlickerLimit)
			{
				_currCreditIndex++;
				if(_currCreditIndex >= _credits.Count)
				{
					_currCreditIndex = 0;
				}
				_creditsTransform.anchoredPosition = _creditsOriginalPos;
				_creditsText.FlickerCount = 0;
				_creditsText.SetText(_credits[_currCreditIndex]);
			}
		}

		public void ShowPregame(bool val)
		{
			_pregameParent.SetActive(val);
		}

		public void PopulateAchievements(string name)
		{
			LevelRecord record = RecordKeeper.LVL_RECORDS[name];
			for (int i = 0; i < record.Achievements.Count; i++)
			{
				UIAchievement g = Instantiate(_achievementPrefab, _achievementContentParent);

				g.Description.SetText(record.Achievements[i].Description);

				UIAchievements[record.Achievements[i].Description] = g;

				_numAchievements += 1;
				if (record.Achievements[i].Complete)
				{
					g.DoneObject.SetActive(true);
					NumCompletedAchievements += 1;
				}
			}
		}

		public void DestroyWinAchievements()
		{
			int i = 0;
			while (i < _achievementWinContentParent.childCount)
			{
				Destroy(_achievementWinContentParent.GetChild(i).gameObject);
				i++;
			}
		}

		public IEnumerator PopulateWinAchievements(string name)
		{
			LevelRecord record = RecordKeeper.LVL_RECORDS[name];
			int numRecords = record.Achievements.Count;

			int i = _currAchievementWinIndex;
			if (_currAchievementWinIndex < numRecords)
			{
				yield return new WaitForSeconds(WinAchievementsDelay);
				UIAchievement g = Instantiate(_achievementPrefab, _achievementWinContentParent);

				g.Description.SetText(record.Achievements[i].Description);

				UIAchievements[record.Achievements[i].Description] = g;

				if (record.Achievements[i].Complete)
				{
					g.DoneObject.SetActive(true);
					Game.DoCreateSFX(SoundIDs.VFX_AchievementGain, 0f);
				}

				_currAchievementWinIndex++;
				StartCoroutine(PopulateWinAchievements(name));
			}

			yield return null;
		}

		#endregion Menu

		#region Game

		public void EnterGame()
		{
			_timeParent.gameObject.SetActive(true);
			_noticePanel.gameObject.SetActive(true);
			_finishParent.SetActive(false);
		}

		public void ExitGame()
		{
			_timeParent.gameObject.SetActive(false);
			_noticePanel.gameObject.SetActive(false);
			_finishParent.gameObject.SetActive(false);
			_gameOverParent.SetActive(false);
		}

		public void EnterPregame()
		{
			_gameParent.SetActive(true);
			_getReadyParent.SetActive(true);
			_countdownParent.SetActive(false);
		}

		public void SetPreLevelStats(string lvlName, string[] timeStrings)
		{
			_pregameLevelTitleText.SetText($"LEVEL: {lvlName}");
			_pregameLevelTimeText.SetText($"BEST TIME: {timeStrings[0]}:{timeStrings[1]}:{timeStrings[2]}");
		}

		public void EnterChallenger()
		{
			_challengerParent.SetActive(true);
		}

		public void ExitChallenger()
		{
			_challengerParent.SetActive(false);
		}

		public void EnterAchievements()
		{
			_achievementParent.SetActive(true);
			_achievementNumberText.SetText($"{_completedAchievements} / {_numAchievements} COMPLETED");
			OnFadedIn -= EnterAchievements;
			StartFadeOut();
		}

		public void ExitAchievements()
		{
			_achievementParent.SetActive(false);
			OnFadedIn -= ExitAchievements;
		}

		#endregion Game

		#region Game Over

		public void EnterGameOver()
		{
			_gameOverParent.SetActive(true);
			_finishParent.SetActive(false);
			_timeoutParent.SetActive(false);
		}

		public void ExitGameOver()
		{
			OnFadedIn -= ExitGameOver;
			_gameOverParent.SetActive(false);
			_finishParent.SetActive(false);
		}

		#endregion Game Over

		#region Game Win

		public void EnterGameWin()
		{
			_winParent.SetActive(true);
		}

		public void ExitGameWin()
		{
			OnFadedOut -= ExitGameWin;
			_winParent.SetActive(false);
			_finishParent.SetActive(false);
		}

		public void EnterGameAchievementWin()
		{
			OnFadedIn -= EnterGameAchievementWin;
			_winAchievementParent.SetActive(true);
			StartFadeOut();
		}

		public void ExitGameAchievementWin()
		{
			OnFadedIn -= EnterGameAchievementWin;
			_winAchievementParent.SetActive(false);
		}

		public void SetGameWinLabels(string levelName, string[] time, string[] recordTime, string misses, string perfects)
		{
			_finishLevelTitleText.SetText(levelName);

			string m = time[0][0].ToString() + time[0][1].ToString();
			string s = time[1][0].ToString() + time[1][1].ToString();
			string ms = time[2][0].ToString() + time[2][1].ToString();

			string rm = recordTime[0][0].ToString() + recordTime[0][1].ToString();
			string rs = recordTime[1][0].ToString() + recordTime[1][1].ToString();
			string rms = recordTime[2][0].ToString() + recordTime[2][1].ToString();

			_finishLevelTimeText.SetText($"{m}:{s}:{ms}");
			_finishLevelMissesText.SetText(misses);
			_finishLevelPerfectsText.SetText(perfects);
			_finishLevelRecordTimeText.SetText($"{rm}:{rs}:{rms}");
		}

		public void ShowNewRecordLabel(bool show)
		{
			_finishNewRecordLabel.gameObject.SetActive(show);
		}

		public void ShowPerfectScoreLabel(bool show)
		{
			_finishPerfectScoreLabel.gameObject.SetActive(show);
		}

		#endregion Game Win

		#region Game Paused

		public void EnterGamePaused()
		{
			StartFadeOut();
			_pauseParent.SetActive(true);
			OnFadedIn -= EnterGamePaused;
		}

		public void ExitGamePaused()
		{
			_pauseParent.SetActive(false);
			OnFadedIn -= ExitSettings;
		}

		#endregion

		#region Settings

		public void UpdateUIFromSettings(SaveData data)
		{
			_settingsParent.SetActive(true);

			Slider_MusicVolume.value = data.MusicVolume;
			Slider_SFXVolume.value = data.SFXVolume;
			Toggle_MusicMute.isOn = data.MusicMuted;
			Toggle_SFXMuted.isOn = data.SFXMuted;
			Toggle_ShortCountdown.isOn = data.ShortCountdown;

			UpdateToggleVisual(_musicMuteText, data.MusicMuted);
			UpdateToggleVisual(_sfxMuteText, data.SFXMuted);
			UpdateToggleVisualGreyed(_shortCountdownText, data.ShortCountdown);

			int selectedRes = 0;
			for(int i = 0; i < RESOLUTIONS.Length; i++)
			{
				if (RESOLUTIONS[i].width == data.ResolutionWidth && 
					RESOLUTIONS[i].height == data.ResolutionHeight &&
					CURRENT_SCREEN_MODE == data.ScreenMode &&
					RESOLUTIONS[i].refreshRateRatio.numerator == data.RefreshRate &&
					RESOLUTIONS[i].refreshRateRatio.denominator == data.RefreshRateDenom) 
				{
					selectedRes = i;
				}
			}

			Dropdown_Resolutions.SetValueWithoutNotify(selectedRes);
			Dropdown_ScreenMode.SetValueWithoutNotify((int)data.ScreenMode);

			UnityEngine.Rendering.Universal.Bloom bloom;
			bool hasBloom = PPVolume.sharedProfile.TryGet(out bloom);
			if (hasBloom)
			{
				bloom.intensity.Override(data.BloomIntensity);
				Slider_Bloom.value = 1 / (BloomIntensityMax / bloom.intensity.value);
			}

			_settingsParent.SetActive(false);
		}

		public void EnterSettings()
		{
			_settingsParent.SetActive(true);
			OnFadedIn -= EnterSettings;
			StartFadeOut();
		}

		public void ExitSettings()
		{
			_settingsParent.SetActive(false);
			OnFadedIn -= ExitSettings;
		}

		public void ShowSettingsAppliedText()
		{
			_settingsAppliedText.EnableOneTimeGlowFlicker();
		}

		public void UpdateToggleVisual(CustomTextProperties txt, bool mute)
		{
			Color mc1 = mute ? _timeGainLoseColor : _timeGainAddColor;
			Color mc2 = mute ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			txt.SetColor(mc1, mc2);
		}

		public void UpdateToggleVisualGreyed(CustomTextProperties txt, bool mute)
		{
			Color mc1 = mute ? _timeGainAddColor : _greyedColor;
			Color mc2 = mute ? _timeGainAddGlowColor : _greyedColor;
			txt.SetColor(mc1, mc2);
		}

		#endregion

		#region State Updates

		private void UpdateScreenFade()
		{
			if (_fadeScreenState == 1)
			{
				_currFadeLerp += Time.deltaTime * FadeSpeed;
				_fadeScreen.color = Color.Lerp(_currFadeColor, _fadeColorIn, _currFadeLerp);
				if (_currFadeLerp >= 1)
				{
					Game.Log(LogTypes.UI, $"Finish Fade In.");
				}
			}
			else if (_fadeScreenState == 2)
			{
				_currFadeLerp += Time.deltaTime * FadeSpeed;
				_fadeScreen.color = Color.Lerp(_currFadeColor, _fadeColorOut, _currFadeLerp);
				if (_currFadeLerp >= 1)
				{
					Game.Log(LogTypes.UI, $"Finish Fade Out.");
					_fadeScreen.raycastTarget = false;
				}
			}
		}

		private void LateUpdateScreenFade()
		{
			if (_fadeScreenState == 1 && _currFadeLerp >= 1)
			{
				_fadeScreenState = 0;
				if (OnFadedIn != null)
					OnFadedIn();
				return;
			}

			if (_fadeScreenState == 2 && _currFadeLerp >= 1)
			{
				_fadeScreenState = 0;
				if (OnFadedOut != null)
					OnFadedOut();
			}
		}

		private void UpdateNoticePanel()
		{
			if(_noticePanelState == 0)
			{
				return;
			}

			if (_noticePanelState == 1)
			{
				_noticePanelLerp += NoticePanelSpeed * Time.deltaTime;
				_noticePanel.anchoredPosition = Vector3.Lerp(_noticePanel.anchoredPosition, _noticePanelDestPos, _noticePanelLerp);
				if (_noticePanelLerp >= 1)
				{
					_noticePanelState = 2;
					_noticePanelLerp = 0f;
				}

				return;
			}

			_noticePanelLingerTimer += Time.deltaTime;
			if (_noticePanelLingerTimer >= NoticePanelLingerTime)
			{
				_noticePanelLerp += NoticePanelSpeed * Time.deltaTime;
				_noticePanel.anchoredPosition = Vector3.Lerp(_noticePanel.anchoredPosition, _noticePanelOriginalPos, _noticePanelLerp);
				if (_noticePanelLerp >= 1)
				{
					_noticePanelState = 0;
				}
			}
		}

		#endregion

		#region System

		public void StartFadeIn()
		{
			_fadeScreen.raycastTarget = true;
			_currFadeColor = _fadeScreen.color;
			_currFadeLerp = 0f;
			_fadeScreenState = 1;
			Game.Log(LogTypes.UI, $"Start Fade In.");
		}

		public void StartFadeOut()
		{
			_fadeScreen.raycastTarget = true;
			_currFadeColor = _fadeScreen.color;
			_currFadeLerp = 0f;
			_fadeScreenState = 2;
			Game.Log(LogTypes.UI, $"Start Fade Out.");
		}

		public void EnterCountdown()
		{
			_getReadyParent.SetActive(false);
			_countdownParent.SetActive(true);
		}

		public void EnableCountdown(bool enable)
		{
			_countdownParent.SetActive(enable);
		}

		public void SetCountdownText(string text)
		{
			_countdownText.text = text;
		}

		public void SetTimer(string[] time)
		{
			_timeTextMin_D0.text = time[0][0].ToString();
			_timeTextMin_D1.text = time[0][1].ToString();

			_timeTextSec_D0.text = time[1][0].ToString();
			_timeTextSec_D1.text = time[1][1].ToString();

			_timeTextMs_D0.text = time[2][0].ToString();
			_timeTextMs_D1.text = time[2][1].ToString();
		}

		public void CreateTimeEntry(string time, string prefix, string moddedTime, bool added)
		{
			CanvasText txt = Instantiate(_timeEntryPrefab, _timeEntryParent);
			string t = added ? "+" : "-";
			txt.text = $"{time} > {prefix} {t} {moddedTime}";
			_currTimeEntries.Add(txt.gameObject);
		}

		public void ClearTimeEntries()
		{
			for (int i = 0; i < _currTimeEntries.Count; i++)
			{
				Destroy(_currTimeEntries[i]);
			}
		}

		public void SetTimeGainText(bool add, string time)
		{
			if (time.Length == 0)
			{
				_timeGainSign.SetText("");
				_timeGainText_D0.SetText("");
				_timeGainText_Dec.SetText("");
				_timeGainText_D1.SetText("");
				_timeGainText_Unit.SetText("");
				return;
			}

			Color c = add ? _timeGainAddColor : _timeGainLoseColor;
			Color cGlow = add ? _timeGainAddGlowColor : _timeGainLoseGlowColor;
			string sign = add ? "+" : "-";

			_timeGainSign.SetText(sign);
			_timeGainSign.SetColor(c, cGlow);
			_timeGainText_D0.SetText(time[0].ToString());
			_timeGainText_D0.SetColor(c, cGlow);
			_timeGainText_Dec.SetText(time[1].ToString());
			_timeGainText_Dec.SetColor(c, cGlow);
			_timeGainText_D1.SetText(time[2].ToString());
			_timeGainText_D1.SetColor(c, cGlow);
			_timeGainText_Unit.SetText("SECONDS");
			_timeGainText_Unit.SetColor(c, cGlow);
		}

		public void ActivateNoticePanel(bool isMiss)
		{
			_noticePanelLingerTimer = 0f;
			_noticePanelState = 1;
			_noticePanel.anchoredPosition = _noticePanelOriginalPos;
			_noticePanelLerp = 0f;

			if (isMiss)
			{
				_noticePanelTextProps.SetText("MISS");
				_noticePanelTextProps.SetColor(_timeGainLoseColor, _timeGainLoseGlowColor);
				_noticePanelBorderImage.color = _timeGainLoseColor;
				return;
			}

			_noticePanelTextProps.SetText("PERFECT");
			_noticePanelTextProps.SetColor(_timeGainAddColor, _timeGainAddGlowColor);
			_noticePanelBorderImage.color = _timeGainAddColor;
		}

		public void ShowFinish(bool fail = false)
		{
			_noticePanel.gameObject.SetActive(false);
			_timeParent.gameObject.SetActive(false);

			Color c = fail ? _timeGainLoseColor : _timeGainAddColor;
			Color cGlow = fail ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			string msg = fail ? "FAIL" : "WINNER";

			_finishTextProps.SetColor(c, cGlow);
			_finishTextProps.SetText(msg);
			_finishParent.SetActive(true);
			_finishParentBorder.color = c;
		}

		public void ShowTimeout()
		{
			_timeoutParent.SetActive(true);
		}

		public void GenerateResolutions()
		{
			RESOLUTIONS = Screen.resolutions;

			List<Resolution> reses = new List<Resolution>(RESOLUTIONS);
			reses.Reverse();

			RESOLUTIONS = reses.ToArray();

			Dropdown_Resolutions.ClearOptions();
			List<string> options = new();

			foreach (var res in RESOLUTIONS)
			{
				options.Add($"{res.width} x {res.height} {res.refreshRateRatio.numerator}Hz");
			}

			Dropdown_Resolutions.AddOptions(options);
		}

		#endregion
	}
}

