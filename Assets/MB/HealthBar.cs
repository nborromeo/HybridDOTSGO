using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _image;
    
    public Image Image => _image;
    public Entity EnemyEntity { get; set; }

    private void Update()
    {
        if (EnemyUIManager.Instance.UpdateOnMb && EnemyEntity != Entity.Null)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var enemyHealth = em.GetComponentData<Health>(EnemyEntity);
            Image.fillAmount = enemyHealth.value / 100f;
        }
    }
}