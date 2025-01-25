using System.Reflection.Emit;
using UnityEngine;
using CanvasText = TMPro.TextMeshProUGUI;
using WorldText = TMPro.TextMeshPro;


namespace wozware.StackerDeluxe
{
	public class CustomTextProperties : MonoBehaviour
	{
		[SerializeField] CanvasText _canvasText;
		[SerializeField] WorldText _worldText;

		[SerializeField] bool _glow;
		[ColorUsage(true, true), SerializeField] Color _glowColor;
		[SerializeField] float _glowInner;
		[SerializeField] float _glowOuter;
		[SerializeField] float _glowPower;
		[SerializeField] float _glowOffset;
		[SerializeField] float _glowPowerSpeed;
		[SerializeField] bool _glowFlicker;
		[SerializeField] float _currGlowFlickerPower = 0.0f;
		[SerializeField] bool _flickerState;
		[SerializeField] bool _oneTimeFlicker = false;
		[SerializeField] float _oneTimeFlickerSpeed = 1.0f;
		[SerializeField] bool _startInvisible;

		bool _validText;
		[SerializeField] Material _fontMaterial = null;

		private void Awake()
		{
			if (_canvasText != null)
			{
				_fontMaterial = _canvasText.fontMaterial;
			}

			if (_worldText != null)
			{
				_fontMaterial = _worldText.fontMaterial;
			}

			_validText = _fontMaterial != null;

			if (_validText)
			{
				ApplyTextProperties();
			}

			if(_startInvisible)
			{
				_canvasText.color = new Color(0, 0, 0, 0);
			}
		}

		private void Update()
		{
			if(_oneTimeFlicker)
			{
				Color c = _canvasText.color;
				c.a -= _oneTimeFlickerSpeed * Time.deltaTime;
				_currGlowFlickerPower -= _oneTimeFlickerSpeed * Time.deltaTime;
				_canvasText.color = c;
				if(c.a <= 0)
				{
					_oneTimeFlicker = false;
				}

				_fontMaterial.SetFloat("_GlowPower", _currGlowFlickerPower);
				return;
			}

			if (!_glowFlicker)
			{
				return;
			}

			if (_flickerState)
			{
				_currGlowFlickerPower += _glowPowerSpeed * Time.deltaTime;
				if (_currGlowFlickerPower >= 1)
				{
					_flickerState = false;
				}
			}
			else
			{
				_currGlowFlickerPower -= _glowPowerSpeed * Time.deltaTime;
				if (_currGlowFlickerPower <= 0)
				{
					_flickerState = true;
				}
			}

			_fontMaterial.SetFloat("_GlowPower", _currGlowFlickerPower);
		}

		public void EnableOneTimeGlowFlicker()
		{
			_currGlowFlickerPower = _glowPower;
			_canvasText.color = _glowColor;
			_oneTimeFlicker = true;
		}

		public void EnableGlowFlicker(bool enable)
		{
			_glowFlicker = enable;
			if(!enable)
			{
				_fontMaterial.SetFloat("_GlowPower", 1);
			}
		}

		public void SetText(string text)
		{
			_canvasText.text = text;
		}

		public void SetColor(Color color, Color glowColor)
		{
			_canvasText.color = color;
			_glowColor = glowColor;
			if(_fontMaterial == null)
				_fontMaterial = _canvasText.fontMaterial;
			_fontMaterial.SetColor("_GlowColor", _glowColor);
		}

		private void ApplyTextProperties()
		{
			if (_glow)
			{
				_fontMaterial.EnableKeyword("GLOW_ON");
				_fontMaterial.SetColor("_GlowColor", _glowColor);
				_fontMaterial.SetFloat("_GlowInner", _glowInner);
				_fontMaterial.SetFloat("_GlowOuter", _glowOuter);
				_fontMaterial.SetFloat("_GlowPower", _glowPower);
				_fontMaterial.SetFloat("_GlowOffset", _glowOffset);
			}
		}
	}
}

