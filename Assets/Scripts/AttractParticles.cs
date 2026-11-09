using UnityEngine;

/// <summary>
/// Builds the "attracting" particle effect at runtime.
///
/// The ParticleSystem serialized in Player.prefab (authored in Unity 2017, resaved into the
/// current binary format) no longer renders at all, whatever parameters or material it gets.
/// This component builds a fresh system at scene root and keeps it on top of the player.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class AttractParticles : MonoBehaviour
{
    [SerializeField]
    Material particleMaterial;

    [SerializeField]
    Color tint = new Color(0.55f, 0.9f, 1f, 1f);

    [SerializeField]
    float heightOffset = 1.2f;

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
        // Deliberately mirrors the configuration that was verified to render in a build:
        // default shape (cone), constant curves, local simulation space.
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
        main.startSize = 0.5f;
        main.startLifetime = 0.9f;
        main.startSpeed = 0.6f;
        main.startColor = this.tint;
        main.maxParticles = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 80f;

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
