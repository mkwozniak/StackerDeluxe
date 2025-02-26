using System;
using System.Collections.Generic;
using UnityEngine;
using CanvasText = TMPro.TextMeshProUGUI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
using Dropdown = TMPro.TMP_Dropdown;

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

		[Header("References")]
		public ButtonVisualModifier DifficultyBtn_Normal;
		public ButtonVisualModifier DifficultyBtn_Hard;
		public ButtonVisualModifier DifficultyBtn_Expert;
		public ButtonVisualModifier RestartBtn_GameOver;
		public ButtonVisualModifier ExitBtn_GameOver;
		public ButtonVisualModifier RestartBtn_Win;
		public ButtonVisualModifier ExitBtn_Win;
		public ButtonVisualModifier SettingsBtn_Menu;
		public ButtonVisualModifier ExitSettingsBtn;
		public ButtonVisualModifier ApplySettingsBtn;
		public Slider Slider_MusicVolume;
		public Slider Slider_SFXVolume;
		public Slider Slider_Bloom;
		public Toggle Toggle_MusicMute;
		public Toggle Toggle_SFXMuted;
		public Dropdown Dropdown_Resolutions;
		public Dropdown Dropdown_ScreenMode;
		public UnityEngine.Rendering.Volume PPVolume;
		public float BloomIntensityMax = 0.5f;

		[SerializeField] Image _fadeScreen;
		[SerializeField] CanvasText _timeEntryPrefab;

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
		[ColorUsage(true, true), SerializeField] Color _timeGainAddGlowColor;
		[ColorUsage(true, true), SerializeField] Color _timeGainLoseGlowColor;

		[Header("Settings Panel")]
		[SerializeField] CustomTextProperties _settingsAppliedText;
		[SerializeField] CustomTextProperties _musicMuteText;
		[SerializeField] CustomTextProperties _sfxMuteText;

		[Header("Notice Panel")]
		[SerializeField] CustomTextProperties _noticePanelTextProps;
		[SerializeField] RectTransform _noticePanel;
		[SerializeField] Image _noticePanelBorderImage;

		[Header("Game Over / Finish")]
		[SerializeField] CustomTextProperties _finishTextProps;
		[SerializeField] Image _finishParentBorder;

		[Header("States")]
		[SerializeField] float _currFadeLerp = 0f;
		[SerializeField] bool _isFading = false;
		[SerializeField] byte _fadeScreenState = 0;

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

		// local members //

		#region Unity Methods

		private void Start()
		{
			Vector3 origPos = _noticePanel.anchoredPosition;
			_noticePanelOriginalPos = origPos;
			_noticePanelDestPos = new Vector3(origPos.x, _noticePanel.anchoredPosition.y - NoticePanelMoveDistance, origPos.z);
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
			OnFadedIn -= ExitMainMenu;
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
			_gameOverParent.SetActive(false);
		}

		public void EnterPregame()
		{
			_gameParent.SetActive(true);
			_getReadyParent.SetActive(true);
			_countdownParent.SetActive(false);
		}

		#endregion Game

		#region Game Over

		public void EnterGameOver()
		{
			_gameOverParent.SetActive(true);
			_finishParent.SetActive(false);
		}

		public void ExitGameOver()
		{
			OnFadedIn -= ExitGameOver;
			_gameOverParent.SetActive(false);
			_finishParent.SetActive(false);
		}

		#endregion Game Over

		#region Game Win

		public void ExitGameWin()
		{
			OnFadedOut -= ExitGameWin;
			_winParent.SetActive(false);
			_finishParent.SetActive(false);
		}
		public void EnterGameWin()
		{
			_winParent.SetActive(true);
		}

		#endregion Game Win

		#region Settings

		public void UpdateUIFromSettings(SaveData data)
		{
			_settingsParent.SetActive(true);

			Slider_MusicVolume.value = data.MusicVolume;
			Slider_SFXVolume.value = data.SFXVolume;
			Toggle_MusicMute.isOn = data.MusicMuted;
			Toggle_SFXMuted.isOn = data.SFXMuted;

			Color mc1 = data.MusicMuted ? _timeGainLoseColor : _timeGainAddColor;
			Color mc2 = data.MusicMuted ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			_musicMuteText.SetColor(mc1, mc2);

			Color sc1 = data.SFXMuted ? _timeGainLoseColor : _timeGainAddColor;
			Color sc2 = data.SFXMuted ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			_sfxMuteText.SetColor(sc1, sc2);
			
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

		public void UpdateMusicMuteToggleVisual(bool mute)
		{
			Color mc1 = mute ? _timeGainLoseColor : _timeGainAddColor;
			Color mc2 = mute ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			_musicMuteText.SetColor(mc1, mc2);
		}

		public void UpdateSFXMuteToggleVisual(bool mute)
		{
			Color mc1 = mute ? _timeGainLoseColor : _timeGainAddColor;
			Color mc2 = mute ? _timeGainLoseGlowColor : _timeGainAddGlowColor;
			_sfxMuteText.SetColor(mc1, mc2);
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

