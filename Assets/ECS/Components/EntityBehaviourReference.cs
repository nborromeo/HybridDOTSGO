using Unity.Entities;

public class EntityBehaviourReference : ICleanupComponentData
{
    public EntityBehaviour value;
}

public  class EntityBehaviourActiveTag : IComponentData { }