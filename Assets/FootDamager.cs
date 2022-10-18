using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootDamager : MonoBehaviour
{
    SalmonManager salmonManager;
    private void Start()
    {
        salmonManager = GetComponentInParent<SalmonManager>();
    }
    void BounceOnEnemy()
    {
        salmonManager.rig.velocity = new Vector2(salmonManager.rig.velocity.x, 10f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !salmonManager.isDamaged)
        {
            BounceOnEnemy();
            collision.gameObject.GetComponent<EnemyManager>().StunOnEnemy();
        }
    }
}
