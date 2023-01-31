using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

public class EntityBehaviour : MonoBehaviour
{
    private int _index = -1;
    
    public EntityManager EntityManager { get; private set; }
    public Entity Entity { get; private set; }

    public void Init(Entity entity, EntityManager entityManager)
    {
        Entity = entity;
        EntityManager = entityManager;
        _index = EntityBehaviourManager.Instance.All.Count;
        EntityManager.AddComponentData(entity, new EntityBehaviourReference {value = this});
        EntityManager.AddComponentData(entity, new EntityBehaviourIndex {value = _index});
        
        EntityBehaviourManager.Instance.Positions.Add(transform);
        EntityBehaviourManager.Instance.All.Add(this);
    }

    public void Destroy()
    {
        Destroy(true);
    }

    private void Destroy(bool destroyGO)
    {
        if (_index < 0)
        {
            return;
        }

        if (EntityBehaviourManager.Instance != null)
        {
            EntityBehaviourManager.Instance.Positions.RemoveAtSwapBack(_index);
            EntityBehaviourManager.Instance.All.RemoveAtSwapBack(_index);

            if (_index < EntityBehaviourManager.Instance.All.Count)
            {
                var swappedEntityBehaviour = EntityBehaviourManager.Instance.All[_index];
                swappedEntityBehaviour._index = _index;

                var swappedEntityBehaviourEntity = swappedEntityBehaviour.Entity;
                EntityManager.SetComponentData(swappedEntityBehaviourEntity, new EntityBehaviourIndex { value = _index });
            }
            
            EntityManager.RemoveComponent<EntityBehaviourReference>(Entity);
            EntityManager.DestroyEntity(Entity);
        }

        _index = -1;

        if (destroyGO)
        {
            Object.Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Destroy(false);
    }

    public T GetComponentData<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.GetComponentData<T>(Entity);
    }
}