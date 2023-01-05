using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    [SerializeField] private int _health;
    
    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            AddComponent(new Health {value = authoring._health});
        }
    }
}