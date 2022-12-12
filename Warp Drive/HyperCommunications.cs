using RandomizedSystems.SaveGames;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RandomizedSystems.Systems;
using RandomizedSystems.Vessels;
using System.IO;
using RandomizedSystems;
using System;

namespace RandomizedSystems.WarpDrivers
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class HyperCommunications : MonoBehaviour
    {
        public static int TargetSeed = 1;
		public List<ProtoVessel> HyperCommVessels = new List<ProtoVessel>();
		public static void seeddown()
		{
			if (TargetSeed > 0)
			{
				int X = TargetSeed - 1;
				ScreenMessages.PostScreenMessage("Comm Seed: " + X, 3.0f, ScreenMessageStyle.UPPER_CENTER);
				TargetSeed = X;
			}
		}
		public static void seedup()
		{
			int x = TargetSeed + 1;
			ScreenMessages.PostScreenMessage("Comm Seed: " + x, 3.0f, ScreenMessageStyle.UPPER_CENTER);
			TargetSeed = x;
		}
		public static string HyperRelayVesselsCount()
        {
			int Counter = 0;
			string[] TxtLines = GetPersistantTxt();
			int VesselCounter = 0;
			//count how many flightstates/vessels.
			foreach (string line in TxtLines)
			{
				if (line.IndexOf("VESSEL") != -1 && line.IndexOf("M") == -1)
				{
					if (TxtLines[Counter+5].IndexOf("type") != -1 && TxtLines[Counter + 5].IndexOf("SpaceObject") == -1)
					{
						VesselCounter += 1;
					}
				}
				Counter++;
			}
			return (VesselCounter.ToString());

		}
		public static string[] GetPersistantTxt()
        {
			try
			{
				
				string appPath = KSPUtil.ApplicationRootPath;
				string saveFolder = "";
				saveFolder = Path.Combine(appPath,"saves");
				if (string.IsNullOrEmpty(saveFolder))
				{
					return null;
				}
				string directory = Path.Combine(saveFolder, HighLogic.SaveFolder);
					// Look in each save folder for a persistence file
				string systemFolder = Path.Combine(directory, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
				// Look for the Kerbin save
				string kerbinSave = TargetSeed + AstroUtils.SEED_PERSISTENCE + AstroUtils.SFS;
				string stockSaveGame = Path.Combine(systemFolder, kerbinSave);
				if (File.Exists(stockSaveGame))
				{
					string[] txtLines = System.IO.File.ReadAllLines(stockSaveGame);
					return txtLines;
				}
				return null;
			}
			catch (IOException e)
			{
				Debugger.LogException("Unable to recover save games!", e);
				return null;
			}
		}
	}
}
