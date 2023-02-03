using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;

public class EntityBehaviourManager : MonoBehaviour
{
    public static EntityBehaviourManager Instance { get; private set; }

    [SerializeField] private bool _destroyInMb;
    
    public TransformAccessArray Transforms { get; set; }
    public List<EntityBehaviour> TransformsBehaviours { get; } = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DeleteRandom();
        }
    }

    private void DeleteRandom()
    {
        var indexToDelete = Random.Range(0, TransformsBehaviours.Count);
        var entityToDestroy = TransformsBehaviours[indexToDelete];

        if (_destroyInMb)
        {
            Destroy(entityToDestroy.gameObject);
        }
        else
        {
            var world = entityToDestroy.EntityManager.World;
            var ecbSystem = world.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSystem.CreateCommandBuffer();
            ecb.DestroyEntity(entityToDestroy.Entity);
        }
    }

    private void OnDestroy()
    {
        Transforms.Dispose();
        Instance = null;
    }
}