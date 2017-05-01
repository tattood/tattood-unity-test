using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialModel : MonoBehaviour {

    public List<GameObject> modelList;
        // Use this for initialization
	void Start () {
        for (int i = 0; i < transform.childCount; i++)
        {
            modelList.Add(transform.GetChild(i).gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {

    }
    public void EnableModel(int modelNo)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i != modelNo)
            {
                modelList[i].SetActive(false);
            }
            else
            {
                modelList[i].SetActive(true);
            }
            
        }
    }
}
