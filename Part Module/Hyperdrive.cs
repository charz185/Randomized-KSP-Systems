using RandomizedSystems.WarpDrivers;
using System;

namespace RandomizedSystems.Parts
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Hyperdrive : PartModule
    {
        private const string CONTROL_LOCK_ID = "Warp Window Lock";
        private static string lastSeed = string.Empty;

        public static string seedString
        {
            get;
            private set;
        }
        public static string seed2
        {
            get;
            private set;
        }

        bool boolcrystal = true;
        double demandcrystal = 1;
        public override void OnActive()
        {
            ResetKerbolPrompt();
        }
        public static int GetResourceID(Part part, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }

        [KSPEvent(guiActive = true, guiName = "seed down")]
        public void seeddown()
        {
            WarpDrive.seeddown();
        }

        [KSPEvent(guiActive = true, guiName = "seed up")]
        public void seedup1()
        {
            WarpDrive.seedup();
        }

        [KSPEvent(guiActive = true, guiName = "Start Warp Drive")]
        /// <summary>
        /// Starts the hyperspace jump.
        /// </summary>

        public void StartHyperspaceJump()
        {
            CelestialBody reference = FlightGlobals.currentMainBody;
            if (reference.referenceBody.name != reference.name)
            {
                ScreenMessages.PostScreenMessage("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                return;

            }
            Part crystalpart = WarpDrive.GetResourcePart(FlightGlobals.ActiveVessel, "WarpCrystal");
            double crystalamount = part.Resources.Get(GetResourceID(crystalpart, "WarpCrystal")).amount;
            if ( crystalamount>= 1)
            {
                int Crystalid = GetResourceID(crystalpart, "WarpCrystal");
                vessel.RequestResource(part, Crystalid, demandcrystal, boolcrystal);
                WarpDrive.SetNextWarpAction(new WarpDrive.OnWarpDelegate(WarpMessage), new WarpDrive.OnWarpDelegate(ResetKerbolPrompt));
                WarpDrive.JumpToNewSystem(true, WarpDrive.seed2, vessel);
            }
        }

        [KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
        /// <summary>
        /// Jumps to kerbol.
        /// </summary>
        public void JumpToKerbol()
        {
            CelestialBody reference = FlightGlobals.currentMainBody;
            if (reference.referenceBody.name != reference.name)
            {
                ScreenMessages.PostScreenMessage("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            
            WarpDrive.SetNextWarpAction(new WarpDrive.OnWarpDelegate(WarpMessage), new WarpDrive.OnWarpDelegate(ResetKerbolPrompt));
            WarpDrive.JumpToKerbol(true, vessel);
        }

        private void WarpMessage()
        {
            ScreenMessages.PostScreenMessage("Warp Drive initialized. Traveling to the " +
                WarpDrive.currentSystem.name + " system, at coordinates " +
                WarpDrive.seedString + ".",
                                              3.0f,
                                              ScreenMessageStyle.UPPER_CENTER);
        }

        private void ResetKerbolPrompt()
        {
            if (WarpDrive.seedString == "1")
            {
                Events["JumpToKerbol"].active = false;
            }
            else
            {
                Events["JumpToKerbol"].active = true;
            }
        }
    }
}

