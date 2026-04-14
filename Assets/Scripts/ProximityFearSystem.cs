using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Proximity Fear System — drives all four assignment requirements:
///   1. Dot Product:   calculates distance to nearest ghost
///   2. Lerp:          vignette darkness scales with proximity
///   3. Particles:     sweat particles trigger when close
///   4. Audio:         heartbeat volume/speed scales with proximity
///
/// Attach this script to the JohnLemon player GameObject.
/// </summary>
public class ProximityFearSystem : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag applied to all ghost/ghoul GameObjects in the scene")]
    [SerializeField] private string ghostTag = "Ghost";
    [Tooltip("Max distance at which fear effects begin")]
    [SerializeField] private float maxFearDistance = 15f;
    [Tooltip("Distance at which fear is at maximum")]
    [SerializeField] private float minFearDistance = 3f;

    [Header("Vignette (Lerp)")]
    [Tooltip("Assign the VignetteOverlay UI Image (full-screen, black edges)")]
    [SerializeField] private Image vignetteImage;
    [Tooltip("Vignette alpha when calm (far from ghosts)")]
    [SerializeField] private float vignetteAlphaMin = 0f;
    [Tooltip("Vignette alpha at maximum fear (very close)")]
    [SerializeField] private float vignetteAlphaMax = 0.85f;

    [Header("Sweat Particles")]
    [Tooltip("Particle system for sweat drops — child of the player")]
    [SerializeField] private ParticleSystem sweatParticles;
    [Tooltip("Distance threshold to start sweating")]
    [SerializeField] private float sweatDistance = 8f;

    [Header("Heartbeat Audio")]
    [Tooltip("AudioSource playing the heartbeat loop")]
    [SerializeField] private AudioSource heartbeatSource;
    [Tooltip("Base pitch when ghost is at max fear distance")]
    [SerializeField] private float heartbeatPitchMin = 0.7f;
    [Tooltip("Pitch when ghost is very close (faster heartbeat)")]
    [SerializeField] private float heartbeatPitchMax = 1.5f;

    // ── Internal state ──
    private GameObject[] ghosts;
    private float currentFear = 0f;          // 0 = calm, 1 = max fear
    private float fearVelocity = 0f;         // for SmoothDamp

    void Start()
    {
        // Cache all ghost references at start
        ghosts = GameObject.FindGameObjectsWithTag(ghostTag);

        // Make sure heartbeat is looping and starts silent
        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.volume = 0f;
            heartbeatSource.Play();
        }

        // Particles off by default
        if (sweatParticles != null)
        {
            sweatParticles.Stop();
        }
    }

    void Update()
    {
        // ──────────────────────────────────────────────────
        // REQUIREMENT 1 — DOT PRODUCT for distance
        // ──────────────────────────────────────────────────
        // Vector3.Dot(v, v) == |v|^2  (squared magnitude).
        // Using dot product avoids an expensive square-root
        // that Vector3.magnitude would require.
        // We also use a second dot product to determine if
        // the nearest ghost is BEHIND the player (scarier!).

        float closestDistSqr = float.MaxValue;
        Vector3 closestDirection = Vector3.zero;

        foreach (GameObject ghost in ghosts)
        {
            if (ghost == null) continue;

            Vector3 toGhost = ghost.transform.position - transform.position;

            // DOT PRODUCT #1: squared distance = dot(v, v)
            float distSqr = Vector3.Dot(toGhost, toGhost);

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestDirection = toGhost;
            }
        }

        // Convert to actual distance for the fear calculation
        float closestDist = Mathf.Sqrt(closestDistSqr);

        // DOT PRODUCT #2: facing check — is ghost behind us?
        // Dot of forward and direction-to-ghost:
        //   > 0 = ghost is in front (less scary, you can see it)
        //   < 0 = ghost is behind  (more scary!)
        float facingDot = 0f;
        if (closestDirection != Vector3.zero)
        {
            facingDot = Vector3.Dot(transform.forward, closestDirection.normalized);
        }

        // Ghost behind you is scarier: extend effective range
        // facingDot < 0 means behind → bonus fear
        float behindBonus = Mathf.Clamp01(-facingDot) * 2f;
        float effectiveDistance = closestDist - behindBonus;

        // Map distance to a 0-1 fear value
        float targetFear = 1f - Mathf.InverseLerp(minFearDistance, maxFearDistance, effectiveDistance);
        targetFear = Mathf.Clamp01(targetFear);

        // Smooth the fear value so it doesn't jitter
        currentFear = Mathf.SmoothDamp(currentFear, targetFear, ref fearVelocity, 0.3f);


        // ──────────────────────────────────────────────────
        // REQUIREMENT 2 — LINEAR INTERPOLATION for vignette
        // ──────────────────────────────────────────────────
        // Lerp the vignette alpha between min and max based
        // on the current fear level (0 = calm, 1 = terrified).

        if (vignetteImage != null)
        {
            float alpha = Mathf.Lerp(vignetteAlphaMin, vignetteAlphaMax, currentFear);
            Color c = vignetteImage.color;
            c.a = alpha;
            vignetteImage.color = c;
        }


        // ──────────────────────────────────────────────────
        // REQUIREMENT 3 — PARTICLE EFFECT (sweat)
        // ──────────────────────────────────────────────────
        // Toggle sweat particles based on distance threshold.

        if (sweatParticles != null)
        {
            if (closestDist < sweatDistance && !sweatParticles.isPlaying)
            {
                sweatParticles.Play();
            }
            else if (closestDist >= sweatDistance && sweatParticles.isPlaying)
            {
                sweatParticles.Stop();
            }
        }


        // ──────────────────────────────────────────────────
        // REQUIREMENT 4 — SOUND EFFECT (heartbeat)
        // ──────────────────────────────────────────────────
        // Lerp the heartbeat volume AND pitch based on fear.
        // Closer ghost = louder + faster heartbeat.

        if (heartbeatSource != null)
        {
            heartbeatSource.volume = Mathf.Lerp(0f, 1f, currentFear);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatPitchMin, heartbeatPitchMax, currentFear);
        }
    }
}
