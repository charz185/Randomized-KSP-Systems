using UnityEngine;
using RandomizedSystems.WarpDrivers;
using RandomizedSystems.SaveGames;

namespace RandomizedSystems.Parts
{
	public class Hyperdrive : PartModule
	{
		public override void OnActive ()
		{
			ResetKerbolPrompt ();
		}
		[KSPEvent(guiActive = true, guiName = "seed down")]
		public void SeedDown()
		{
			WarpDrive.seeddown(WarpDrive.seed);
		}

		[KSPEvent(guiActive = true, guiName = "seed up")]
		public void SeedUp1()
		{
			WarpDrive.seedup(WarpDrive.seed);
		}
		[KSPEvent(guiActive = true, guiName = "Start Warp Drive")]
		/// <summary>
		/// Starts the hyperspace jump.
		/// </summary>
		public void StartHyperspaceJump ()
		{
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			if (WarpDrive.seedString != WarpDrive.lastSeed)
			{
				WarpDrive.SetNextWarpAction(new WarpDrive.OnWarpDelegate(WarpMessage), new WarpDrive.OnWarpDelegate(ResetKerbolPrompt));
				WarpDrive.Warp(true, WarpDrive.seedString, false);
				PersistenceGenerator.WarpSingleVessel(WarpDrive.lastSeed, WarpDrive.seedString, FlightGlobals.ActiveVessel);
			} else
            {
				ScreenMessages.PostScreenMessage("Cannot warp to the system your in.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			WarpDrive.SetNextWarpAction (new WarpDrive.OnWarpDelegate (WarpMessage), new WarpDrive.OnWarpDelegate (ResetKerbolPrompt));
			WarpDrive.JumpToKerbol (true, vessel);
		}

		private void WarpMessage ()
		{
			ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + 
				WarpDrive.currentSystem.name + " system, at coordinates " + 
				WarpDrive.seedString + ".", 
			                                  3.0f, 
			                                  ScreenMessageStyle.UPPER_CENTER);
		}

		private void ResetKerbolPrompt ()
		{
			if (WarpDrive.seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				Events ["JumpToKerbol"].active = false;
			}
			else
			{
				Events ["JumpToKerbol"].active = true;
			}
		}
	}
}

