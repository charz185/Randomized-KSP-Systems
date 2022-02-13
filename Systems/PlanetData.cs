using RandomizedSystems.Randomizers;
using System.Collections.Generic;

namespace RandomizedSystems.Systems
{
    public class PlanetData
    {
        public SolarData solarSystem
        {
            get
            {
                if (string.IsNullOrEmpty(seed))
                {
                    return null;
                }
                if (SolarData.solarSystems.ContainsKey(seed))
                {
                    return SolarData.solarSystems[seed];
                }
                return null;
            }
        }

        public string seed = string.Empty;
        public CelestialBody planet;
        public int planetID = -1;
        public int moonCount = 0;
        /// <summary>
        /// This keeps track of the Planet IDs of our child bodies, for easy lookup.
        /// </summary>
        public List<int> childDataIDs = new List<int>();
        #region Randomizers
        protected List<PlanetRandomizer> allRandomizers = new List<PlanetRandomizer>();
        // We always want to reference the randomizer in our list
        public AtmosphereRandomizer atmosphereRandomizer
        {
            get
            {
                if (atmoRandomizerIndex == -1)
                {
                    return null;
                }
                return (AtmosphereRandomizer)allRandomizers[atmoRandomizerIndex];
            }
            protected set
            {
                atmoRandomizerIndex = allRandomizers.Count;
                allRandomizers.Add(value);
            }
        }

        private int atmoRandomizerIndex = -1;

        public GeneralRandomizer generalRandomizer
        {
            get
            {
                if (generalRandomizerIndex == -1)
                {
                    return null;
                }
                return (GeneralRandomizer)allRandomizers[generalRandomizerIndex];
            }
            protected set
            {
                generalRandomizerIndex = allRandomizers.Count;
                allRandomizers.Add(value);
            }
        }

        private int generalRandomizerIndex = -1;


        public OrbitRandomizer OrbitRandomizer
        {
            get
            {
                if (orbitRandomizerIndex == -1)
                {
                    return null;
                }
                return (OrbitRandomizer)allRandomizers[orbitRandomizerIndex];
            }
            protected set
            {
                orbitRandomizerIndex = allRandomizers.Count;
                allRandomizers.Add(value);
            }
        }

        private int orbitRandomizerIndex = -1;
        #endregion
        #region Randomizer Accessors
        // These are here for ease of use
        public CelestialBody referenceBody
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return null;
                }
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.referenceBody;
            }
        }

        public PlanetData referenceBodyData
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return null;
                }
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.referenceBodyData;
            }
        }

        public double sphereOfInfluence
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return 0;
                }
                // This will only randomize if we haven't already
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.sphereOfInfluence;
            }
            set
            {
                OrbitRandomizer.sphereOfInfluence = value;
            }
        }

        public double semiMajorAxis
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return 0;
                }
                OrbitRandomizer.Randomize();
                return OrbitRandomizer.orbitData.semiMajorAxis;
            }
        }

        public double gravityMultiplier
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return 0;
                }
                OrbitRandomizer.Randomize();
                return OrbitRandomizer.gravityMultiplier;
            }
        }

        public double eccentricity
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return 0;
                }
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.orbitData.eccentricity;
            }
        }

        public string name
        {
            get
            {
                if (generalRandomizer == null || !string.IsNullOrEmpty(_name))
                {
                    return _name;
                }
                return generalRandomizer.GetName(true);
            }
            set
            {
                if (generalRandomizer != null)
                {
                    generalRandomizer.name = value;
                }
                _name = value;
            }
        }

        private string _name = string.Empty;

        public List<CelestialBody> childBodies
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return null;
                }
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.childBodies;
            }
            set
            {
                OrbitRandomizer.childBodies = value;
            }
        }

        public double gravity
        {
            get
            {
                if (OrbitRandomizer == null)
                {
                    return 0;
                }
                OrbitRandomizer.CreateOrbit();
                return OrbitRandomizer.gravity;
            }
            set
            {
                OrbitRandomizer.gravity = value;
            }
        }
        #endregion
        public PlanetData(CelestialBody planet, string seed, int id)
        {
            this.planetID = id;
            this.seed = seed;
            this.planet = planet;

            // From here we add our randomizers
            OrbitRandomizer = new OrbitRandomizer(planet, this);
            if (IsSun())
            {
                generalRandomizer = new GeneralRandomizer(planet, this);
            }
            atmosphereRandomizer = new AtmosphereRandomizer(planet, this);

            foreach (PlanetRandomizer randomizer in allRandomizers)
            {
                randomizer.Cache();
            }
        }

        public void RandomizeValues()
        {
            foreach (PlanetRandomizer randomizer in allRandomizers)
            {
                randomizer.Randomize();
            }
            SystemNamer.RegisterPlanet(this);
        }

        public void ApplyChanges()
        {
            if (solarSystem.seed != AstroUtils.KERBIN_SYSTEM_COORDS && string.IsNullOrEmpty(name))
            {
                SystemNamer.NameBody(this);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                planet.bodyName = name;
            }
            if (solarSystem.debug)
            {
                string output = "Planet: " + name;
                if (IsSun())
                {
                    output = "Star: " + name;
                }
                output += ", ID: " + planetID;
                Debugger.LogWarning(output);
            }
            foreach (PlanetRandomizer randomizer in allRandomizers)
            {
                randomizer.Apply();
            }
        }

        public bool IsSun()
        {
            return AstroUtils.IsSun(planet);
        }

        public bool IsMoon()
        {
            // If our reference body is *not* the sun, we are a moon
            return referenceBody.name != solarSystem.sunData.name;
        }
    }
}

