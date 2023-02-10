using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        
        var transformIndices = _entityQuery.ToComponentDataArray<EntityTransformIndex>(Allocator.TempJob);
        var queryPositions = _entityQuery.ToComponentDataArray<WorldTransform>(Allocator.TempJob);
        var transformIndexToQueryIndex = new NativeArray<int>(transformIndices.Length, Allocator.TempJob);

        var indicesJob = new QueryIndicesJob {transformIndices = transformIndices, transformIndexToQueryIndex = transformIndexToQueryIndex};
        var indicesJobHandle = indicesJob.Schedule(queryPositions.Length, 100, state.Dependency);

        var positionJob = new PositionJob {queryPositions = queryPositions, transformIndexToQueryIndex = transformIndexToQueryIndex};
        state.Dependency = positionJob.Schedule(EntityBehaviourManager.Instance.Transforms, indicesJobHandle);
    }

    /// <summary>
    /// Given the ECS entity might not be ordered by entity index in our position query (_entityQuery) as the ui
    /// transforms. We need to map each entity id to the current index in the query, which can vary frame to frame.
    /// </summary>
    [BurstCompile]
    private struct QueryIndicesJob : IJobParallelFor
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<EntityTransformIndex> transformIndices;
        [NativeDisableParallelForRestriction] public NativeArray<int> transformIndexToQueryIndex;
        
        public void Execute(int queryIndex)
        {
            var transformIndex = transformIndices[queryIndex].value;
            transformIndexToQueryIndex[transformIndex] = queryIndex;
        }
    }
    
    /// <summary>
    /// Iterate every entity ui (ordered by entity id) and copy the entity ECS position. Consider transforms in the
    /// TransformAccess must not have parent in order to be processed in different threads
    /// </summary>
    [BurstCompile]
    private struct PositionJob : IJobParallelForTransform
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<WorldTransform> queryPositions;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int> transformIndexToQueryIndex;
        
        public void Execute(int transformIndex, TransformAccess transform)
        {
            var queryIndex = transformIndexToQueryIndex[transformIndex];
            transform.position = queryPositions[queryIndex].Position;
        }
    }

    [BurstCompile] public void OnDestroy(ref SystemState state) { }
}