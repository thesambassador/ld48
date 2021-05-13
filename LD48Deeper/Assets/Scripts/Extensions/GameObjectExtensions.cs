using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	public static T SearchForComponentUpwards<T>(this GameObject gameObject) where T : class {
		T result = null;
		Transform currTarget = gameObject.transform.parent;
		result = gameObject.GetComponent<T>();
		while(currTarget != null && result == null) {
			result = currTarget.GetComponent<T>();
			currTarget = currTarget.parent;
		}
		return result;
	}
}
