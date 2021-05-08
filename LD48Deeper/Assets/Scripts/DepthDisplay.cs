using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthDisplay : MonoBehaviour
{
	public PlayerController PlayerObject;
	public TMPro.TMP_Text DisplayText;

	public int CurDepth = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (PlayerObject != null && PlayerObject.gameObject.activeInHierarchy) {
			int depth = -(int)PlayerObject.transform.position.y;
			if (depth < 0) depth = 0;
			SetDepth(depth);
		}
    }

	public void SetDepth(int depth) {
		CurDepth = depth;
		DisplayText.text = "Depth: " + depth;
	}
}
