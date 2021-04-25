using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
	public float DestroyTime = 1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		DestroyTime -= Time.deltaTime;
		if(DestroyTime <= 0) {
			Destroy(this.gameObject);
		}
    }
}
