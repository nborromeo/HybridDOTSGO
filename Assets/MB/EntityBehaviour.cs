using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

public class EntityBehaviour : MonoBehaviour
{
    private int _index = -1;
    private World _world;

    private bool NeedsCleanup => _index >= 0;
    private bool EntityManagerExists => _world.IsCreated;

    public EntityManager EntityManager { get; private set; }
    public Entity Entity { get; private set; }

    public void Init(Entity entity, EntityManager entityManager)
    {
        _index = EntityBehaviourManager.Instance.All.Count;
        _world = entityManager.World;
        
        Entity = entity;
        EntityManager = entityManager;
        EntityManager.AddComponentData(entity, new EntityBehaviourReference {value = this});
        EntityManager.AddComponentData(entity, new EntityBehaviourIndex {value = _index});
        
        EntityBehaviourManager.Instance.Positions.Add(transform);
        EntityBehaviourManager.Instance.All.Add(this);
    }

    public void DestroyAndCleanup()
    {
        Destroy(gameObject);
        Cleanup();
    }

    private void Cleanup()
    {
        if (EntityBehaviourManager.Instance != null)
        {
            EntityBehaviourManager.Instance.Positions.RemoveAtSwapBack(_index);
            EntityBehaviourManager.Instance.All.RemoveAtSwapBack(_index);

            if (_index < EntityBehaviourManager.Instance.All.Count)
            {
                var swappedBehaviour = EntityBehaviourManager.Instance.All[_index];
                swappedBehaviour._index = _index;
                
                if (EntityManagerExists)
                {
                    var swappedEntity = swappedBehaviour.Entity;
                    if (EntityManager.HasComponent<EntityBehaviourIndex>(swappedEntity))
                    {
                        EntityManager.SetComponentData(swappedEntity, new EntityBehaviourIndex {value = _index});
                    }
                }
            }
        }

        _index = -1;

        if (EntityManagerExists)
        {
            EntityManager.RemoveComponent<EntityBehaviourReference>(Entity);
            EntityManager.DestroyEntity(Entity);
        }
    }
    
    public T GetComponentData<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.GetComponentData<T>(Entity);
    }

    private void OnDestroy()
    {
        if (NeedsCleanup)
        {
            Cleanup();
        }
    }
}