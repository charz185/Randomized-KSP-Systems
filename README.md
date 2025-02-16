(new fork, made by charz185)

# Kerbal Space Program Solar System Randomizer

Allows for a ship to use a warp drive to jump to hyperspace, arriving in a completely different system, generated by a custom seed.

Thanks to /u/SuperSeniorComicGuy over at Reddit's /r/KerbalSpaceProgram subreddit [for the idea](http://www.reddit.com/r/KerbalSpaceProgram/comments/2jcwb1/beta_than_ever_the_future_of_ksp/clakjdc)!


Here's a list of what changes:
(soon to be re added)
### Atmospheres:  
* Atmosphere (does a body have one?)  
* Oxygen (does a body have it?)  
* Atmosphere Height  
* Atmosphere Color  
* Atmosphere Pressure

### General Planet Stuff:  
* Gravity  
* Temperature  
* Names  

### Orbits:  
* Reference Bodies (former moons may now orbit the sun, and former planets may orbit other planets)  
* Semi-Major Axis  
* Eccentricity  
* Inclination  
* Longitude Ascending Node  
* Rotation (how long a day is)  
* Sphere of Influence (unrelated to the size of the body -- Ike can have an SOI the size of Jool, for example)  

What happens is an entirely new solar system is produced. The planets themselves are the same as the old ones, but everything about them is different. You can then leave the system (by warping your ship home or simply pausing and hit "Return to Space Center") and all ships left in orbit will remain in orbit just as you left them.

To activate the Warp Drive, just go into a solar orbit around Kerbol. Right-click on the Warp Drive part (found in the VAB under "Utility" -- it looks like an ASAS module), then click on "Activate Warp Drive." A box will come up asking for hyperspace coordinates. Type in your seed, then click the button and the system will randomize based on your seed.
## changelog 1.3.0
> added all missing features from the original
> removed warp crystal
> removed warp crystallizer part
> no more immediate errors!


## changelog 1.2.8
> added new resource, warp crystal
> added new part, warp crystallizer
> changed Kerbal system cords from 0 to 1
> made it so that the warp drive uses warp crystal as fuel to warp.

## changelog 1.2.0
> updated mod to newest version of C#, so that itworks on newer versions of KSP
> randomizers don't work
> error shows up on warp, but warps anyways.
> craft persistance is still in
> buttons to choose the seed, rather than text.
> to go back to kerbol, change seed to just 1 "1".


## Changelog
#### 1.1.0
> **Features**     
> *Added craft persistence between solar systems.*  
> Updated cost and science requirement for Hyperdrive part.  
> Automatically return to Kerbol when "Return to Space Center" is clicked, leaving your craft in orbit around the other system.  
> **Tweaks**  
> Changed some randomization features to use a normal distribution.  
> Improved performance of RNG (thanks, paul23!)  

#### 1.0.0  
> Initial release.

## Future Additions: 
* Name your own planets  
* Custom model for Warp Drive  
* Procedurally-generated terrain on planets  
* View a "starmap" that allows you to warp to nearby stars without needing to remember a seed.  
* Custom contracts for different systems
