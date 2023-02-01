using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;
using Object = UnityEngine.Object;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
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
        var enemyEntities = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemyEntities);
        EntityBehaviourManager.Instance.Transforms = new TransformAccessArray(spawner.amount);

        for (var i = 0; i < enemyEntities.Length; i++)
        {
            var enemyEntity = enemyEntities[i];
            var enemyGo = Object.Instantiate(EnemyUIManager.Instance.Prefab);
            enemyGo.Init(enemyEntity, state.EntityManager);

            var position = new float3 {x = (i / (GridSize * GridSize)), y = i / GridSize % GridSize, z = i % 10} * 2;
            var healthBar = enemyGo.GetComponentInChildren<HealthBar>();
            state.EntityManager.AddComponentData(enemyEntity, new HealthBarRef {value = healthBar});
            state.EntityManager.SetComponentData(enemyEntity, new LocalTransform {Position = position, Scale = 1});

            //Adding random tag components to generate multiple archetypes.
            switch (UnityEngine.Random.Range(0, 100))
            {
                case < 33:
                    state.EntityManager.AddComponent<TestTag1>(enemyEntity);
                    break;
                case < 66:
                    state.EntityManager.AddComponent<TestTag2>(enemyEntity);
                    break;
            }
        }

        var spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawner>();
        state.EntityManager.DestroyEntity(spawnerEntity);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}