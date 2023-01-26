using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

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

        if (!spawner.spawned)
        {
            InitialSpawn(ref spawner, ref state);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var enemyEntity = state.EntityManager.Instantiate(spawner.enemyPrefab);
            var position = new float3 {x = Random.Range(25f, 50f), y = Random.Range(25f, 50f), z = Random.Range(25f, 50f)};
            InitEntity(enemyEntity, position, ref state);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DeleteRandom(ref state);
        }
    }

    private void InitialSpawn(ref EnemySpawner spawner, ref SystemState state)
    {
        var enemyEntities = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemyEntities);
        EntityBehaviourManager.Instance.Positions = new TransformAccessArray(spawner.amount);

        for (var i = 0; i < enemyEntities.Length; i++)
        {
            var enemyEntity = enemyEntities[i];
            var position = new float3 {x = i / (GridSize * GridSize), y = i / GridSize % GridSize, z = i % 10} * 2;
            InitEntity(enemyEntity, position, ref state);
        }

        spawner.spawned = true;
        SystemAPI.SetSingleton(spawner);
    }

    private void InitEntity(Entity enemyEntity, float3 position, ref SystemState state)
    {
        var entityBehaviour = Object.Instantiate(EnemyUIManager.Instance.Prefab);
        entityBehaviour.Entity = enemyEntity;
        entityBehaviour.EntityManager = state.EntityManager;

        state.EntityManager.AddComponentData(enemyEntity, new EntityBehaviourReference {value = entityBehaviour});
        state.EntityManager.AddComponentData(enemyEntity, new EntityBehaviourIndex {value = entityBehaviour.Index});
        state.EntityManager.AddComponentData(enemyEntity, new HealthBarRef {value = entityBehaviour.GetComponentInChildren<HealthBar>()});
        state.EntityManager.SetComponentData(enemyEntity, new LocalTransform {Position = position, Scale = 1});
    }

    private void DeleteRandom(ref SystemState state)
    {
        var indexToDelete = Random.Range(0, EntityBehaviourManager.Instance.All.Count);
        var healthBarToDelete = EntityBehaviourManager.Instance.All[indexToDelete];
        state.EntityManager.DestroyEntity(healthBarToDelete.Entity);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}