using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
        //Save the references to read/write data from our entity counterpart
        Entity = entity;
        EntityManager = entityManager;
        
        //Save the index of our transform in the TransformAccessArray before adding ourselves to it
        _index = EntityBehaviourManager.Instance.Transforms.length;
        EntityManager.AddComponentData(entity, new EntityBehaviourIndex {value = _index});
        EntityBehaviourManager.Instance.Transforms.Add(transform);
        
        //Add a cleanup component to the entity to make sure it destroy this GameObject when cleaning up
        EntityManager.AddComponentData(entity, new EntityBehaviourReference {value = this});
        
        EntityBehaviourManager.Instance.All.Add(this);
        _world = entityManager.World;
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
            EntityBehaviourManager.Instance.Transforms.RemoveAtSwapBack(_index);
            EntityBehaviourManager.Instance.All.RemoveAtSwapBack(_index);
            var isLast = _index >= EntityBehaviourManager.Instance.Transforms.length;

            if (!isLast)
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
    
    public bool HasComponent<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.HasComponent<T>(Entity);
    }

    private void OnDestroy()
    {
        if (NeedsCleanup)
        {
            Cleanup();
        }
    }
}