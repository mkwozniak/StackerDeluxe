using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Text = TMPro.TextMeshProUGUI;

[RequireComponent(typeof(RectTransform))]
public class ButtonVisualModifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Button RootButton;
	public bool HoverScale = true;
	public Vector3 HoverScaleMultiplier = new Vector3(1, 1, 1);
	public bool HoverEvent = false;
	public UnityEvent OnHoverEvent;
	public bool HasHoverExitEvent = false;
	public UnityEvent OnHoverExit;

	public Text HoverText;

	RectTransform _rectTransform;
	Vector3 _originalScale;

	private void Start()
	{
		_rectTransform = HoverText.GetComponent<RectTransform>();
		_originalScale = _rectTransform.localScale;
	}

	public void Descale()
	{
		if (HoverScale)
		{
			_rectTransform.localScale = _originalScale;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (HoverScale)
		{
			_rectTransform.localScale = HoverScaleMultiplier;
		}

		if (HoverEvent)
		{
			OnHoverEvent.Invoke();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (HoverScale)
		{
			_rectTransform.localScale = _originalScale;
		}

		if (HasHoverExitEvent)
		{
			OnHoverExit.Invoke();
		}
	}

	public void EnableHoverImage()
	{
		HoverText.enabled = true;
	}

	public void DisableHoverImage()
	{
		HoverText.enabled = false;
	}
}
