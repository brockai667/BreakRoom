using UnityEngine;

public class Breakable : MonoBehaviour
{
    public int hp = 3;
    public int damage = 1;
    public Material[] damageMaterials; // zorad od najmenej po najviac poškodený

    private int maxHp;
    private Renderer objectRenderer;

    void Start()
    {
        maxHp = hp;
        objectRenderer = GetComponent<Renderer>();
        UpdateMaterial();
    }

    public void Hit()
    {
        hp -= damage;
        UpdateMaterial();

        if (hp <= 0)
            Destroy(gameObject);
    }

    void UpdateMaterial()
    {
        if (damageMaterials == null || damageMaterials.Length == 0) return;

        float percent = (float)hp / maxHp;
        int index = Mathf.FloorToInt((1 - percent) * damageMaterials.Length);
        index = Mathf.Clamp(index, 0, damageMaterials.Length - 1);

        objectRenderer.material = damageMaterials[index];
    }
}