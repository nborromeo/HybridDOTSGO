using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof (SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial struct EntityBehaviourCleanupSystem : ISystem
{
    private EntityQuery _query;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _query = SystemAPI.QueryBuilder().WithAll<EntityBehaviourReference>().WithNone<EntityBehaviourActiveTag>().Build();
        state.RequireForUpdate(_query);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        //We can't use SystemAPI.Query because we want to destroy the entity definitely.
        //Alternatively we can delay the destruction into another entity command buffer.
        var entityBehaviours = _query.ToComponentDataArray<EntityBehaviourReference>();
        for (var i = 0; i < entityBehaviours.Length; i++)
        {
            entityBehaviours[i].value.DestroyAndCleanup();
        }
    }
    
    [BurstCompile] public void OnDestroy(ref SystemState state)  {  }
}