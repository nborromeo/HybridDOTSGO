using UnityEngine;
using UnityEngine.Jobs;

public class EnemyUI : MonoBehaviour
{
    public static EnemyUI Instance { get; private set; }
    
    private Transform[] _positions;
    
    [SerializeField] private int _amount;
    [SerializeField] private GameObject _enemyUIPrefab;

    public TransformAccessArray Positions { get; private set; }

    private void Awake()
    {
        Instance = this;

        _positions = new Transform[_amount];
        for (var i = 0; i < _amount; i++)
        {
            var enemyUI = Instantiate(_enemyUIPrefab);
            _positions[i] = enemyUI.transform;
            enemyUI.GetComponentInChildren<HealthBar>().EnemyId = i;

            if (enemyUI.TryGetComponent<HealthBarUpdateFromECS>(out var updateFromEcs))
            {
                updateFromEcs.EnemyId = i;
            }
        }
        
        Positions = new TransformAccessArray(_positions);
    }
    
    private void OnDestroy()
    {
        Instance = null;
        Positions.Dispose();
    } 
}