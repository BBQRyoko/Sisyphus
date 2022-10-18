using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    SpriteRenderer sprite;
    [SerializeField] int health = 20;
    public bool isStunned;
    [SerializeField] float stunTimer;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            if (isStunned)
            {
                sprite.flipY = true;
                stunTimer += Time.deltaTime;
                if (stunTimer >= 2.5f)
                {
                    stunTimer = 0;
                    isStunned = false;
                }
            }
            else
            {
                sprite.flipY = false;
                stunTimer = 0;
            }
        }
    }
    public void DamageOnEnemy(int damage) 
    {
        health -= damage;
    }
    public void StunOnEnemy() 
    {
        isStunned = true;
        stunTimer = 0;
    }
}
