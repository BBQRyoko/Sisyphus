using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggManager : MonoBehaviour
{
    SalmonManager salmonManager;
    Rigidbody2D rig;
    public bool isPickable;
    int damage = 10;

    private void Start()
    {
        isPickable = false;
        salmonManager = FindObjectOfType<SalmonManager>();
        rig = GetComponent<Rigidbody2D>();
    }

    void BounceOnEnemy(bool enemyOnRight) 
    {
        if (enemyOnRight)
        {
            rig.AddForce(new Vector2(-0.15f, 0.2f).normalized * (15f), ForceMode2D.Impulse); ;
        }
        else
        {
            rig.AddForce(new Vector2(0.15f, 0.2f).normalized * (15f), ForceMode2D.Impulse); ;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8) 
        {
            isPickable = true;
        }
        if (collision.gameObject.tag == "Enemy" && !isPickable) 
        {
            salmonManager.containerHealth -= 1;
            collision.gameObject.GetComponent<EnemyManager>().DamageOnEnemy(damage);
            if (transform.position.x < collision.transform.position.x)
                BounceOnEnemy(true);
            else
                BounceOnEnemy(false);
        }
    }
}
