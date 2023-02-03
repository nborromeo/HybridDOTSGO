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
public partial struct EntityBehaviourPositionUpdaterSystem : ISystem
{
    private EntityQuery _entityQuery;
    private NativeArray<int> _entityIndexToQueryIndex;
    private NativeArray<EntityBehaviourIndex> _entityIndices;
    private NativeArray<LocalToWorld> _entityPositions;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _entityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, EntityBehaviourIndex>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (EntityBehaviourManager.Instance == null || !EntityBehaviourManager.Instance.Transforms.isCreated)
        {
            return;
        }
      
        //We dispose the previous frame used arrays. An alternative is to clean them in a system that executes
        //at the end of this frame, but could cause a sync point while waiting these jobs to end.
        DisposeArrays();

        using (new ProfilerMarker("Get Arrays").Auto())
        {  
            _entityIndices = _entityQuery.ToComponentDataArray<EntityBehaviourIndex>(Allocator.TempJob);
            _entityPositions = _entityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            _entityIndexToQueryIndex = new NativeArray<int>(_entityIndices.Length, Allocator.TempJob);
        }

        var indicesJob = new QueryIndicesJob {entityIndices = _entityIndices, entityIndexToQueryIndex = _entityIndexToQueryIndex};
        var indicesJobHandle = indicesJob.Schedule(_entityIndices.Length, 100, state.Dependency);
        
        var positionJob = new PositionJob {positions = _entityPositions, entityIndexToQueryIndex = _entityIndexToQueryIndex};
        state.Dependency = positionJob.Schedule(EntityBehaviourManager.Instance.Transforms, indicesJobHandle);
    }

    private void DisposeArrays()
    {
        if (_entityPositions.IsCreated)
        {
            _entityIndices.Dispose();
            _entityPositions.Dispose();
            _entityIndexToQueryIndex.Dispose();
        }
    }

    /// <summary>
    /// Given the ECS entity might not be ordered by entity index in our position query (_entityQuery) as the ui
    /// transforms. We need to map each entity id to the current index in the query, which can vary frame to frame.
    /// </summary>
    [BurstCompile]
    private struct QueryIndicesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EntityBehaviourIndex> entityIndices;
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
        [ReadOnly] public NativeArray<LocalToWorld> positions;
        [ReadOnly] public NativeArray<int> entityIndexToQueryIndex;
        
        public void Execute(int entityIndex, TransformAccess transform)
        {
            var queryIndex = entityIndexToQueryIndex[entityIndex];
            transform.position = positions[queryIndex].Position;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        DisposeArrays();
    }
}