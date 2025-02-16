using UnityEngine;
using System.Collections.Generic;
using RandomizedSystems.Systems;

namespace RandomizedSystems.Randomizers
{
	/// <summary>
	/// The Orbit Randomizer is in charge of randomizing anything which affects other bodies.
	/// This includes gravity and sphere of influence, as well as our own orbit (as us moving around can influence other bodies)
	/// </summary>
	public class OrbitRandomizer : PlanetRandomizer
	{
		public OrbitRandomizer (CelestialBody body, PlanetData bodyData)
		{
			SetBody (body, bodyData);
		}

		public struct OrbitData
		{
			public double inclination;
			public double eccentricity;
			public double semiMajorAxis;
			public double longitudeAscendingNode;
			public double argumentOfPeriapsis;
			public double meanAnomalyAtEpoch;
			public double epoch;
			public double period;
			public CelestialBody referenceBody;
			public bool randomized;
		}

		/// <summary>
		/// The size of our sphere of influence.
		/// </summary>
		public double sphereOfInfluence;
		public double gravity;

		/// <summary>
		/// Expresses gravity in relation to Kerbin's stock gravity.
		/// </summary>
		/// <value>How much more (or less) this planet's gravity is compared to Kerbin.</value>
		public double gravityMultiplier
		{
			get
			{
				return gravity / AstroUtils.KERBIN_GRAVITY;
			}
		}

		public CelestialBody referenceBody;

		public PlanetData referenceBodyData
		{
			get;
			protected set;
		}

		public List<CelestialBody> childBodies = new List<CelestialBody> ();
		public OrbitData orbitData;
		protected Orbit orbit;
		protected OrbitDriver orbitDriver;

		public override void Cache ()
		{
			if (!IsSun ())
			{
				orbit = planet.GetOrbit ();
				orbitData = OrbitDataFromOrbit (orbit);
				orbitDriver = planet.orbitDriver;
				referenceBody = orbit.referenceBody;
				referenceBodyData = solarSystem.GetPlanetByCelestialBody (referenceBody);
			}
			else
			{
				referenceBody = solarSystem.sun;
				referenceBodyData = solarSystem.sunData;
			}
			gravity = planet.gravParameter;
			sphereOfInfluence = planet.sphereOfInfluence;
		}

		public override void Randomize ()
		{
			CreateOrbit ();
		}

		public void CreateOrbit ()
		{
			if (orbitData.randomized || WarpDrivers.WarpDrive.seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				// Already randomized data
				return;
			}
			orbitData = new OrbitData ();
			orbitData.randomized = true;
			if (IsSun ())
			{
				// Special case
				orbitData.referenceBody = solarSystem.sun;
				orbitData.semiMajorAxis = 0;
				return;
			}
			#region Reference Body
			CreateReferenceBody ();
			#endregion
			#region Gravity
			CreateGravity ();
			#endregion
			#region Sphere of Influence
			CreateSphereOfInfluence ();
			#endregion
			#region Semi-Major Axis
			double semiMajorAxis = AstroUtils.MAX_SEMI_MAJOR_AXIS;
			if (referenceBodyData.IsSun ())
			{
				semiMajorAxis = CreatePlanet ();
			}
			else
			{
				// Planet is moon
				semiMajorAxis = CreateMoon ();
			}
			// Remove eccentricity from the semi-major axis
			if (orbitData.eccentricity != 1.0f)
			{
				semiMajorAxis /= (1.0 - orbitData.eccentricity);
			}
			orbitData.semiMajorAxis = semiMajorAxis;
			#endregion
			#region Inclination
			// New way uses normal distribution
			double normalRNG = WarpRNG.GenerateNormalRandom ();
			double normalInc = normalRNG * 5.0;
			orbitData.inclination = normalInc;
			#endregion
			#region Eccentricity
			// Eccentricity must be a value between 0 and 0.99
			// We prefer low values
			normalRNG = WarpRNG.GenerateNormalRandom ();
			// We want to try to clamp the range somewhere between 0 and 0.1, since that produces results most similar to KSP
			double eccentRNG = normalRNG * 0.01666667;
			eccentRNG += 0.05;
			if (eccentRNG < 0)
			{
				eccentRNG *= -1.0;
			}
			double eccentricity = eccentRNG;
			orbitData.eccentricity = eccentricity;
			#endregion
			#region Longitude Ascending Node
			int lan = WarpRNG.GenerateInt (0, 360);
			orbitData.longitudeAscendingNode = lan;
			#endregion
			#region Argument Of Periapsis
			int argumentOfPeriapsis = WarpRNG.GenerateInt (0, 360);
			orbitData.argumentOfPeriapsis = argumentOfPeriapsis;
			#endregion
			#region Mean Anomaly at Epoch
			float meanAnomalyAtEpoch = WarpRNG.GenerateFloat (0.0f, Mathf.PI * 2.0f);
			if (orbitData.semiMajorAxis < 0)
			{
				meanAnomalyAtEpoch /= Mathf.PI;
				meanAnomalyAtEpoch -= 1.0f;
				meanAnomalyAtEpoch *= 5.0f;
			}
			orbitData.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
			#endregion
			#region Period
			double referenceMass = AstroUtils.MassInSolarMasses (referenceBody.Mass);
			double usMass = AstroUtils.MassInSolarMasses (planet.Mass);
			orbitData.period = AstroUtils.CalculatePeriodFromSemiMajorAxis (semiMajorAxis, referenceMass, usMass);
			#endregion
		}

		private void CreateReferenceBody (bool forcePlanet = false)
		{
			referenceBodyData = null;
			referenceBody = null;
			float value = WarpRNG.GetValue ();
			// Planet is in a solar orbit if any of these are true:
			// 1. RNG rolls a value above at or below 0.25 (25% chance)
			// 2. There is only one planet in the solar system (should never happen).
			// 3. We already have a moon orbiting us (no moons orbiting other moons)
			if (forcePlanet || value <= 0.25f || solarSystem.planetCount <= 1 || childBodies.Count > 0)
			{
				referenceBody = solarSystem.sun;
				referenceBodyData = solarSystem.sunData;
			}
			else
			{
				// We will be a moon
				List<int> attemptedInts = new List<int> ();
				int attempts = 0;
				// Toss out a candidate if any of the following is true:
				// 1. The reference body is null or us (causes KSP to crash)
				// 2. The reference body is a moon
				// 3. The reference body is smaller than us
				// Move us to solar orbit after 100 attempts.
				while ((referenceBody == null || referenceBody == planet || referenceBodyData.referenceBody != solarSystem.sun || referenceBody.Radius < planet.Radius))
				{
					attempts++;
					// Keep track of already-attempted planets
					// Might change this to pull a list of all planets from the solar system and poll that
					int index = WarpRNG.GenerateInt (0, solarSystem.planetCount);
					if (attemptedInts.Contains (index))
					{
						continue;
					}
					attemptedInts.Add (index);
					// Get the planet dictated by the random int
					referenceBodyData = solarSystem.GetPlanetByID (index);
					referenceBody = referenceBodyData.planet;
					if (attempts >= 100)
					{
						referenceBody = solarSystem.sun;
						referenceBodyData = solarSystem.sunData;
						break;
					}
					// Loop will do a logic check to make sure the chosen planet is valid
					// Will continue iterating until we have found a valid planet
				}
			}
			// Notify the solar system and the planet itself that our reference body has a new body orbiting it
			solarSystem.AddChildToPlanet (referenceBodyData, planet);
			// Update orbital data
			orbitData.referenceBody = referenceBody;
		}

		private void CreateGravity ()
		{
			float value = WarpRNG.GetValue ();
			float gravityMult = 0.0f;
			if (IsMoon ())
			{
				// Moons in KSP for the most part have SOIs which are greater than their real-life counterparts
				// SOI -> Gravity is not a 1:1 ratio; instead a moon's SOI is usually 7-8 times more powerful than its gravity
				// To skew the gravity data for moons, we use the formula y = (0.0788628 * x^2)-(0.788279 * x)+1.58089
				// Note that values below 7.25 generate negative multipliers
				float randomGravity = WarpRNG.GenerateFloat (7.25f, 9f);
				gravityMult = (0.0788628f * randomGravity * randomGravity) - (0.788279f * randomGravity) + 1.58089f;
			}
			else
			{
				gravityMult = WarpRNG.GenerateFloat (0.15f, 2.0f);
				// There is a chance that a planet is a gas giant like Jool
				if (value <= 0.05f)
				{
					gravityMult *= 20.0f;
				}
			}
			// All gravity values are relative to Kerbin
			gravity = gravityMult * AstroUtils.KERBIN_GRAVITY;
		}

		private void CreateSphereOfInfluence ()
		{
			sphereOfInfluence = AstroUtils.CalculateSOIFromMass (planetData);
			if (sphereOfInfluence > AstroUtils.KERBIN_SOI * 30)
			{
				// This is where Jool's SOI caps out -- we don't want to go any larger
				sphereOfInfluence = AstroUtils.KERBIN_SOI * 30;
			}
		}

		private double CreatePlanet ()
		{
			// Find Semi-Major Axis in KAU (Kerbin Astronomical Units)
			double kerbinSemiMajorAxisMultiplier = WarpRNG.GenerateNormalRandom ();
			// Standard deviation of 2
			kerbinSemiMajorAxisMultiplier *= 2.0;
			// Center it so it's roughly between 0.2 and 4 times Kerbin's orbit
			kerbinSemiMajorAxisMultiplier += 3.2;
			// Now we bias it a little bit (making it technically not a "true" normal distribution, but alas)
			// Really should use Math.Abs
			if (kerbinSemiMajorAxisMultiplier < 0)
			{
				kerbinSemiMajorAxisMultiplier *= -1.0;
			}
			if (kerbinSemiMajorAxisMultiplier < 0.05)
			{
				// Don't want to be too close to the sun
				kerbinSemiMajorAxisMultiplier += 0.05;
			}
			return kerbinSemiMajorAxisMultiplier * AstroUtils.KERBAL_ASTRONOMICAL_UNIT;
		}

		private double CreateMoon ()
		{
			float value = WarpRNG.GetValue ();
			// Floor resulting value at 1%, to be used later
			if (value < 0.0001f)
			{
				value = 0.0001f;
			}
			// Semi-Major Axis can be anywhere within the hill sphere of parent body
			double hillSphere = AstroUtils.CalculateHillSphere (referenceBodyData);
			double tempMajorAxis = hillSphere * value * 0.5;
			double parentAtmosphereHeight = planet.Radius + (sphereOfInfluence * 0.5) + referenceBody.Radius + (referenceBody.atmDensityASL * 1000.0 * Mathf.Log (1000000.0f));
			while (tempMajorAxis < parentAtmosphereHeight)
			{
				// Inside planet's atmosphere
				value += WarpRNG.GenerateFloat (0.001f, 0.1f);
				tempMajorAxis = hillSphere * value;
			}
			foreach (int id in referenceBodyData.childDataIDs)
			{
				// This ensures we do not crash into other planets
				PlanetData childData = solarSystem.GetPlanetByID (id);
				double moonAxis = childData.semiMajorAxis;
				double moonMin = moonAxis - childData.planet.Radius - (childData.sphereOfInfluence * 0.5);
				double moonMax = moonAxis + childData.planet.Radius + (childData.sphereOfInfluence * 0.5);
				while (tempMajorAxis + planet.Radius >= moonMin && tempMajorAxis <= moonMax)
				{
					value += WarpRNG.GenerateFloat (0.001f, 0.1f);
					tempMajorAxis = hillSphere * value;
				}
			}
			if (tempMajorAxis > referenceBodyData.sphereOfInfluence)
			{
				Debugger.LogWarning ("Rejecting " + planetData.planetID + " as a candidate due to bad SOI matchup. " +
					"Previous body: " + referenceBodyData.planetID);
				orbitData = new OrbitData ();
				orbitData.randomized = true;
				referenceBodyData = null;
				referenceBody = null;
				// Make us a planet instead
				CreateReferenceBody (true);
				CreateGravity ();
				CreateSphereOfInfluence ();
				Debugger.LogWarning ("New body: " + referenceBodyData.planetID);
				return CreatePlanet ();
			}
			return tempMajorAxis;
		}

		public override void Apply ()
		{
			if (!IsSun ())
			{
				if (solarSystem.debug)
				{
					Debugger.Log ("Sphere of influence: " + sphereOfInfluence + " meters (" + (sphereOfInfluence / AstroUtils.KERBIN_SOI) + " times Kerbin SOI)");
				}
				referenceBody.orbitingBodies.Add (planet);
				planet.sphereOfInfluence = sphereOfInfluence;
			}
			if (solarSystem.debug)
			{
				Debugger.Log ("Gravity: " + (gravity / AstroUtils.KERBIN_GRAVITY) + " times Kerbin gravity.");
			}
			planet.gravParameter = gravity;
			if (orbitDriver != null)
			{
				orbit = CreateOrbit (orbitData, orbit);
				orbitDriver.orbit = orbit;
				orbitDriver.UpdateOrbit ();
			}
		}

		private static OrbitData OrbitDataFromOrbit (Orbit orbit)
		{
			OrbitData data = new OrbitData ();
			data.inclination = orbit.inclination;
			data.eccentricity = orbit.eccentricity;
			data.semiMajorAxis = orbit.semiMajorAxis;
			data.longitudeAscendingNode = orbit.LAN;
			data.argumentOfPeriapsis = orbit.argumentOfPeriapsis;
			data.meanAnomalyAtEpoch = orbit.meanAnomalyAtEpoch;
			data.epoch = orbit.epoch;
			data.period = orbit.period;
			data.referenceBody = orbit.referenceBody;
			return data;
		}

		private Orbit CreateOrbit (OrbitData data, Orbit orbit)
		{
			return CreateOrbit (data.inclination,
			                    data.eccentricity, 
			                    data.semiMajorAxis, 
			                    data.longitudeAscendingNode, 
			                    data.argumentOfPeriapsis, 
			                    data.meanAnomalyAtEpoch, 
			                    data.epoch,
			                    orbit,
			                    data.referenceBody);
		}

		private Orbit CreateOrbit (double inclination,
		                           double eccentricity,
		                           double semiMajorAxis, 
		                           double longitudeAscendingNode, 
		                           double argumentOfPeriapsis, 
		                           double meanAnomalyAtEpoch, 
		                           double epoch, 
		                           Orbit orbit,
		                           CelestialBody referenceBody)
		{
			if (double.IsNaN (inclination))
			{
				inclination = 0;
				Debugger.LogWarning ("Inclination not a number!");
			}
			if (double.IsNaN (eccentricity))
			{
				eccentricity = 0;
				Debugger.LogWarning ("Eccentricity not a number!");
			}
			if (double.IsNaN (semiMajorAxis))
			{
				semiMajorAxis = referenceBody.Radius + referenceBody.atmDensityASL + 10000;
				Debugger.LogWarning ("Semi-Major Axis not a number!");
			}
			if (double.IsNaN (longitudeAscendingNode))
			{
				longitudeAscendingNode = 0;
				Debugger.LogWarning ("Longitude Ascending Node not a number!");
			}
			if (double.IsNaN (argumentOfPeriapsis))
			{
				argumentOfPeriapsis = 0;
				Debugger.LogWarning ("Argument of Periapsis not a number!");
			}
			if (double.IsNaN (meanAnomalyAtEpoch))
			{
				meanAnomalyAtEpoch = 0;
				Debugger.LogWarning ("Mean Anomaly at Epoch not a number!");
			}
			if (double.IsNaN (epoch))
			{
				epoch = Planetarium.GetUniversalTime ();
				Debugger.LogWarning ("Epoch not a number!");
			}
			if (Mathf.Sign ((float)eccentricity - 1.0f) == Mathf.Sign ((float)semiMajorAxis))
			{
				semiMajorAxis = -semiMajorAxis;
			}
			if (Mathf.Sign ((float)semiMajorAxis) >= 0)
			{
				while (meanAnomalyAtEpoch < 0)
				{
					meanAnomalyAtEpoch += Mathf.PI * 2;
				}
				while (meanAnomalyAtEpoch > Mathf.PI * 2)
				{
					meanAnomalyAtEpoch -= Mathf.PI * 2;
				}
			}
			if (referenceBody == null)
			{
				Debugger.LogError ("Reference body is null!");
				// Cannot proceed with setting orbit
				return orbit;
			}
			if (solarSystem.debug)
			{
				Debugger.Log ("Reference Body: " + referenceBody);
				Debugger.Log ("Inclination: " + inclination);
				Debugger.Log ("Eccentricity: " + eccentricity);
				Debugger.Log ("Semi-Major Axis: " + semiMajorAxis + " (" + (semiMajorAxis / AstroUtils.KERBAL_ASTRONOMICAL_UNIT) + " Kerbin Astronomical Units)");
				Debugger.Log ("Longitude of Ascending Node: " + longitudeAscendingNode);
				Debugger.Log ("Argument of Periapsis: " + argumentOfPeriapsis);
				Debugger.Log ("Mean Anomaly at Epoch: " + meanAnomalyAtEpoch);
				Debugger.Log ("Epoch: " + epoch);
			}
			Orbit newOrbit = new Orbit (inclination, eccentricity, semiMajorAxis, longitudeAscendingNode, argumentOfPeriapsis, meanAnomalyAtEpoch, epoch, referenceBody);
			return newOrbit;
		}
	}
}

