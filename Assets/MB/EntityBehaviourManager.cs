using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public class EntityBehaviourManager : MonoBehaviour
{
    public static EntityBehaviourManager Instance { get; private set; }
    
    public TransformAccessArray Positions { get; set; }
    public List<EntityBehaviour> All { get; } = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
        Positions.Dispose();
    }
}