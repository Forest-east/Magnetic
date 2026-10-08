using UnityEngine;

/// <summary>
/// 出产砖：行为与普通砖一致，额外记录自己"出产什么"，
/// 并据此把模型贴图换成对应的样式（箱子出产点 / 金属球出产点）。
///
/// 说明：本脚本在 2017 原版基础上做了空引用加固 —— 原版直接访问
/// renderer / particles，一旦预制体上没挂对应引用就会在第一次 Update 抛异常。
/// 现在的版本在任何引用缺失时都只是跳过，不会崩。
/// </summary>
public class SpawnerTile : NormalTile
{
    /// <summary>
    /// 出产类型，决定使用哪张贴图
    /// </summary>
    public enum SpawnerType
    {
        CrateSpawner,
        BallSpawner,
    }

    /// <summary>
    /// 本出产砖的类型
    /// </summary>
    [SerializeField]
    SpawnerType spawnerType = SpawnerType.CrateSpawner;

    /// <summary>
    /// 模型上的 MeshRenderer
    /// </summary>
    [SerializeField]
    MeshRenderer modelRenderer;

    /// <summary>
    /// 出产箱子时使用的贴图
    /// </summary>
    [SerializeField]
    Material crateSpawnerMaterial;

    /// <summary>
    /// 出产金属球时使用的贴图
    /// </summary>
    [SerializeField]
    Material ballSpawnerMaterial;

    /// <summary>
    /// 贴图是否已经设置过，保证只设置一次
    /// </summary>
    bool materialSet = false;

    /// <summary>
    /// 出产时播放的粒子效果（可选）
    /// </summary>
    [SerializeField]
    ParticleSystem particles;

    /// <summary>
    /// 父类 Tile / NormalTile 已经占用了 Awake 和 Start，
    /// 而且 OnTriggerStay 无法覆盖，所以这里用 Update 完成一次性初始化。
    /// </summary>
    void Update()
    {
        if (this.materialSet)
        {
            return;
        }
        this.materialSet = true;

        if (this.modelRenderer == null)
        {
            return;
        }

        Material target = null;
        switch (this.spawnerType)
        {
            case SpawnerType.CrateSpawner:
                target = this.crateSpawnerMaterial;
                break;
            case SpawnerType.BallSpawner:
                target = this.ballSpawnerMaterial;
                break;
        }

        if (target != null)
        {
            this.modelRenderer.sharedMaterial = target;
        }
    }

    /// <summary>
    /// 播放出产粒子效果（没挂粒子系统时安全跳过）
    /// </summary>
    public void PlayParticles()
    {
        if (this.particles != null)
        {
            this.particles.Play();
        }
    }
}
