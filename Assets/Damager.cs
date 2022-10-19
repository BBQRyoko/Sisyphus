using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    [SerializeField] int damage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.gameObject.GetComponent<SalmonManager>().isDamaged) 
            {
                if (collision.transform.position.x < transform.position.x)
                {
                    collision.gameObject.GetComponent<SalmonManager>().PlayerTakeDamage(damage, true);
                }
                else
                {
                    collision.gameObject.GetComponent<SalmonManager>().PlayerTakeDamage(damage, false);
                }
            }
            Destroy(this.gameObject);
        }
        if (collision.gameObject.layer == 8) 
        {
            Destroy(this.gameObject);
        }
    }
}
