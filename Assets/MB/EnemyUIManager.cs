using UnityEngine;
using UnityEngine.Jobs;

public class EnemyUIManager : MonoBehaviour
{
    public static EnemyUIManager Instance { get; private set; }
    
    [SerializeField] private GameObject _prefab;
    [SerializeField] private bool _updateOnMB;
    
    public TransformAccessArray Positions { get; set; }
    public GameObject Prefab => _prefab;
    public bool UpdateOnMb => _updateOnMB;

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