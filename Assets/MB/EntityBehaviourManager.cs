using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;

public class EntityBehaviourManager : MonoBehaviour
{
    public static EntityBehaviourManager Instance { get; private set; }

    [SerializeField] private bool _destroyInECS;
    
    public TransformAccessArray Positions { get; set; }
    public List<EntityBehaviour> All { get; } = new();

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
        var indexToDelete = UnityEngine.Random.Range(0, All.Count);
        var entityToDestroy = All[indexToDelete];

        if (_destroyInECS)
        {
            var world = entityToDestroy.EntityManager.World;
            var ecbSystem = world.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSystem.CreateCommandBuffer();
            ecb.DestroyEntity(entityToDestroy.Entity);
        }
        else
        {
            Destroy(entityToDestroy.gameObject);
        }
    }

    private void OnDestroy()
    {
        Positions.Dispose();
        Instance = null;
    }
}