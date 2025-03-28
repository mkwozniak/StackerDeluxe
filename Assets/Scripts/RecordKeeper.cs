using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wozware.StackerDeluxe
{
	public static class RecordKeeper
	{
		public static int PERFECTS = 0;
		public static int MISSED = 0;
		public static float TIME = 0.0f;
		public static bool PERFECT_SCORE = false;
		public static bool NEW_RECORD = false;

		public static Dictionary<string, LevelRecord> LVL_RECORDS = new();

		public static void ResetCurrentRecords()
		{
			PERFECTS = 0;
			MISSED = 0;
			TIME = 0.0f;
			PERFECT_SCORE = false;
			NEW_RECORD = false;
		}
	}
}


