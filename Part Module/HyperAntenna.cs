using UnityEngine;
using RandomizedSystems.WarpDrivers;
using RandomizedSystems.SaveGames;
using System;

namespace RandomizedSystems.Parts
{
	public class HyperAntenna : PartModule
	{
		[KSPEvent(guiActive = true, guiName = "Start Communications Relay")]
		public void StartCommunicationsRelay()
        {
            string VesselCount = HyperCommunications.HyperRelayVesselsCount();
			ScreenMessages.PostScreenMessage("In seed "+ HyperCommunications.TargetSeed+ " there are "+VesselCount+ " vessels.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
		}
		[KSPEvent(guiActive = true, guiName = "Increase Target Seed")]
		public void CommsTargetSeedInc()
        {
            HyperCommunications.seedup();

		}
		[KSPEvent(guiActive = true, guiName = "Decrease target Seed")]
		public void CommsTargetSeedDec()
		{
            HyperCommunications.seeddown();
		}
	}
}

