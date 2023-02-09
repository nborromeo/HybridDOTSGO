using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine.Jobs;

/// <summary>
/// Synchronizes positions of linked entities.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof (SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EntityBehaviourCleanupSystem))]
public partial struct EntityBehaviourTransformUpdateSystem : ISystem
{
    private EntityQuery _entityQuery;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _entityQuery = SystemAPI.QueryBuilder().WithAll<WorldTransform, EntityTransformIndex>().Build();
        state.RequireForUpdate(_entityQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (EntityBehaviourManager.Instance == null || !EntityBehaviourManager.Instance.Transforms.isCreated)
        {
            return;
        }
        
        var entityIndices = _entityQuery.ToComponentDataArray<EntityTransformIndex>(Allocator.TempJob);
        var entityPositions = _entityQuery.ToComponentDataArray<WorldTransform>(Allocator.TempJob);
        var entityIndexToQueryIndex = new NativeArray<int>(entityIndices.Length, Allocator.TempJob);

        var indicesJob = new QueryIndicesJob {entityIndices = entityIndices, entityIndexToQueryIndex = entityIndexToQueryIndex};
        var indicesJobHandle = indicesJob.Schedule(entityPositions.Length, 100, state.Dependency);

        var positionJob = new PositionJob {positions = entityPositions, entityIndexToQueryIndex = entityIndexToQueryIndex};
        state.Dependency = positionJob.Schedule(EntityBehaviourManager.Instance.Transforms, indicesJobHandle);
    }

    /// <summary>
    /// Given the ECS entity might not be ordered by entity index in our position query (_entityQuery) as the ui
    /// transforms. We need to map each entity id to the current index in the query, which can vary frame to frame.
    /// </summary>
    [BurstCompile]
    private struct QueryIndicesJob : IJobParallelFor
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<EntityTransformIndex> entityIndices;
        [NativeDisableParallelForRestriction] public NativeArray<int> entityIndexToQueryIndex;
        
        public void Execute(int queryIndex)
        {
            var entityIndex = entityIndices[queryIndex].value;
            entityIndexToQueryIndex[entityIndex] = queryIndex;
        }
    }
    
    /// <summary>
    /// Iterate every entity ui (ordered by entity id) and copy the entity ECS position. Consider transforms in the
    /// TransformAccess must not have parent in order to be processed in different threads
    /// </summary>
    [BurstCompile]
    private struct PositionJob : IJobParallelForTransform
    {     
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<WorldTransform> positions;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int> entityIndexToQueryIndex;
        
        public void Execute(int entityIndex, TransformAccess transform)
        {
            var queryIndex = entityIndexToQueryIndex[entityIndex];
            transform.position = positions[queryIndex].Position;
        }
    }

    [BurstCompile] public void OnDestroy(ref SystemState state) { }
}