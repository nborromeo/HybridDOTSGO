using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial struct EntityBehaviourCleanupSystem : ISystem
{
    private EntityQuery _query;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _query = SystemAPI.QueryBuilder().WithAll<EntityBehaviourReference>().WithNone<EntityBehaviourIndex>().Build();
        state.RequireForUpdate(_query);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var entityBehaviours = _query.ToComponentDataArray<EntityBehaviourReference>();
        for (var i = 0; i < entityBehaviours.Length; i++)
        {
            entityBehaviours[i].value.DestroyAndCleanup();
        }
    }
    
    [BurstCompile] public void OnDestroy(ref SystemState state)  {  }
}