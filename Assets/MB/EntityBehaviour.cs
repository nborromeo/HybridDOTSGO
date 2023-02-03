using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class EntityBehaviour : MonoBehaviour
{
    private bool _syncTransforms;
    private int _index = -1;
    private World _world;

    private bool NeedsCleanup => _index >= 0;
    private bool EntityManagerExists => _world.IsCreated;

    public EntityManager EntityManager { get; private set; }
    public Entity Entity { get; private set; }

    public void Init(Entity entity, EntityManager entityManager, bool syncTransforms = true)
    {
        //Save the references to read/write data from our entity counterpart
        Entity = entity;
        EntityManager = entityManager;
        _world = entityManager.World;
        _syncTransforms = syncTransforms;
        
        //Save the index of our transform in the TransformAccessArray before adding ourselves to it
        if (syncTransforms)
        {
            _index = EntityBehaviourManager.Instance.Transforms.length;
            EntityManager.AddComponentData(entity, new EntityTransformIndex {value = _index});
            EntityBehaviourManager.Instance.Transforms.Add(transform);
            EntityBehaviourManager.Instance.TransformsBehaviours.Add(this);
        }
        else
        {
            _index = int.MaxValue;
        }

        //Add a cleanup component to the entity to make sure it destroy this GameObject when cleaning up
        EntityManager.AddComponentData(entity, new EntityBehaviourReference {value = this});
        EntityManager.AddComponent<EntityBehaviourActiveTag>(entity);
    }

    public void DestroyAndCleanup()
    {
        Destroy(gameObject);
        Cleanup();
    }

    private void Cleanup()
    {
        if (_syncTransforms && EntityBehaviourManager.Instance != null)
        {
            EntityBehaviourManager.Instance.Transforms.RemoveAtSwapBack(_index);
            EntityBehaviourManager.Instance.TransformsBehaviours.RemoveAtSwapBack(_index);
            var isLast = _index >= EntityBehaviourManager.Instance.Transforms.length;

            if (!isLast)
            {
                var swappedBehaviour = EntityBehaviourManager.Instance.TransformsBehaviours[_index];
                swappedBehaviour._index = _index;
                
                if (EntityManagerExists)
                {
                    var swappedEntity = swappedBehaviour.Entity;
                    if (EntityManager.HasComponent<EntityTransformIndex>(swappedEntity))
                    {
                        EntityManager.SetComponentData(swappedEntity, new EntityTransformIndex {value = _index});
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