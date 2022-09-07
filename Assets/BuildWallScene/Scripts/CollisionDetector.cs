using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public List<GameObject> collidingObjects = new List<GameObject>();
    private void OnDestroy()
    {
        Builder.instance.isCollidingWithFirstWall = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(gameObject.name.Contains("Window") || gameObject.name.Contains("Door")))
        {
            collidingObjects.Add(other.gameObject);

            if (other.gameObject.CompareTag("FirstWall"))
            {
                if (Builder.instance.walls.Count > 3 && collidingObjects.FindAll(x => x.gameObject.name.Contains("Wall")).Count <= 2)
                    Builder.instance.isCollidingWithFirstWall = true;
            }
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if(gameObject.name.Contains("Window") || gameObject.name.Contains("Door"))
        {
            GameObject.Find("---BUILDER").GetComponent<ObjectBuilder>().currentWall = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(gameObject.name.Contains("Window") || gameObject.name.Contains("Door")))
        {
            collidingObjects.Remove(other.gameObject);

            if (other.gameObject.CompareTag("FirstWall"))
            {
                Builder.instance.isCollidingWithFirstWall = false;
            }
        }

        else
        {
            GameObject.Find("---BUILDER").GetComponent<ObjectBuilder>().currentWall = null;
        }
    }
}
