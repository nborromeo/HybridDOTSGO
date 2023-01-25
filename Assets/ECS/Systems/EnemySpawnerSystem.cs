using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

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

        if (!spawner.spawned)
        {
            InitialSpawn(ref spawner, ref state);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            InstanceNew(ref spawner, ref state);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DeleteRandom(ref state);
        }
    }

    private void InitialSpawn(ref EnemySpawner spawner, ref SystemState state)
    {
        var enemyUIs = new Transform[spawner.amount];
        var enemyEntities = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        state.EntityManager.Instantiate(spawner.enemyPrefab, enemyEntities);

        for (var i = 0; i < enemyEntities.Length; i++)
        {
            var enemyEntity = enemyEntities[i];

            var ui = Object.Instantiate(EnemyUIManager.Instance.Prefab);
            var healthBar = ui.GetComponentInChildren<HealthBar>();
            healthBar.EnemyEntity = enemyEntity;
            //healthBar.EntityManager = state.EntityManager; //This is needed in case of multiple worlds
            enemyUIs[i] = ui.transform;
            EnemyUIManager.Instance.HealthBars.Add(healthBar);

            var position = new float3 {x = i / (GridSize * GridSize), y = i / GridSize % GridSize, z = i % 10} * 2;
            SystemAPI.SetComponent(enemyEntity, new EnemyId {value = i});
            SystemAPI.SetComponent(enemyEntity, new Health {value = i * 10 % 100});
            SystemAPI.SetComponent(enemyEntity, new LocalTransform {Position = position, Scale = 1});
            state.EntityManager.SetComponentData(enemyEntity, new HealthBarRef {value = healthBar});
        }

        EnemyUIManager.Instance.Positions = new TransformAccessArray(enemyUIs);

        spawner.spawned = true;
        SystemAPI.SetSingleton(spawner);
    }

    private void InstanceNew(ref EnemySpawner spawner, ref SystemState state)
    {
        var enemyEntity = state.EntityManager.Instantiate(spawner.enemyPrefab);
        var ui = Object.Instantiate(EnemyUIManager.Instance.Prefab);
        
        var healthBar = ui.GetComponentInChildren<HealthBar>();
        healthBar.EnemyEntity = enemyEntity;

        var position = new float3 {x = Random.Range(25f, 50f), y = Random.Range(25f, 50f), z = Random.Range(25f, 50f)};
        SystemAPI.SetComponent(enemyEntity, new EnemyId {value = EnemyUIManager.Instance.Positions.length});
        SystemAPI.SetComponent(enemyEntity, new Health {value = 50});
        SystemAPI.SetComponent(enemyEntity, new LocalTransform {Position = position, Scale = 1});
        state.EntityManager.SetComponentData(enemyEntity, new HealthBarRef {value = healthBar});
            
        EnemyUIManager.Instance.HealthBars.Add(healthBar);
        EnemyUIManager.Instance.Positions.Add(ui.transform);
    }

    private void DeleteRandom(ref SystemState state)
    {
        //Delete the enemy's GO and Entity 
        var indexToDelete = Random.Range(0, EnemyUIManager.Instance.HealthBars.Count);
        var healthBarToDelete = EnemyUIManager.Instance.HealthBars[indexToDelete];
        Object.Destroy(healthBarToDelete.transform.parent.gameObject);
        state.EntityManager.DestroyEntity(healthBarToDelete.EnemyEntity);
        
        //Update the swapped entity id
        EnemyUIManager.Instance.Positions.RemoveAtSwapBack(indexToDelete);
        EnemyUIManager.Instance.HealthBars.RemoveAtSwapBack(indexToDelete);
        if (indexToDelete < EnemyUIManager.Instance.HealthBars.Count)
        {
            var swappedBackHealthBar = EnemyUIManager.Instance.HealthBars[indexToDelete];
            var swappedBackHealthBarEntity = swappedBackHealthBar.EnemyEntity;
            state.EntityManager.SetComponentData(swappedBackHealthBarEntity, new EnemyId {value = indexToDelete});
        }
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}