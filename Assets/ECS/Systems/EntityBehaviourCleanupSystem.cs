using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(EnemySpawnerSystem))]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct EntityBehaviourCleanupSystem : ISystem
{
    private EntityQuery _query;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _query = SystemAPI.QueryBuilder().WithAll<EntityBehaviourReference>().WithNone<LocalToWorld>().Build();
        state.RequireForUpdate(_query);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityBehaviours = _query.ToComponentDataArray<EntityBehaviourReference>();
        var entities = _query.ToEntityArray(Allocator.Temp);
        
        for (var i = 0; i < entityBehaviours.Length; i++)
        {
            entityBehaviours[i].value.Destroy();
            state.EntityManager.RemoveComponent<EntityBehaviourReference>(entities[i]);
            state.EntityManager.DestroyEntity(entities[i]);
        }
    }
    
    [BurstCompile] public void OnDestroy(ref SystemState state)  {  }
}