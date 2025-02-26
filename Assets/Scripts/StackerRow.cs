using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wozware.StackerDeluxe
{
	public class StackerRow : MonoBehaviour
	{
		public static int STAGE_WIDTH = 3;
		public static float LAST_STOPPED_X = 0;

		[Header("Settings")]
		[SerializeField] MeshRenderer _stackerCubePrefab;
		[SerializeField] ParticleSystem _stackerCubeExplosion;
		[SerializeField] float _speed;
		[SerializeField] int _thickness;
		[SerializeField] LayerMask _cubeLayer;
		[SerializeField] int _currHeight = 0;
		[SerializeField] Material _loseCubeMaterial;
		[SerializeField] Material _defaultCubeMaterial;

		[Header("States")]
		[SerializeField] int _currDirection = 1;
		[SerializeField] float _speedTimer = 0f;
		[SerializeField] int _dividedWidth = 0;
		[SerializeField] int _endingWidth = 0;
		[SerializeField] int _currStageLimitLeft = 0;
		[SerializeField] int _currStageLimitRight = 0;
		[SerializeField] bool _active = false;
		[SerializeField] bool _didMiss = false;
		[SerializeField] int _moveIndex = 0;

		private List<MeshRenderer> _cubes = new();
		private List<Transform> _missedCubes = new();

		public bool DidMiss
		{
			get { return _didMiss; }
		}

		public bool Active
		{
			set { _active = value; }
		}

		private void Awake()
		{
			_active = false;
		}

		private void Update()
		{
			if (!_active)
				return;

			UpdateMovement();
			UpdateVisibility();
		}

		public void Initialize(int thickness, float speed, int height)
		{
			_thickness = thickness;
			_speed = speed;
			_currHeight = height;

			// initialize thickness of stacker row
			int d = _thickness / 2;
			_dividedWidth = -(_thickness / 2);
			_endingWidth = _thickness - (d + 1);
			_currStageLimitLeft = -(STAGE_WIDTH + _dividedWidth);
			_currStageLimitRight = (STAGE_WIDTH - _endingWidth);

			// set random starting position of stacker row
			int ranX = Random.Range(_currStageLimitLeft - 1, _currStageLimitRight + 1);
			while (ranX == LAST_STOPPED_X)
			{
				ranX = Random.Range(_currStageLimitLeft - 1, _currStageLimitRight + 1);
			}

			transform.position = new Vector3(ranX, transform.position.y, transform.position.z);
			_moveIndex = ranX + 3;

			Game.OnGameOver += SetCubesToLoseMaterial;
			SpawnStackerCubes();
			_active = true;
			_didMiss = false;
			Game.Log(LogTypes.STACKER_ROW, $"Pushing New Stacker Row To Height: {height}");
		}

		public void Stop()
		{
			_active = false;
			LAST_STOPPED_X = transform.position.x;

			bool didLoseCube = false;
			bool allCubesMissed = true;
			_missedCubes.Clear();

			for (int i = 0; i < _cubes.Count; i++)
			{
				MeshRenderer c = _cubes[i];
				c.material = _defaultCubeMaterial;
				// if its the first stacker, dont check any misses
				if (_currHeight == 0)
				{
					continue;
				}

				// check for any misses below this row
				bool hit = Physics.Raycast(c.transform.position, Vector3.down, 1, _cubeLayer);
				if (!hit)
				{
					_missedCubes.Add(c.transform);
					didLoseCube = true;
					continue;
				}

				allCubesMissed = false;
			}

			if(allCubesMissed && didLoseCube)
			{
				Game.DoTriggerGameOver(false);
				return;
			}

			if(didLoseCube)
			{
				DestroyMissedCubes();
				Game.DoCreateSFX(SoundIDs.StackerExplode, 0.1f);
				Game.DoCreateSFX(SoundIDs.MissStacker, 0);
				_didMiss = true;
				Game.DoContinueStackerSpawn();
				return;
			}

			if(Game.DoCheckGameWin())
			{
				return;
			}

			Game.DoCreateSFX(SoundIDs.PlaceStacker, 0);
			Game.DoContinueStackerSpawn();
		}

		public void DestroySelf()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Vector3 pos = transform.GetChild(i).position;
				pos.z -= 1f;
				Game.DoCreateParticleFX(_stackerCubeExplosion, transform.GetChild(i).position, 3f);
			}

			Game.DoCreateSFX(SoundIDs.StackerExplode, 0.1f);
			Destroy(gameObject);
		}

		private void SpawnStackerCubes()
		{
			for (int i = _dividedWidth; i <= _endingWidth; i++)
			{
				MeshRenderer newCube = Instantiate(_stackerCubePrefab);
				_cubes.Add(newCube);
				newCube.transform.parent = transform;
				newCube.transform.localPosition = new Vector3(i, 0, 0);
			}
		}

		private void DestroyMissedCubes()
		{
			for (int i = 0; i < _missedCubes.Count; i++)
			{
				if (_missedCubes[i] == null)
					continue;

				Game.DoCreateParticleFX(_stackerCubeExplosion, _missedCubes[i].position, 3f);
				Destroy(_missedCubes[i].gameObject);
				Game.DoReduceThickness();
			}
		}

		private void SetCubesToLoseMaterial()
		{
			Game.OnGameOver -= SetCubesToLoseMaterial;
			for (int i = 0; i < _cubes.Count; i++)
			{
				if (_cubes[i] == null)
					continue;
				_cubes[i].material = _loseCubeMaterial;
			}
		}

		private void UpdateMovement()
		{
			_speedTimer += Time.deltaTime;

			if (_speedTimer >= _speed)
			{
				if (transform.position.x >= _currStageLimitRight && _currDirection == 1)
				{
					_currDirection = -1;
				}

				if (transform.position.x <= _currStageLimitLeft && _currDirection == -1)
				{
					_currDirection = 1;
				}

				_speedTimer = 0f;
				transform.position += new Vector3(_currDirection, 0, 0);
				_moveIndex += 1 * _currDirection;

				if (Game.STACKER_CRAWL_SOUND_MODE == StackerSoundCrawlModes.OneShot)
				{
					Game.DoPlayOneShot(OneShotIDs.SynthOne, _moveIndex);
				}
			}
		}

		private void UpdateVisibility()
		{
			for (int i = 0; i < _cubes.Count; i++)
			{
				if (_cubes[i] == null)
					continue;

				if (_cubes[i].transform.position.x > _currStageLimitRight + 1
					|| _cubes[i].transform.position.x < _currStageLimitLeft - 1)
				{
					_cubes[i].enabled = false;
					continue;
				}

				if (!_cubes[i].enabled)
					_cubes[i].enabled = true;
			}
		}
	}
}
