using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParicleSytemScaler : MonoBehaviour
{
    public GameObject[] particleSytems;
    public float searchDistance;
    public float distance;
    private void Update()
    {
        int i = 0;
        GameObject tmpPrevios = particleSytems[particleSytems.Length - 1];
        SortArray(particleSytems);
        foreach (GameObject particleSystem in particleSytems)
        {
            i++;         
            if(Vector2.Distance(new Vector2(particleSystem.transform.position.x, particleSystem.transform.position.y),
                new Vector2( gameObject.transform.position.x, gameObject.transform.position.y)) > searchDistance)
            {
                particleSystem.transform.position = tmpPrevios.transform.position + new Vector3(distance, 0, 0);
            }
            tmpPrevios = particleSystem;
        } 
    }
    public static GameObject[] SortArray(GameObject[] array)
    {
        int length = array.Length;

        GameObject temp = array[0];

        for (int i = 0; i < length; i++)
        {
            for (int j = i + 1; j < length; j++)
            {
                if (array[i].transform.position.x > array[j].transform.position.x)
                {
                    temp = array[i];

                    array[i] = array[j];

                    array[j] = temp;
                }
            }
        }

        return array;
    }
}
