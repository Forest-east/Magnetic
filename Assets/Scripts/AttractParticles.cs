using UnityEngine;

/// <summary>
/// Builds the "attracting" particle effect at runtime.
///
/// The ParticleSystem serialized in Player.prefab (authored in Unity 2017, resaved into the
/// current binary format) no longer renders at all, whatever parameters or material it gets.
/// This component rebuilds the same idea - particles spawned on a ring that converge inward
/// towards the player - but brighter and bigger so the effect actually reads on screen.
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
    /// Bright, slightly cyan white so the additive particles glow on the dark floor
    /// </summary>
    [SerializeField]
    Color tint = new Color(0.80f, 0.95f, 1f, 1f);

    [SerializeField]
    float heightOffset = 0.9f;

    /// <summary>
    /// Radius the particles spawn at, before converging on the player
    /// </summary>
    [SerializeField]
    float spawnRadius = 1.9f;

    /// <summary>
    /// Inward speed. Negative values pull the particles towards the centre.
    /// </summary>
    [SerializeField]
    float convergeSpeed = -2.6f;

    /// <summary>
    /// How long a particle lives. Long enough to travel from the ring to the centre.
    /// </summary>
    [SerializeField]
    float lifetime = 0.75f;

    /// <summary>
    /// Particle size in world units (the original effect used 0.2, which was too small to read)
    /// </summary>
    [SerializeField]
    float size = 0.45f;

    /// <summary>
    /// Particles per second (the original used 20, which was too sparse)
    /// </summary>
    [SerializeField]
    float rate = 130f;

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
        main.maxParticles = 700;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = this.rate;

        // Particles spawn on the surface of a sphere and fly inwards, which is the
        // "being pulled towards the player" look the original effect had.
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
