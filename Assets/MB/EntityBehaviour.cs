using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class EntityBehaviour : MonoBehaviour
{
    public Entity Entity { get; set; }
    public EntityManager EntityManager { get; set; }
    public int Index { get; private set; }

    protected virtual void Awake()
    {
        Index = EntityBehaviourManager.Instance.All.Count;
        EntityBehaviourManager.Instance.Positions.Add(transform);
        EntityBehaviourManager.Instance.All.Add(this);
    }

    public void Destroy()
    {
        EntityBehaviourManager.Instance.Positions.RemoveAtSwapBack(Index);
        EntityBehaviourManager.Instance.All.RemoveAtSwapBack(Index);

        if (Index < EntityBehaviourManager.Instance.All.Count)
        {
            var swappedEntityBehaviour = EntityBehaviourManager.Instance.All[Index];
            swappedEntityBehaviour.Index = Index;
            
            var swappedEntityBehaviourEntity = swappedEntityBehaviour.Entity;
            EntityManager.SetComponentData(swappedEntityBehaviourEntity, new EntityBehaviourIndex {value = Index});
        }

        Destroy(gameObject);
    }

    public T GetComponentData<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.GetComponentData<T>(Entity);
    }
}