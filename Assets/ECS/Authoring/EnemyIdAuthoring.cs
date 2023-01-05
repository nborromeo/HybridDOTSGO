using Unity.Entities;
using UnityEngine;

public class EnemyIdAuthoring : MonoBehaviour
{
    [SerializeField] private int _id;
    
    public class Baker : Baker<EnemyIdAuthoring>
    {
        public override void Bake(EnemyIdAuthoring authoring)
        {
            AddComponent(new EnemyId {value = authoring._id});
        }
    }
}