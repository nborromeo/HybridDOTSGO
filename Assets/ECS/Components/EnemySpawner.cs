using Unity.Entities;

public struct EnemySpawner : IComponentData
{
    public Entity enemyPrefab;
    public int amount;
}