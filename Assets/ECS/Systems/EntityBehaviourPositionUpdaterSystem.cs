using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

/// <summary>
/// Updates the canvas health bar positions to follow the ECS enemies position.
/// </summary>
[BurstCompile]
public partial struct EntityBehaviourPositionUpdaterSystem : ISystem
{
    private EntityQuery _enemyQuery;
    private NativeArray<int> _enemyQueryIndexById;
    private NativeArray<EntityBehaviourIndex> _enemyIds;
    private NativeArray<LocalToWorld> _enemyPositions;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _enemyQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld>().WithAll<EntityBehaviourIndex>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (EntityBehaviourManager.Instance == null || !EntityBehaviourManager.Instance.Positions.isCreated)
        {
            return;
        }
        
        //We dispose the previous frame used arrays. An alternative is to clean them in a system that executes
        //at the end of this frame, but could cause a sync point while waiting this jobs to end.
        DisposeArrays();
        
        _enemyIds = _enemyQuery.ToComponentDataArray<EntityBehaviourIndex>(Allocator.TempJob);
        _enemyPositions = _enemyQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        _enemyQueryIndexById = new NativeArray<int>(_enemyIds.Length, Allocator.TempJob);
        
        var indicesJob = new QueryIndicesJob {enemyIds = _enemyIds, enemyQueryIndexById = _enemyQueryIndexById};
        var positionJob = new PositionJob {positions = _enemyPositions, enemyQueryIndexById = _enemyQueryIndexById};
        var indicesJobHandle = indicesJob.Schedule(_enemyIds.Length, 100, state.Dependency);
        state.Dependency = positionJob.Schedule(EntityBehaviourManager.Instance.Positions, indicesJobHandle);
    }

    private void DisposeArrays()
    {
        if (_enemyPositions.IsCreated)
        {
            _enemyPositions.Dispose();
            _enemyIds.Dispose();
            _enemyQueryIndexById.Dispose();
        }
    }

    /// <summary>
    /// Given the ECS enemies might not be ordered by enemy id in our position query (_enemyQuery) as the ui transforms
    /// we need to map each enemy id to the current index in the query, which can vary frame to frame.
    /// </summary>
    private struct QueryIndicesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EntityBehaviourIndex> enemyIds;
        [NativeDisableParallelForRestriction] public NativeArray<int> enemyQueryIndexById;
        
        public void Execute(int enemyQueryIndex)
        {
            var enemyId = enemyIds[enemyQueryIndex].value;
            enemyQueryIndexById[enemyId] = enemyQueryIndex;
        }
    }
    
    /// <summary>
    /// Iterate every enemy ui (ordered by enemy id) and copy the enemy ECS position. Consider transforms in the
    /// TransformAccess must not have parent in order to be processed in different threads
    /// </summary>
    private struct PositionJob : IJobParallelForTransform
    {     
        [ReadOnly] public NativeArray<LocalToWorld> positions;
        [ReadOnly] public NativeArray<int> enemyQueryIndexById;
        
        public void Execute(int uiIndex, TransformAccess transform)
        {
            var enemyId = uiIndex; //The enemy ui transforms are ordered by enemy id, then the index is the id.
            var enemyQueryIndex = enemyQueryIndexById[enemyId];
            transform.position = positions[enemyQueryIndex].Position;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        DisposeArrays();
    }
}