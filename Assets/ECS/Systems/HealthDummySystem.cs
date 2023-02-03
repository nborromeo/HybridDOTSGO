using Unity.Burst;
using Unity.Entities;

/// <summary>
/// Dummy system to simulate write workload on the health component
/// </summary>
[BurstCompile]
public partial struct HealthDummySystem : ISystem
{
    [BurstCompile] public void OnCreate(ref SystemState state) { }
    [BurstCompile] public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Schedule some dummy jobs to simulate some heavy logic
        var job1 = new HealthDummyJob().Schedule(state.Dependency);
        var job2 = new HealthDummyJob().Schedule(job1);
        state.Dependency = new HealthDummyJob().Schedule(job2);
    }
    
    [BurstCompile]
    partial struct HealthDummyJob : IJobEntity
    {
        void Execute(ref Health health)
        {
            health.value += 1;
        }
    }
}