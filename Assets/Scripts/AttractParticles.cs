using UnityEngine;

/// <summary>
/// Builds the "attracting" particle effect at runtime.
///
/// The ParticleSystem serialized in Player.prefab (authored in Unity 2017, resaved into the
/// current binary format) no longer renders at all, whatever parameters or material it gets.
/// This component rebuilds the idea - particles appearing around the player and converging
/// inwards - while keeping the particles OUTSIDE the player model so they never cover it.
///
/// The range is chosen so a particle dies just before it reaches the body:
///   travel distance = |convergeSpeed| * lifetime
///   end radius      = spawnRadius - travel distance   (kept above the player's half width)
///
/// NOTE: every curve below is a plain constant on purpose. Two-constant MinMaxCurves
/// (MinMaxCurve(min, max)) end up rendering nothing in a build, so they are avoided.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class AttractParticles : MonoBehaviour
{
    [SerializeField]
    Material particleMaterial;

    /// <summary>
    /// Soft cyan white. Bright enough to read on the dark floor without washing out.
    /// </summary>
    [SerializeField]
    Color tint = new Color(0.72f, 0.90f, 1f, 1f);

    /// <summary>
    /// Height of the ring centre relative to the player. Kept low so the halo surrounds
    /// the body instead of hovering over it.
    /// </summary>
    [SerializeField]
    float heightOffset = 0.35f;

    /// <summary>
    /// Radius the particles spawn at
    /// </summary>
    [SerializeField]
    float spawnRadius = 2.6f;

    /// <summary>
    /// Inward speed. Negative values pull the particles towards the centre.
    /// </summary>
    [SerializeField]
    float convergeSpeed = -2.0f;

    /// <summary>
    /// Lifetime. Short enough that a particle fades out before it reaches the player model:
    /// 2.0 * 0.55 = 1.1 units of travel, ending at radius 1.5.
    /// </summary>
    [SerializeField]
    float lifetime = 0.55f;

    /// <summary>
    /// Particle size in world units
    /// </summary>
    [SerializeField]
    float size = 0.32f;

    /// <summary>
    /// Particles per second
    /// </summary>
    [SerializeField]
    float rate = 80f;

    ParticleSystem system;

    public ParticleSystem System { get { return this.system; } }

    void Awake()
    {
        this.system = Build();

        PlayerController controller = GetComponent<PlayerController>();
        if(controller != null) {
            controller.SetAttractParticles(this.system);
        }
    }

    void LateUpdate()
    {
        if(this.system != null) {
            this.system.transform.position = this.transform.position + (Vector3.up * this.heightOffset);
        }
    }

    void OnDestroy()
    {
        if(this.system != null) {
            Destroy(this.system.gameObject);
        }
    }

    ParticleSystem Build()
    {
        GameObject host = new GameObject("AttractParticlesEffect");
        host.transform.position = this.transform.position + (Vector3.up * this.heightOffset);
        host.transform.rotation = Quaternion.identity;
        host.transform.localScale = Vector3.one;
        host.layer = this.gameObject.layer;

        ParticleSystem ps = host.AddComponent<ParticleSystem>();

        ParticleSystemRenderer renderer = host.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sharedMaterial = this.ResolveMaterial();
        renderer.trailMaterial = renderer.sharedMaterial;

        ParticleSystem.MainModule main = ps.main;
        main.startSize = this.size;
        main.startLifetime = this.lifetime;
        main.startColor = this.tint;
        main.maxParticles = 400;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = this.rate;

        // Spawn on the surface of a sphere and fly inwards. The lifetime is tuned so a
        // particle dies before it reaches the player, leaving a shrinking halo around the
        // model rather than a cloud on top of it.
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = this.spawnRadius;
        shape.radiusThickness = 0f;

        main.startSpeed = this.convergeSpeed;

        return ps;
    }

    Material ResolveMaterial()
    {
        if(this.particleMaterial != null) {
            return this.particleMaterial;
        }

        Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
        if(shader == null) {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.color = this.tint;
        if(material.HasProperty("_TintColor")) {
            material.SetColor("_TintColor", this.tint);
        }
        return material;
    }
}
