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
[UpdateAfter(typeof(EntityBehaviourCleanupSystem))]
public partial struct EntityBehaviourPositionUpdaterSystem : ISystem
{
    private EntityQuery _entityQuery;
    private NativeArray<int> _entityQueryIndexById;
    private NativeArray<EntityBehaviourIndex> _entityIds;
    private NativeArray<LocalToWorld> _entityPositions;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _entityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld>().WithAll<EntityBehaviourIndex>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (EntityBehaviourManager.Instance == null || !EntityBehaviourManager.Instance.Positions.isCreated)
        {
            return;
        }
        
        //We dispose the previous frame used arrays. An alternative is to clean them in a system that executes
        //at the end of this frame, but could cause a sync point while waiting these jobs to end.
        DisposeArrays();
        
        _entityIds = _entityQuery.ToComponentDataArray<EntityBehaviourIndex>(Allocator.TempJob);
        _entityPositions = _entityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        _entityQueryIndexById = new NativeArray<int>(_entityIds.Length, Allocator.TempJob);
        
        var indicesJob = new QueryIndicesJob {entityIds = _entityIds, entityQueryIndexById = _entityQueryIndexById};
        var positionJob = new PositionJob {positions = _entityPositions, entityQueryIndexById = _entityQueryIndexById};
        var indicesJobHandle = indicesJob.Schedule(_entityIds.Length, 100, state.Dependency);
        state.Dependency = positionJob.Schedule(EntityBehaviourManager.Instance.Positions, indicesJobHandle);
    }

    private void DisposeArrays()
    {
        if (_entityPositions.IsCreated)
        {
            _entityPositions.Dispose();
            _entityIds.Dispose();
            _entityQueryIndexById.Dispose();
        }
    }

    /// <summary>
    /// Given the ECS entity might not be ordered by entity id in our position query (_entityQuery) as the ui transforms
    /// we need to map each entity id to the current index in the query, which can vary frame to frame.
    /// </summary>
    private struct QueryIndicesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EntityBehaviourIndex> entityIds;
        [NativeDisableParallelForRestriction] public NativeArray<int> entityQueryIndexById;
        
        public void Execute(int entityQueryIndex)
        {
            var entityId = entityIds[entityQueryIndex].value;
            entityQueryIndexById[entityId] = entityQueryIndex;
        }
    }
    
    /// <summary>
    /// Iterate every entity ui (ordered by entity id) and copy the entity ECS position. Consider transforms in the
    /// TransformAccess must not have parent in order to be processed in different threads
    /// </summary>
    private struct PositionJob : IJobParallelForTransform
    {     
        [ReadOnly] public NativeArray<LocalToWorld> positions;
        [ReadOnly] public NativeArray<int> entityQueryIndexById;
        
        public void Execute(int uiIndex, TransformAccess transform)
        {
            var entityId = uiIndex; //The entity ui transforms are ordered by entity id, then the index is the id.
            var entityQueryIndex = entityQueryIndexById[entityId];
            transform.position = positions[entityQueryIndex].Position;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        DisposeArrays();
    }
}