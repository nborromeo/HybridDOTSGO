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
        //Batch spawning of all the needed entitites
        var spawner = SystemAPI.GetSingleton<EnemySpawner>();
        var enemyEntities = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemyEntities);
        
        //Initialization of the transform access array for position syncing in EntityBehaviourTransformUpdateSystem
        EntityBehaviourManager.Instance.Transforms = new TransformAccessArray(spawner.amount);

        //Usually you can initialize batch spawned entities in jobs, but we can't here due to the GOs instantiation
        for (var i = 0; i < enemyEntities.Length; i++)
        {
            //Initialize the Entity position
            var enemyEntity = enemyEntities[i];
            var position = new float3 {x = i / (GridSize * GridSize), y = i / GridSize % GridSize, z = i % 10} * 2;
            state.EntityManager.SetComponentData(enemyEntity, new LocalTransform {Position = position, Scale = 1});
            
            //Spawn the GO counterpart and link it with the entity
            var enemyGo = Object.Instantiate(EnemyUIManager.Instance.Prefab);
            var syncTransforms = UnityEngine.Random.Range(0, 100) < 50;
            enemyGo.Init(enemyEntity, state.EntityManager, syncTransforms);

            //Link the entity to its GO counterpart
            var healthBar = enemyGo.GetComponentInChildren<HealthBar>();
            state.EntityManager.AddComponentData(enemyEntity, new HealthBarRef {value = healthBar});

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

        //Destroy the spawner
        var spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawner>();
        state.EntityManager.DestroyEntity(spawnerEntity);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}