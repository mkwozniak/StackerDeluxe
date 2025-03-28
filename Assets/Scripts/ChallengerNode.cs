using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengerNode : MonoBehaviour
{
	[SerializeField] List<ChallengerNode> _connectedNodes = new();
	[SerializeField] LineRenderer _linePrefab;
	[SerializeField] Transform _lineParent;
	[SerializeField] Vector3 _lineOffset;

	private void Awake()
	{
		GenerateConnections();
	}

	private void GenerateConnections()
	{
		for(int i = 0; i < _lineParent.childCount; i++)
		{
			Destroy(_lineParent.GetChild(i));
		}

		for(int i = 0; i < _connectedNodes.Count; i++)
		{
			LineRenderer r = Instantiate(_linePrefab, _lineParent);
			r.positionCount = 2;
			r.SetPosition(0, transform.position + _lineOffset);
			r.SetPosition(1, _connectedNodes[i].transform.position + _lineOffset);
		}
	}

	private void OnMouseOver()
	{
		Debug.Log("test");
		if(Input.GetMouseButton(0))
		{
			Debug.Log("Clicked on Node");
		}
	}
}
