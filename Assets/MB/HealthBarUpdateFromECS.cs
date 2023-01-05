using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HealthBarUpdateFromECS : MonoBehaviour
{
    private Entity _enemyEntity;
    private EntityQuery _enemiesQuery;
    private ComponentLookup<Health> _enemiesHealth;
    private HealthBar _healthBar;

    public int EnemyId { get; set; }

    private void Awake()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _enemiesQuery = em.CreateEntityQuery(ComponentType.ReadOnly<EnemyId>());
        _healthBar = GetComponent<HealthBar>();
    }

    private void Update()
    {
        if (HasEnemyEntity())
        {
            var health = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Health>(_enemyEntity);
            _healthBar.Image.fillAmount = health.value / 100f;
        }
    }

    private bool HasEnemyEntity()
    {
        if (_enemyEntity != Entity.Null)
        {
            return true;
        }

        var enemyIds = _enemiesQuery.ToComponentDataArray<EnemyId>(Allocator.Temp);
        if (enemyIds.Length <= 0)
        {
            return false;
        }

        for (var i = 0; i < enemyIds.Length; i++)
        {
            var enemyId = enemyIds[i].value;
            if (enemyId == EnemyId)
            {
                var enemyEntities = _enemiesQuery.ToEntityArray(Allocator.Temp);
                _enemyEntity = enemyEntities[i];
                return true;
            }
        }

        return false;
    }
}