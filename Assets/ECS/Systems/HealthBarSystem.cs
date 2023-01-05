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
        //Consider that HealthBarImage being a managed component don't need to specify the need of write access 
        foreach (var (enemyIndex, health, healthBarImage) in SystemAPI.Query<EnemyId, Health, HealthBarImage>())
        {
            if (healthBarImage.value == null)
            {
                healthBarImage.value = HealthBar.GetEnemyBar(enemyIndex.value);
            }
            
            //healthBarImage.value.fillAmount = health.value / 100f;
        }
    }
}