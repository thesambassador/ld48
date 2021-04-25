using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AmmoDisplay : MonoBehaviour
{
	public AmmoController TargetAmmo;
	public TMPro.TMP_Text AmmoText;
    // Start is called before the first frame update
    void Start()
    {
		TargetAmmo.EventAmmoChanged.AddListener(OnAmmoChanged);
		AmmoText.text = TargetAmmo.CurrentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnAmmoChanged(int newAmount) {
		AmmoText.text = newAmount.ToString();
	}
}
