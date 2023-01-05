using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Spawner system for the enemies
/// </summary>
[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    private const int GridSize = 10;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<EnemySpawner>();
        var enemies = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemies);

        for (var i = 0; i < enemies.Length; i++)
        {
            var pos = new float3
            {
                x = i / (GridSize * GridSize) * 2,
                y = i / GridSize % GridSize * 2,
                z = i % 10 * 2
            };

            SystemAPI.SetComponent(enemies[i], new EnemyId {value = i});
            SystemAPI.SetComponent(enemies[i], new Health {value = i * 10 % 100});
            SystemAPI.SetComponent(enemies[i], new LocalTransform {Position = pos, Scale = 1});
        }
        
        var spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawner>();
        state.EntityManager.DestroyEntity(spawnerEntity);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}