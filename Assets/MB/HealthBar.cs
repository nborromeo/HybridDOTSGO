using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private static readonly Dictionary<int, Image> _allBars = new();

    public static Image GetEnemyBar(int enemyId) => _allBars[enemyId];
    
    private int _enemyId = -1;
    [SerializeField] private Image _image;

    public Image Image => _image;

    public int EnemyId
    {
        get => _enemyId;
        set
        {
            _enemyId = value;
            _allBars.Add(_enemyId, _image);
        }
    }

    private void OnDestroy()
    {
        if (_enemyId >= 0)
        {
            _allBars.Remove(_enemyId);
        }
    }
}