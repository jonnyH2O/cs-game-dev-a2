# Haunted Jaunt — Proximity Fear System 

I added a unified fear mechanic to the Haunted Jaunt project. As JohnLemon gets closer to any ghost or gargoyle, the tension ramps up through visuals, particles, and audio. It all gets worse if the ghost is behind you.

**To experience it:** walk toward any enemy in the haunted house. The effects fade in gradually and stack together. Walk away to calm back down.

---

## Dot Product
We use the dot product in two ways. First, `Vector3.Dot(toGhost, toGhost)` gives us the squared distance to each ghost (this is faster than calling `.magnitude` since it skips the square root). We loop through all tagged enemies and find whichever is closest.

Second, `Vector3.Dot(transform.forward, directionToGhost)` tells us whether the nearest ghost is in front of or behind the player. A negative dot means it's behind you, which increases the fear effect (because not being able to see what's chasing you is scarier).

## Linear Interpolation
`Mathf.Lerp` adds a vignette overlay that darkens the edges of the screen. As the fear value goes from 0 (calm) to 1 (terrified), the vignette alpha blends from fully transparent to nearly opaque. The heartbeat pitch also uses lerp to smoothly scale between a slow resting rate and a fast panicked rate. Everything is smoothed with `SmoothDamp` so nothing snaps on or off abruptly.

## Particle Effect
A particle system is attached as a child of JohnLemon that emits small blue "sweat" droplets. When a ghost gets within 8 units, the particles start playing. They use world space simulation with gravity, so the drops fall naturally rather than sticking to the player. When you move away from the ghost, the particles stop emitting, and the remaining drops finish falling.

## Sound Effect
A looping heartbeat audio clip plays on a dedicated AudioSource on the player. The script controls both volume and pitch based on proximity. The closer the ghost means louder and faster. The audio is set to 2D spatial blend, so it sounds like an internal heartbeat rather than something coming from a direction in the world.

---

## Team Members
| Name | Contributions |
|------|--------------|
| Jonny Heinrich | Proximity fear system, dot product distance/facing detection, vignette overlay, sweat particles, heartbeat audio, all of it
