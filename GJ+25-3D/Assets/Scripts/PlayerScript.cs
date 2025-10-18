using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int health;
    public float attackCooldown;
    public float attackTimer;
    private SpriteRenderer spr;
    public GameObject leftRange, rightRange;
    public float speed;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        leftRange.SetActive(false);
        rightRange.SetActive(false);
        attackTimer = 0f;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        attackTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && attackTimer >= attackCooldown)
        {
            StartCoroutine(ActivateAtkHitbox(leftRange));
            FlipX(horizontal);
            attackTimer = 0f;
        }

        if(Input.GetKeyDown(KeyCode.RightArrow) && attackTimer >= attackCooldown)
        {
            StartCoroutine(ActivateAtkHitbox(rightRange));
            FlipX(horizontal);
            attackTimer = 0f;
        }



        

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


    public IEnumerator ActivateAtkHitbox(GameObject atkRange)
    {
        atkRange.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        atkRange.SetActive(false);
    }
}
