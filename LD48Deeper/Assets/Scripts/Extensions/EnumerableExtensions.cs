using System.Collections;
using System.Collections.Generic;
//using System;
using System.Linq;
using UnityEngine;


public static class EnumerableExtensions
{
    public static void Shuffle<T>(this IList<T> list, bool useMyRandom = true)
    {
		
        int n = list.Count;
        while (n > 1)
        {
            n--;
			int k = UnityEngine.Random.Range(0, n + 1);
			if (useMyRandom) {
				//k = MyRandom.Range(0, n + 1);
			}
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

	public static T GetRandomElement<T>(this IList<T> list) {
		return list[Random.Range(0, list.Count)];
	}

	public static T PopRandomElement<T>(this IList<T> list) {
		int rIndex = Random.Range(0, list.Count);
		T result = list[rIndex];
		list.RemoveAt(rIndex);
		return result;
	}

	//gets a random element from a list that is not already in "other"
	//currently brute force... meh
	public static T GetRandomElementNotIn<T>(this IList<T> list, IList<T> other) {
		IList<T> possibleValues = list.Where(listItem => !other.Contains(listItem)).ToList();

		if (possibleValues.Count == 0) {
			throw new System.Exception("All elements in list are contained by other");
		} //error?
		else {
			return possibleValues.GetRandomElement();
		}

	}

	//gets a random element from a list that is not already in "other"
	//currently brute force... meh
	public static T GetRandomElementNotEqualTo<T>(this IList<T> list, T other) {
		T possible = list.GetRandomElement();
		while (possible.Equals(other)) {
			possible = list.GetRandomElement();
		}
		return possible;
	}

	public static List<T> GetRandomDistinctElements<T>(this IList<T> list, int numElements) {
		List<T> result = new List<T>();
		IList<T> copy = list.Select(item => item).ToList();
		copy.Shuffle();
		for (int i = 0; i < numElements && i < list.Count; i++) {
			result.Add(copy[i]);
		}
		return result;
	}

}