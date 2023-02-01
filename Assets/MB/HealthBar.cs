using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private EntityBehaviour _behaviour;
    [SerializeField] private Image _image;

    public Image Image => _image;

    private void Awake()
    {
        _behaviour = GetComponentInParent<EntityBehaviour>();
        enabled = EnemyUIManager.Instance.UpdateOnMb;
    }

    private void Update()
    {
        if (_behaviour.HasComponent<Health>())
        {
            var enemyHealth = _behaviour.GetComponentData<Health>();
            Image.fillAmount = enemyHealth.value / 100f;
        }
    }
}