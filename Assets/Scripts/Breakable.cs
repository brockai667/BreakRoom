using UnityEngine;

public class Breakable : MonoBehaviour
{
    public int hp = 3;
    public int damage = 1;

    public void Hit()
    {
        hp -= damage;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}