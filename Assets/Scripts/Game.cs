using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace wozware.StackerDeluxe
{
	public sealed partial class Game : MonoBehaviour
	{
		// Global Settings //
		public static StackerSoundCrawlModes STACKER_CRAWL_SOUND_MODE = StackerSoundCrawlModes.Arpeggio;

		public static string SAVE_FILE_NAME = "save.dat";

		[Header("Settings")]
		[SerializeField] int _startingRow = -7;
		[SerializeField] float _gameFinishDestroyDelay = 0.25f;
		[SerializeField] float _defaultCameraLerpSpeed = 2f;
		[SerializeField] float _defaultCameraDampSpeed = 2f;
		[SerializeField] float _nextStackerCameraLerpSpeed = 4f;
		[SerializeField] float _musicFadeSpeed = 1.0f;
		[SerializeField] float _gameFinishDelay = 1f;
		[SerializeField] float _gameOverFinalDelay = 1f;
		[SerializeField] float _gameWinDelay = 2f;

		[Header("References")]
		[SerializeField] StackerData _gameData;
		[SerializeField] UI _ui;
		[SerializeField] Camera _camera;
		[SerializeField] GameObject _stageParent;
		[SerializeField] StackerRow _stackerRowPrefab;
		[SerializeField] Transform _stackerParent;
		[SerializeField] Transform _particleParent;
		[SerializeField] Transform _bottomStage;
		[SerializeField] Transform _emptyPoint;
		[SerializeField] Transform _challengerWorldParent;
		[SerializeField] AudioSource _musicSource;
		[SerializeField] Transform _sfxParent;
		[SerializeField] AudioSource _sfxPrefab;
		[SerializeField] GameObject _mainMenuParticleParent;
		[SerializeField] MeshRenderer _gridRenderer;
		[SerializeField] Material _skybox0;
		[SerializeField] Material _skybox1;
		[SerializeField] Material _skybox2;
		[SerializeField] Material _defaultGridMaterial;
		[SerializeField] Material _loseGridMaterial;

		[Header("Gameplay States")]
		[SerializeField] GameStates _gameState;
		[SerializeField] StackerLevel _currLevel;
		[SerializeField] StackerRow _currStackerRow;
		[SerializeField] LevelDifficulties _currDifficulty;
		[SerializeField] bool _inCountdown = false;
		[SerializeField] bool _canPlaceStacker = false;
		[SerializeField] float _currCountdown = 3f;
		[SerializeField] int _currRow = 0;
		[SerializeField] int _currHeight = 0;
		[SerializeField] int _startingThickness = 3;
		[SerializeField] int _currThickness = 0;
		[SerializeField] float _currTime;
		[SerializeField] float _currTimeLeft;
		[SerializeField] float _gameFinishDestroyTimer = 0f;
		[SerializeField] float _gameFinishTimer = 0f;
		[SerializeField] float _finalGameOverTimer = 0f;
		[SerializeField] float _finalGameWinTimer = 0f;
		[SerializeField] bool _inFinalGameOver = false;
		[SerializeField] bool _inFinalGameWin = false;
		[SerializeField] SoundIDs _nextMusicID = SoundIDs.Empty;
		[SerializeField] byte _musicState = 0;
		[SerializeField] bool _timerEnabled = false;
		[SerializeField] bool _inSettingsFromPause = false;

		[Header("Camera States")]
		[SerializeField] bool _lerpingCamera = false;
		[SerializeField] float _lerpCam = 0f;
		[SerializeField] float _lerpCamSpeed = 0f;
		[SerializeField] Vector3 _finalCameraLerpPos = Vector3.zero;
		[SerializeField] Vector3 _startCameraLerpPos = Vector3.zero;
		[SerializeField] bool _dampingCamera = false;
		[SerializeField] float _dampCamSpeed = 0f;
		[SerializeField] Vector3 _finalCameraDampPos = Vector3.zero;

		// Cached members //
		Vector3 _camDampVelocity = Vector3.zero;
		float _currCountdownInterval = 0.0f;
		private string _saveFilePath = "";
		SaveData _saveData;
		private GameStates _toSettingsState;

		// Dynamic gameplay structures //
		Stack<StackerRow> _currStackerRows = new();

		// Func pointers for each game state //
		Dictionary<GameStates, Action> _gameUpdates = new();

		#region Unity Methods

		private void Awake()
		{
			_gameData.Initialize();
			_saveFilePath = GetSaveFilePath();
			CreateOrLoadSaveFile();
			_ui.GenerateResolutions();
			_ui.UpdateUIFromSettings(_saveData);
			Screen.SetResolution(_saveData.ResolutionWidth, _saveData.ResolutionHeight, _saveData.ScreenMode);

			// link actions
			LinkGameEvents();
			// link ui events
			LinkUIEvents();

			// populate achievements
			InitializeAchievements(LevelDifficulties.Debug);
			InitializeAchievements(LevelDifficulties.Normal);
			InitializeAchievements(LevelDifficulties.Hard);
			InitializeAchievements(LevelDifficulties.Expert);

			_stageParent.SetActive(false);
		}

		private void Start()
		{
			EnterMenuMode();
		}

		private void Update()
		{
			UpdateGlobal();
			_gameUpdates[_gameState]();
		}

		#endregion

		#region System

		public static void Log(LogTypes type, string message, byte msgType = 0)
		{
			if (msgType == 1)
			{
				Debug.LogWarning($"[{type.ToString()}] - {message}");
				return;
			}

			if(msgType == 2)
			{
				Debug.LogError($"[{type.ToString()}] - {message}");
				return;
			}

			Debug.Log($"[{type.ToString()}] - {message}");
		}

		private string GetSaveFilePath()
		{
			return $"{Application.persistentDataPath}/{SAVE_FILE_NAME}";
		}

		private void CreateOrLoadSaveFile()
		{
			if (!File.Exists(_saveFilePath))
			{
				FileStream file;
				BinaryFormatter formatter = new BinaryFormatter();
				SaveData data = new SaveData();

				// set default save data
				data.MusicVolume = 1f;
				data.SFXVolume = 1f;
				data.ShortCountdown = false;

				UI.CURRENT_RESOLUTION = Screen.currentResolution;
				UI.CURRENT_SCREEN_MODE = Screen.fullScreenMode;

				data.ResolutionWidth = UI.CURRENT_RESOLUTION.width;
				data.ResolutionHeight = UI.CURRENT_RESOLUTION.height;
				data.ScreenMode = UI.CURRENT_SCREEN_MODE;
				data.RefreshRate = UI.CURRENT_RESOLUTION.refreshRateRatio.numerator;
				data.RefreshRateDenom = UI.CURRENT_RESOLUTION.refreshRateRatio.denominator;
				data.BloomIntensity = _ui.BloomIntensityMax;
				data.LevelRecords = new List<LevelRecord>(RecordKeeper.LVL_RECORDS.Values);

				Log(LogTypes.FILE, "Save data in path does not exist. Creating new default file.");
				file = File.Create(_saveFilePath);
				formatter.Serialize(file, data);
				file.Close();
				_saveData = data;
				return;
			}

			LoadFromSaveFile();
		}

		private void LoadFromSaveFile()
		{
			FileStream file;
			BinaryFormatter formatter = new BinaryFormatter();
			Log(LogTypes.FILE, "Loading save data from existing file.");
			file = File.Open(_saveFilePath, FileMode.Open);
			SaveData data = (SaveData)formatter.Deserialize(file);

			for(int i = 0; i < data.LevelRecords.Count; i++)
			{
				RecordKeeper.LVL_RECORDS[data.LevelRecords[i].Name] = data.LevelRecords[i];
			}

			// load save data
			_saveData = data;

			file.Close();
		}

		private void SaveToSaveFile()
		{
			FileStream file;
			BinaryFormatter formatter = new BinaryFormatter();
			file = File.Open(_saveFilePath, FileMode.Open);
			formatter.Serialize(file, _saveData);
			file.Close();
			Log(LogTypes.FILE, "Saved current save data to file.");
		}

		private void SetMusicVolume(float val)
		{
			_saveData.MusicVolume = val;
			_musicSource.volume = _saveData.MusicVolume;
		}

		private void SetSFXVolume(float val)
		{
			_saveData.SFXVolume = val;
		}

		private void MuteMusic(bool val)
		{
			_saveData.MusicMuted = val;
			_musicSource.mute = _saveData.MusicMuted;
			if(!val)
			{
				_musicSource.volume = _saveData.MusicVolume;
			}
		}

		private void MuteSFX(bool val)
		{
			_saveData.SFXMuted = val;
		}

		#endregion

		#region Menu

		private void EnterMenuMode()
		{
			_ui.OnFadedIn -= EnterMenuMode;
			_ui.EnterMainMenu();
			_gameState = GameStates.MainMenu;
			RenderSettings.skybox = _skybox0;
			SetMusic(SoundIDs.MusicMainMenu);
			_mainMenuParticleParent.SetActive(true);
			_ui.StartFadeOut();
		}

		private void EnterSettingsMode()
		{
			_ui.OnFadedIn -= EnterSettingsMode;
			RenderSettings.skybox = _skybox2;
		}

		private void SetMenuSkybox()
		{
			_ui.OnFadedIn -= SetMenuSkybox;
			RenderSettings.skybox = _skybox0;
		}

		private void InitializeAchievements(LevelDifficulties difficulty)
		{
			StackerLevel lvl = null;
			bool hasLevel = _gameData.TryGetStandardStackerLevel(difficulty, ref lvl);
			if(!hasLevel)
			{
				Debug.LogError($"Cannot Initialize Achievements for {difficulty}. Level does not exist.");
				return;
			}

			if (lvl.Achievements.Count > RecordKeeper.LVL_RECORDS[lvl.Name].Achievements.Count)
			{
				for (int i = RecordKeeper.LVL_RECORDS[lvl.Name].Achievements.Count;
					i < lvl.Achievements.Count; i++)
				{
					RecordKeeper.LVL_RECORDS[lvl.Name].Achievements.Add(lvl.Achievements[i]);
				}
			}

			_ui.PopulateAchievements(lvl.Name);
		}

		private void SetAchievementSkybox()
		{
			_ui.OnFadedIn -= SetAchievementSkybox;
			RenderSettings.skybox = _skybox2;
		}

		private void EnterAchievementWinMode()
		{
			_ui.OnFadedIn -= EnterAchievementWinMode;
			_ui.CurrentAchievementWinIndex = 0;
			_ui.DestroyWinAchievements();
			StartCoroutine(_ui.PopulateWinAchievements(_currLevel.Name));
		}

		#endregion Menu

		#region Game

		private void EnterPregameMode()
		{
			_mainMenuParticleParent.SetActive(false);
			_ui.OnFadedIn -= EnterPregameMode;
			_stageParent.SetActive(true);
			_ui.ExitGamePaused();
			_gridRenderer.material = _defaultGridMaterial;
			_ui.EnterPregame();

			RenderSettings.skybox = _skybox1;
			ResetActiveGame();
			_ui.StartFadeOut();
			_ui.OnFadedOut += FinishPregameMode;
			SetMusic(SoundIDs.Empty);
		}

		private void FinishPregameMode()
		{
			_ui.OnFadedOut -= FinishPregameMode;
			StartCameraDamp(_bottomStage.position, _defaultCameraDampSpeed);
			OnCameraFinishDamp += StartGame;
		}

		private void ExitGameMode()
		{
			_stageParent.SetActive(false);
			_ui.ExitGame();
		}

		private void ResetActiveGame()
		{
			_currRow = _startingRow;
			_startingThickness = _currLevel.StartingThickness;
			_currThickness = _startingThickness;
			_currHeight = 0;
			_canPlaceStacker = false;
			_finalGameOverTimer = 0f;
			_finalGameWinTimer = 0f;
			_gameFinishTimer = 0f;
			_gameFinishDestroyTimer = 0f;
			RecordKeeper.ResetCurrentRecords();
			_ui.ClearTimeEntries();
		}

		private void StartGame()
		{
			_ui.EnterCountdown();
			_currTimeLeft = _currLevel.TimeLeft;
			_ui.SetTimer(GetTimeStrings(_currTimeLeft));
			EnableCountdown(true);
			_gameState = GameStates.GameActive;
			OnCameraFinishDamp -= StartGame;
		}

		private void EnterGameAtDifficulty(LevelDifficulties difficulty)
		{
			if (!TrySelectStandardLevel(difficulty))
			{
				return;
			}

			_ui.StartFadeIn();
			_ui.OnFadedIn += EnterPregameMode;
			_ui.OnFadedIn += _ui.ExitMainMenu;
		}

		private void EnterChallengerWorld()
		{
			_ui.StartFadeIn();
			_ui.OnFadedIn += _ui.ExitMainMenu;
			_ui.OnFadedIn += OpenChallengerWorld;
		}

		private void ExitChallengerWorld()
		{
			_ui.StartFadeIn();
			_ui.OnFadedIn += _ui.EnterMainMenu;
			_ui.OnFadedIn += MoveCameraToEmptyPoint;
			_ui.OnFadedIn += CloseChallengerWorld;
		}

		private void OpenChallengerWorld()
		{
			_ui.EnterChallenger();
			_challengerWorldParent.gameObject.SetActive(true);
			SetMusic(SoundIDs.MusicChallenger);
			StartCameraDamp(_challengerWorldParent.position, _defaultCameraDampSpeed);
			_ui.OnFadedIn -= OpenChallengerWorld;
			_ui.StartFadeOut();
		}

		private void CloseChallengerWorld()
		{
			_ui.ExitChallenger();
			SetMusic(SoundIDs.MusicMainMenu);
			_challengerWorldParent.gameObject.SetActive(false);
			_ui.OnFadedIn -= CloseChallengerWorld;
		}

		private void EnterPause()
		{
			_gameState = GameStates.GamePaused;
			PauseCurrentRowMusic(true);
			_ui.EnterGamePaused();
			_ui.ExitGame();
			_currStackerRow.Pause(true);
		}

		private void ExitPause(bool toExit)
		{
			if(toExit)
			{
				_ui.ExitGamePaused();
				return;
			}

			_gameState = GameStates.GameActive;
			_ui.EnterGame();
			_ui.ExitGamePaused();
			PauseCurrentRowMusic(false);
			_currStackerRow.Pause(false);
		}

		private void MoveCameraToEmptyPoint()
		{
			StartCameraDamp(_emptyPoint.transform.position, _defaultCameraDampSpeed);
			_ui.OnFadedIn -= MoveCameraToEmptyPoint;
		}

		private void MoveCameraBackToStackerRow()
		{
			Vector3 nextDampPos = new Vector3(_camera.transform.position.x, 
				_currStackerRow.transform.position.y, 
				_camera.transform.position.z);
			StartCameraDamp(nextDampPos, _defaultCameraDampSpeed);
			_ui.OnFadedIn -= MoveCameraBackToStackerRow;
		}

		#endregion Game

		#region Game Over

		private void EnterGameOverMode()
		{
			_ui.StartFadeIn();
			_ui.OnFadedIn += FinishGameOverEnter;
			StartCameraDamp(_emptyPoint.position, _defaultCameraDampSpeed / 2f);
		}

		private void FinishGameOverEnter()
		{
			_ui.OnFadedIn -= FinishGameOverEnter;
			ExitGameMode();
			_ui.ExitGamePaused();
			_ui.StartFadeOut();
			RenderSettings.skybox = _skybox0;
			_ui.EnterGameOver();
		}

		private void TriggerGameOver(bool timeout = false)
		{
			_gameState = GameStates.GameOver;
			_ui.ShowFinish(fail: true);
			_gridRenderer.material = _loseGridMaterial;

			if(OnGameOver != null)
			{
				OnGameOver();
			}

			_gameFinishDestroyTimer = 0f;
			_finalGameOverTimer = 0f;
			_finalGameWinTimer = 0f;
			_inFinalGameOver = false;
			SetMusic(SoundIDs.MusicGameOver);
			if (timeout)
			{
				_currStackerRow.Active = false;
				_ui.ShowTimeout();
				CreateSFX(SoundIDs.VFX_Timeout);
				return;
			}
			CreateSFX(SoundIDs.VFX_GameOver);
		}

		#endregion Game Over

		#region Game Win

		private void EnterGameWin()
		{
			_ui.StartFadeIn();
			_ui.OnFadedIn += FinishGameWinEnter;
			StartCameraDamp(_emptyPoint.position, _defaultCameraDampSpeed / 2f);
		}

		private void FinishGameWinEnter()
		{
			_ui.OnFadedIn -= FinishGameWinEnter;
			ExitWinMode();
			_ui.ExitGamePaused();
			_ui.StartFadeOut();
			RenderSettings.skybox = _skybox2;
			_ui.EnterGameWin();

			_ui.SetGameWinLabels(_currLevel.Name, GetTimeStrings(_currTimeLeft), 
				GetTimeStrings(RecordKeeper.LVL_RECORDS[_currLevel.Name].TimeLeft),
				RecordKeeper.MISSED.ToString(), RecordKeeper.PERFECTS.ToString());

			UpdateWinRecords();

			SaveToSaveFile();
		}

		private void ExitWinMode()
		{
			_stageParent.SetActive(false);
			_ui.ShowNewRecordLabel(false);
			_ui.ShowPerfectScoreLabel(false);
			_ui.ExitGameWin();
		}

		private void TriggerGameWin()
		{
			_gameState = GameStates.GameWin;
			_ui.ShowFinish();
			_inFinalGameWin = false;
			_finalGameWinTimer = 0f;
			SetMusic(SoundIDs.MusicWin);
			CreateSFX(SoundIDs.VFX_GameWin);
		}

		private void UpdateWinRecords()
		{
			float timeRecord = RecordKeeper.LVL_RECORDS[_currLevel.Name].TimeLeft;
			if (_currTimeLeft > timeRecord)
			{
				timeRecord = _currTimeLeft;
				CreateSFX(SoundIDs.VFX_NewRecord);
				_ui.ShowNewRecordLabel(true);
			}

			if (RecordKeeper.MISSED <= 0)
			{
				_ui.ShowPerfectScoreLabel(true);
			}

			for (int i = 0; i < _saveData.LevelRecords.Count; i++)
			{
				if (_currLevel.Name == _saveData.LevelRecords[i].Name)
				{
					UpdateWinAchievements(i, timeRecord);
					Debug.Log($"Saving Record Data from RecordKeeper: {_currLevel.Name}");
					break;
				}
			}
		}

		private void UpdateWinAchievements(int index, float timeRecord)
		{
			List<Achievement> list = RecordKeeper.LVL_RECORDS[_currLevel.Name].Achievements;

			for (int i = 0; i < list.Count; i++)
			{
				Achievement a = list[i];
				Debug.Log(RecordKeeper.LVL_RECORDS[_currLevel.Name].Achievements[i].Complete);
				if (RecordKeeper.LVL_RECORDS[_currLevel.Name].Achievements[i].Complete)
					continue;

				bool complete = false;

				if (a.AchievementType == AchievementTypes.Time)
				{
					if(a.TimeThreshold <= _currTimeLeft)
					{
						a.Complete = true;
						complete = true;
					}
				}

				if(a.AchievementType == AchievementTypes.Clear)
				{
					a.Complete = true;
					complete = true;
				}

				if(a.AchievementType == AchievementTypes.Perfect)
				{
					if (RecordKeeper.MISSED <= 0)
					{
						a.Complete = true;
						complete = true;
					}
				}

				if(complete)
				{
					list[i] = new Achievement(list[i].Name, list[i].Description, list[i].AchievementType, list[i].TimeThreshold, true);
					_ui.UIAchievements[a.Description].DoneObject.SetActive(true);
					_ui.NumCompletedAchievements += 1;
				}
			}

			RecordKeeper.LVL_RECORDS[_currLevel.Name] =
				new LevelRecord(_currLevel.Name, timeRecord,
				RecordKeeper.MISSED, RecordKeeper.PERFECTS, list);

			_saveData.LevelRecords[index] = RecordKeeper.LVL_RECORDS[_currLevel.Name];
		}

		#endregion Game Win

		#region State Update

		private void UpdateGlobal()
		{
			UpdateCameraSmoothDamp();
			UpdateCameraLerp();
			UpdateMusic();
		}

		private void UpdateMainMenu()
		{
			_ui.UpdateMenuCredits();
		}

		private void UpdateGameActive()
		{
			if (_inCountdown)
			{
				UpdatePregameCountdown();
				return;
			}

			if(Input.GetKeyDown(KeyCode.Escape) && _canPlaceStacker)
			{
				EnterPause();
				return;
			}

			if(_timerEnabled)
			{
				UpdateTimer();
			}

			if (Input.GetMouseButtonDown(0) && _canPlaceStacker)
			{
				PlaceCurrentStackerRow();
				_canPlaceStacker = false;
				return;
			}
		}

		private void UpdateGamePaused()
		{
			if (Input.GetKeyDown(KeyCode.Escape) && !_inSettingsFromPause)
			{
				ExitPause(toExit: false);
				return;
			}
		}

		private void UpdateGameOver()
		{
			if (_inFinalGameOver)
			{
				return;
			}

			_gameFinishTimer += Time.deltaTime;
			if(_gameFinishTimer < _gameFinishDelay)
			{
				return;
			}

			_gameFinishDestroyTimer += Time.deltaTime;
			if (_gameFinishDestroyTimer > _gameFinishDestroyDelay)
			{
				UpdateGameOverCleanup();
			}
		}

		private void UpdateGameOverCleanup()
		{
			if (!TryUpdateRowPop())
				return;

			_finalGameOverTimer += Time.deltaTime;
			if (_finalGameOverTimer >= _gameOverFinalDelay)
			{
				_inFinalGameOver = true;
				EnterGameOverMode();
				return;
			}
		}

		private void UpdateGameWin()
		{
			if (_inFinalGameWin)
			{
				return;
			}

			_gameFinishTimer += Time.deltaTime;
			if (_gameFinishTimer < _gameFinishDelay)
			{
				return;
			}

			_gameFinishDestroyTimer += Time.deltaTime;
			if (_gameFinishDestroyTimer > _gameFinishDestroyDelay)
			{
				UpdateGameWinCleanup();
			}
		}

		private void UpdateGameWinCleanup()
		{
			if (!TryUpdateRowPop())
				return;

			_finalGameWinTimer += Time.deltaTime;
			if (_finalGameWinTimer >= _gameWinDelay)
			{
				_inFinalGameWin = true;
				EnterGameWin();
				return;
			}
		}

		private void UpdateMusic()
		{
			if(_saveData.MusicMuted)
			{
				if(_musicSource.volume > 0)
					_musicSource.volume = 0;
				return;
			}

			// fading out //
			if(_musicState == 1)
			{
				_musicSource.volume -= _musicFadeSpeed * Time.deltaTime;
				if (_musicSource.volume <= 0)
				{
					if(_nextMusicID == SoundIDs.Empty)
					{
						_musicSource.Stop();
						_musicState = 0;
						return;
					}

					_musicSource.clip = _gameData.GetSound(_nextMusicID);
					_musicSource.Play();
					_musicState = 2;
					return;
				}
			}

			// fading in //
			if(_musicState == 2)
			{
				_musicSource.volume += _musicFadeSpeed * Time.deltaTime;
				if(_musicSource.volume >= _saveData.MusicVolume)
				{
					_musicState = 0;
				}
			}	
		}

		private void UpdateCameraSmoothDamp()
		{
			if (!_dampingCamera)
			{
				return;
			}

			_camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, _finalCameraDampPos, ref _camDampVelocity, _dampCamSpeed);

			if (Vector3.Distance(_camera.transform.position, _finalCameraDampPos) <= 0.05f)
			{
				_dampingCamera = false;
				if (OnCameraFinishDamp != null)
				{
					OnCameraFinishDamp();
					_camera.transform.position = _finalCameraDampPos;
				}
			}
		}

		private void UpdateCameraLerp()
		{
			if (!_lerpingCamera)
			{
				return;
			}

			_lerpCam += Time.deltaTime * _lerpCamSpeed;
			_camera.transform.position = Vector3.Lerp(_camera.transform.position, _finalCameraLerpPos, _lerpCam);

			if (_lerpCam >= 1)
			{
				if (OnCameraFinishLerp != null)
				{
					OnCameraFinishLerp();
				}

				_lerpingCamera = false;
			}
		}

		private void UpdatePregameCountdown()
		{
			_currCountdown -= Time.deltaTime;
			_ui.SetCountdownText(((int)_currCountdown).ToString());

			if (_currCountdownInterval - _currCountdown >= 1.0f && _currCountdown > 0.98f)
			{
				CreateSFX(SoundIDs.Countdown, 0);
				_currCountdownInterval = _currCountdown;
			}

			if (_currCountdown <= 0)
			{
				FinishGameCountdown();
			}
		}

		private void UpdateTimer()
		{
			_currTime += Time.deltaTime;
			_currTimeLeft -= Time.deltaTime;
			if (_currTimeLeft <= 0)
			{
				_currTimeLeft = 0;
				_ui.SetTimer(GetTimeStrings(_currTimeLeft));
				TriggerGameOver(timeout: true);
				return;
			}

			_ui.SetTimer(GetTimeStrings(_currTimeLeft));
		}

		private bool TryUpdateRowPop()
		{
			StackerRow next;
			bool hasNext = _currStackerRows.TryPop(out next);
			if (hasNext)
			{
				Vector3 nextLerpPos = new Vector3(_camera.transform.position.x, next.transform.position.y, _camera.transform.position.z);
				StartCameraLerp(nextLerpPos, _defaultCameraLerpSpeed);
				next.DestroySelf();
				_gameFinishDestroyTimer = 0f;
				return false;
			}

			return true;
		}

		#endregion State Update

		#region Camera

		private void StartCamLerpToNextStackerRow()
		{
			Vector3 nextLerpPos = new Vector3(_camera.transform.position.x, _currStackerRow.transform.position.y, _camera.transform.position.z);
			OnCameraFinishLerp += FinishCamLerpToNextStacker;
			StartCameraLerp(nextLerpPos, _nextStackerCameraLerpSpeed);
		}

		private void FinishCamLerpToNextStacker()
		{
			_canPlaceStacker = true;
			OnCameraFinishLerp -= FinishCamLerpToNextStacker;

			if (_gameState != GameStates.GameOver && 
				STACKER_CRAWL_SOUND_MODE == StackerSoundCrawlModes.Arpeggio)
			{
				PlayCurrentRowMusic();
			}


			float nextSpeed = _currLevel.Rows[_currHeight].Speed;
			_currStackerRow.Initialize(_currThickness, nextSpeed, _currHeight);
			_currStackerRows.Push(_currStackerRow);
			_ui.SetTimeGainText(false, "");
			_currRow += 1;
			_currHeight += 1;
			_timerEnabled = true;
		}

		private void StartCameraLerp(Vector3 pos, float speed, bool overrideZ = false)
		{
			_lerpCam = 0f;
			_lerpCamSpeed = speed;
			_startCameraLerpPos = _camera.transform.position;
			_finalCameraLerpPos = pos;
			if (!overrideZ)
			{
				_startCameraLerpPos.z = _camera.transform.position.z;
				_finalCameraLerpPos.z = _camera.transform.position.z;
			}
			_lerpingCamera = true;
		}

		private void StartCameraDamp(Vector3 pos, float speed, bool overrideZ = false)
		{
			_dampCamSpeed = speed;
			_finalCameraDampPos = pos;
			_camDampVelocity = Vector3.zero;

			if (!overrideZ)
			{
				_finalCameraDampPos.z = _camera.transform.position.z;
			}
			_dampingCamera = true;
		}

		#endregion

		#region Gameplay

		private void PlayCurrentRowMusic()
		{
			_musicSource.volume = _saveData.MusicVolume;
			_musicSource.clip = _currLevel.Rows[_currHeight].MusicLoop;
			_musicSource.Play();
		}

		private void PauseCurrentRowMusic(bool pause)
		{
			if(pause)
			{
				_musicSource.Pause();
				return;
			}

			_musicSource.Play();

		}

		private bool TrySelectStandardLevel(LevelDifficulties difficulty)
		{
			Log(LogTypes.GAME, $"Try Selecte Difficulty: {difficulty}");
			return _gameData.TryGetStandardStackerLevel(difficulty, ref _currLevel);
		}

		private void EnableCountdown(bool enable)
		{
			_inCountdown = enable;
			_currCountdown = _saveData.ShortCountdown ? 1f : 4f;
			_currCountdownInterval = 4.0f;
			_ui.EnableCountdown(enable);
			if (enable)
				CreateSFX(SoundIDs.Countdown, 0);
		}

		private void FinishGameCountdown()
		{
			CreateSFX(SoundIDs.CountdownFinish, 0);
			EnableCountdown(false);
			_ui.EnterGame();
			SpawnStackerRow();
			StartCamLerpToNextStackerRow();
		}

		private void ReduceThickness()
		{
			_currThickness -= 1;
			if (_currThickness <= 0)
			{
				TriggerGameOver();
			}
		}

		private void SpawnStackerRow()
		{
			if (!_currLevel.Rows.ContainsKey(_currHeight))
			{
				Debug.LogError($"Current Level does not contain row at height {_currHeight}.");
				return;
			}

			int nextThickness = _currLevel.Rows[_currHeight].Thickness;
			if (nextThickness < _currThickness)
			{
				_currThickness = _currLevel.Rows[_currHeight].Thickness;
			}

			_currStackerRow = Instantiate(_stackerRowPrefab);
			_currStackerRow.transform.parent = _stackerParent;
			_currStackerRow.transform.localPosition = new Vector3(0, _currRow, 0);
		}

		private void PlaceCurrentStackerRow()
		{
			_musicSource.Stop();
			_timerEnabled = false;
			_currStackerRow.Stop();
		}

		private void ContinueNextStackerSpawn()
		{
			string timeGainStr = _currLevel.RowList[_currHeight].SecondsGain.ToString("0.0");
			float timeGain = _currStackerRow.DidMiss ? -1 : 1;
			_currTimeLeft += _currLevel.RowList[_currHeight].SecondsGain * timeGain;
			_ui.SetTimeGainText(!_currStackerRow.DidMiss, timeGainStr);
			_ui.ActivateNoticePanel(_currStackerRow.DidMiss);

			var record = _currStackerRow.DidMiss ? RecordKeeper.MISSED += 1 : RecordKeeper.PERFECTS += 1;

			CreateTimeEntry();
			SpawnStackerRow();
			StartCamLerpToNextStackerRow();
		}

		private bool CheckGameWin()
		{
			if (_currHeight > _currLevel.RowList.Count - 1)
			{
				TriggerGameWin();
				return true;
			}

			return false;
		}

		private void CreateTimeEntry()
		{ 
			string[] timeStrings = GetTimeStrings(_currTimeLeft);
			string[] timeModStrings = GetTimeStrings(_currLevel.RowList[_currHeight].SecondsGain);
			_ui.CreateTimeEntry($"{timeStrings[0]}:{timeStrings[1]}:{timeStrings[2]}",
				"PERFECT",
				$"{timeModStrings[0]}:{timeModStrings[1]}:{timeModStrings[2]}",
				true);
		}

		private float[] GetSplitTime(float time)
		{
			int minutes = (int)time / 60;
			int seconds = (int)time - 60 * minutes;
			int milliseconds = (int)(100 * (time - minutes * 60 - seconds));
			return new float[] { minutes, seconds, milliseconds };
		}

		private string[] GetTimeStrings(float time)
		{
			float[] split = GetSplitTime(time);
			string m = $"{string.Format("{0:00}", split[0])}";
			string s = $"{string.Format("{0:00}", split[1])}";
			string ms = $"{string.Format("{0:00}", split[2])}";
			return new string[] {m, s, ms};
		}

		private void CreateParticleFX(ParticleSystem prefab, Vector3 pos, float lifeTime)
		{
			ParticleSystem p = Instantiate(prefab, pos, Quaternion.identity, _particleParent);
			p.Play();
			Destroy(p.gameObject, lifeTime);
		}

		private void CreateSFX(SoundIDs id, float pitchShift = 0.0f)
		{
			if (_saveData.SFXMuted || _saveData.SFXVolume == 0)
				return;

			AudioSource s = Instantiate(_sfxPrefab, Vector3.zero, Quaternion.identity, _sfxParent);
			s.clip = _gameData.GetSound(id);
			s.volume = _saveData.SFXVolume;
			if(pitchShift != 0)
			{
				s.pitch = 1 + UnityEngine.Random.Range(-pitchShift, pitchShift);
			}

			s.Play();
			Destroy(s.gameObject, s.clip.length + 0.1f);
		}

		private void SetMusic(SoundIDs id)
		{
			_nextMusicID = id;
			_musicState = 1;
		}

		private void PlayOneShot(OneShotIDs id, int index)
		{
			if (_saveData.SFXMuted)
				return;

			AudioClip clip;
			if(!_gameData.TryGetOneShot(id, index, out clip))
			{
				return;
			}

			AudioSource s = Instantiate(_sfxPrefab, Vector3.zero, Quaternion.identity, _sfxParent);
			s.clip = clip;
			s.Play();
			Destroy(s.gameObject, s.clip.length + 0.1f);
		}

		#endregion
	}
}

