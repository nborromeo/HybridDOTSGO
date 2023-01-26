using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class EntityBehaviour : MonoBehaviour
{
    private int _index;
    private EntityManager _entityManager;
    
    public Entity Entity { get; private set; }

    public void Init(Entity entity, EntityManager entityManager)
    {
        Entity = entity;
        _entityManager = entityManager;
        _index = EntityBehaviourManager.Instance.All.Count;
        _entityManager.AddComponentData(entity, new EntityBehaviourReference {value = this});
        _entityManager.AddComponentData(entity, new EntityBehaviourIndex {value = _index});
        
        EntityBehaviourManager.Instance.Positions.Add(transform);
        EntityBehaviourManager.Instance.All.Add(this);
    }

    public void Destroy()
    {
        EntityBehaviourManager.Instance.Positions.RemoveAtSwapBack(_index);
        EntityBehaviourManager.Instance.All.RemoveAtSwapBack(_index);

        if (_index < EntityBehaviourManager.Instance.All.Count)
        {
            var swappedEntityBehaviour = EntityBehaviourManager.Instance.All[_index];
            swappedEntityBehaviour._index = _index;
            
            var swappedEntityBehaviourEntity = swappedEntityBehaviour.Entity;
            _entityManager.SetComponentData(swappedEntityBehaviourEntity, new EntityBehaviourIndex {value = _index});
        }
 
        _entityManager.RemoveComponent<EntityBehaviourReference>(Entity);
        _entityManager.DestroyEntity(Entity);
        Destroy(gameObject);
    }

    public T GetComponentData<T>() where T : unmanaged, IComponentData
    {
        return _entityManager.GetComponentData<T>(Entity);
    }
}