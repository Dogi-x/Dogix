using UnityEngine;
using UnityEngine.UI;

public class PingIcon : MonoBehaviour
{
    [SerializeField] private Image  icon;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Sprite lootSprite;
    [SerializeField] private Sprite dangerSprite;

    public void Setup(PingType type)
    {
        if (icon == null) return;
        icon.sprite = type switch
        {
            PingType.Enemy  => enemySprite,
            PingType.Loot   => lootSprite,
            _               => dangerSprite,
        };
    }
}
