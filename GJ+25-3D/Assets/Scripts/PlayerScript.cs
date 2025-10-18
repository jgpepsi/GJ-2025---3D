using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int health;
    public float attackCooldown;
    private SpriteRenderer spr;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        FlipX(horizontal);

    }

    public void FlipX(float horizontal)
    {
        switch (horizontal)
        {
            case > 0:
                spr.flipX = false;
                break;
            case < 0:
                spr.flipX = true;
                break;
            default:
                break;
        }
    }

    public void Attack(float horizontal)
    {
        Debug.Log("Attacked");

    }
}
