using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int amount;

    public class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            AddComponent(new EnemySpawner
            {
                enemyPrefab = GetEntity(authoring.enemyPrefab),
                amount = authoring.amount
            });
        }
    }
}