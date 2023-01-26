using UnityEngine;

public class EnemyUIManager : MonoBehaviour
{
    public static EnemyUIManager Instance { get; private set; }
    
    [SerializeField] private EntityBehaviour _prefab;
    [SerializeField] private bool _updateOnMB;
    
    public EntityBehaviour Prefab => _prefab;
    public bool UpdateOnMb => _updateOnMB;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}