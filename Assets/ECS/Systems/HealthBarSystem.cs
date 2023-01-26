using Unity.Burst;
using Unity.Entities;

/// <summary>
/// Updates the canvas health bars based on the ECS health component value. We update before the health dummy system
/// to prevent this query to block the main thread waiting for the dummy workload to finish, generating a one frame
/// delay. An alternative is executing this system as late as possible.
/// </summary>
[BurstCompile, UpdateBefore(typeof(HealthDummySystem))]
public partial struct HealthBarSystem : ISystem
{
    [BurstCompile] public void OnCreate(ref SystemState state) { }
    [BurstCompile] public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        if (EnemyUIManager.Instance == null ||  EnemyUIManager.Instance.UpdateOnMb)
        {
            return;
        }
        
        //Consider that HealthBarImage being a managed component don't need to specify the need of write access 
        foreach (var (health, healthBarImage) in SystemAPI.Query<Health, HealthBarRef>().WithAll<EntityBehaviourIndex>())
        {
            healthBarImage.value.Image.fillAmount = health.value / 100f;
        }
    }
}