using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace wozware.StackerDeluxe
{
	[System.Serializable]
	public struct StackerLevelRow
	{
		public int Thickness;
		public float Speed;
		public float SecondsGain;
		public AudioClip MusicLoop;
	}

	[System.Serializable]
	public struct SoundData
	{
		public SoundIDs ID;
		public List<AudioClip> Clips;
	}

	[System.Serializable]
	public struct OneShotData
	{
		public OneShotIDs ID;
		public List<AudioClip> Clips;
	}

	[System.Serializable]
	public struct LevelRecord
	{
		public string Name;
		public float TimeLeft;
		public int Perfects;
		public int Misses;

		public LevelRecord(string name, float timeLeft, int perfects, int misses)
		{
			Name = name;
			TimeLeft = timeLeft;	
			Perfects = perfects;
			Misses = misses;
		}
	}

	[System.Serializable]
	public sealed class SaveData
	{
		public float MusicVolume;
		public float SFXVolume;
		public bool MusicMuted;
		public bool SFXMuted;
		public int ResolutionWidth;
		public int ResolutionHeight;
		public uint RefreshRate;
		public uint RefreshRateDenom;
		public FullScreenMode ScreenMode;
		public float BloomIntensity;
		public List<LevelRecord> LevelRecords;
	}

	public enum LogTypes
	{
		GAME,
		UI,
		FILE,
		STACKER_ROW,
	}

	public enum GameStates
	{
		MainMenu,
		GameActive,
		GameOver,
		GameWin,
		GamePaused,
		Challenger,
	}

	public enum LevelDifficulties
	{
		Normal,
		Hard,
		Expert,
		Debug,
	}

	public enum SoundIDs
	{
		Empty,
		PlaceStacker,
		MissStacker,
		StackerExplode,
		Countdown,
		CountdownFinish,
		MusicMainMenu,
		MusicGameOver,
		MusicWin,
		ButtonHover,
		ButtonClick,
		TimerTick,
		VFX_Timeout,
		VFX_GameOver,
		VFX_GameWin,
		VFX_NewRecord,
		MusicChallenger,
	}

	public enum OneShotIDs
	{
		SynthOne,
	}

	public enum StackerSoundCrawlModes
	{
		Arpeggio,
		OneShot,
		Off,
	}

	[CreateAssetMenu(fileName = "StackerData", menuName = "StackerDeluxe/StackerData", order = 2)]
	public class StackerData : ScriptableObject
	{
		[SerializeField] StackerLevel _levelNormal;
		[SerializeField] StackerLevel _levelHard;
		[SerializeField] StackerLevel _levelExpert;
		[SerializeField] StackerLevel _levelDebug;
		[SerializeField] List<StackerLevel> _levelsChallenger;

		[SerializeField] List<SoundData> _soundsList;
		[SerializeField] List<OneShotData> _oneShotList;

		Dictionary<LevelDifficulties, StackerLevel> _standardLevels = new();
		Dictionary<int, StackerLevel> _challengerLevels = new();
		Dictionary<SoundIDs, List<AudioClip>> _sounds = new();
		Dictionary<OneShotIDs, List<AudioClip>> _oneShots = new();

		public void Initialize()
		{
			_standardLevels[LevelDifficulties.Normal] = _levelNormal;
			_standardLevels[LevelDifficulties.Hard] = _levelHard;
			_standardLevels[LevelDifficulties.Expert] = _levelExpert;
			_standardLevels[LevelDifficulties.Debug] = _levelDebug;

			RecordKeeper.LVL_RECORDS.Add(_levelNormal.Name, new LevelRecord(_levelNormal.Name, 0, 0, 0));
			RecordKeeper.LVL_RECORDS.Add(_levelHard.Name, new LevelRecord(_levelHard.Name, 0, 0, 0));
			RecordKeeper.LVL_RECORDS.Add(_levelExpert.Name, new LevelRecord(_levelExpert.Name, 0, 0, 0));
			RecordKeeper.LVL_RECORDS.Add(_levelDebug.Name, new LevelRecord(_levelDebug.Name, 0, 0, 0));

			for (int i = 0; i < _levelsChallenger.Count; i++)
			{
				_challengerLevels[i] = _levelsChallenger[i];
			}

			for(int i = 0; i < _soundsList.Count; i++)
			{
				_sounds[_soundsList[i].ID] = _soundsList[i].Clips;
			}

			for (int i = 0; i < _oneShotList.Count; i++)
			{
				_oneShots[_oneShotList[i].ID] = _oneShotList[i].Clips;
			}
		}

		public bool TryGetStandardStackerLevel(LevelDifficulties difficulty, ref StackerLevel lvl)
		{
			if(!_standardLevels.ContainsKey(difficulty))
			{
				Game.Log(LogTypes.GAME, $"StackerLevel difficulty {difficulty} does not exist.", 2);
				lvl = null;
				return false;
			}

			Game.Log(LogTypes.GAME, $"Selected Difficulty: {difficulty}");
			lvl = _standardLevels[difficulty];
			lvl.Initialize();
			return true;
		}

		public AudioClip GetSound(SoundIDs id)
		{
			return _sounds[id][Random.Range(0, _sounds[id].Count - 1)];
		}

		public bool TryGetOneShot(OneShotIDs id, int index, out AudioClip clip)
		{
			if(!_oneShots.ContainsKey(id) || index < 0)
			{
				clip = null;
				return false;
			}

			if (index >= _oneShots[id].Count)
			{
				clip = null;
				return false;
			}

			clip = _oneShots[id][index];
			return true;
		}
	}
}

