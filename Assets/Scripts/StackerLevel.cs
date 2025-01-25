using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* BPM CHART -- FOR EVERY BEAT IN A BAR IT IS (SPEED * 8) (WITH THE ARPEGGIO AT 1/32)
 * IF SPEED 0.25 > 30bpm = 0.25 * 8 = 2s PER BEAT
 * IF SPEED 0.2 > 37.5bpm = 0.2 * 8 = 1.6s PER BEAT
 * IF SPEED 0.15 > 50bpm = 0.15 * 8 = 1.2s PER BEAT
 * IF SPEED 0.1 > 75bpm = 0.1 * 8 = 0.8s PER BEAT
 * IF SPEED 0.075 > 99bpm = 0.075 * 8 = 0.6s PER BEAT
 * IF SPEED 0.05 > 147bpm = 0.05 * 8 = 0.4s PER BEAT
 */

namespace wozware.StackerDeluxe
{
	[CreateAssetMenu(fileName = "StackerLevel", menuName = "StackerDeluxe/StackerLevel", order = 1)]
	public class StackerLevel : ScriptableObject
	{
		public string Name;
		public int StartingThickness;
		public List<StackerLevelRow> RowList;
		public float TimeLeft;

		public Dictionary<int, StackerLevelRow> Rows = new();

		public void Initialize()
		{
			Rows.Clear();
			for (int i = 0; i < RowList.Count; i++)
			{
				Rows.Add(i, RowList[i]);
			}
		}
	}
}

