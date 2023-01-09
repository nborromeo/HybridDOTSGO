using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

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

    public void OnUpdate(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<EnemySpawner>();
        var spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawner>();
        var enemyUIs = new Transform[spawner.amount];
        var enemyEntities = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemyEntities);

        for (var i = 0; i < enemyEntities.Length; i++)
        {
            var enemyEntity = enemyEntities[i];
            
            var ui = Object.Instantiate(EnemyUIManager.Instance.Prefab);
            var healthBar = ui.GetComponentInChildren<HealthBar>();
            healthBar.EnemyEntity = enemyEntity;
            enemyUIs[i] = ui.transform;
            
            var position = new float3 {x = i / (GridSize * GridSize), y = i / GridSize % GridSize, z = i % 10} * 2;
            SystemAPI.SetComponent(enemyEntity, new EnemyId {value = i});
            SystemAPI.SetComponent(enemyEntity, new Health {value = i * 10 % 100});
            SystemAPI.SetComponent(enemyEntity, new LocalTransform {Position = position, Scale = 1});
            state.EntityManager.SetComponentData(enemyEntity, new HealthBarRef {value = healthBar});
        }
        
        EnemyUIManager.Instance.Positions = new TransformAccessArray(enemyUIs);
        state.EntityManager.DestroyEntity(spawnerEntity);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}