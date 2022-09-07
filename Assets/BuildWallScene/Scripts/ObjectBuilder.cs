using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectBuilder : MonoBehaviour
{
    public List<ConstructableObject> constructableObjects;

    public enum ObjectType
    {
        Window = 0,
        Door = 1,
        Column = 2
    }

    public ObjectType objectType;

    bool canConstructObject;

    public bool isDragging;
    public bool canDrag;
    public bool dragCompleted;

    Vector3 dragStartPoint;
    Vector3 dragEndPoint;

    GameObject column;

    public float windowHeight;
    public bool canPlaceWindow;
    public Material highlightedMat;
    public Material completedMat;
    Material currentMaterial;

    public bool windowChoosen;
    public bool windowPlaced;

    public GameObject currentObject;
    string currentObjectName;
    public bool canPlaceObject;
    public RaycastHit[] hits;

    public GameObject currentWall;

    private void Update()
    {
        canConstructObject = Builder.instance.roomBuilded;

        if (canConstructObject)
        {
            if(currentObject != null)
            {
                switch(currentObjectName)
                {
                    case "Window":
                        ConstructWindow();
                        break;
                    case "Door":
                        ConstructDoor();
                        break;
                    case "Column":
                        ConstructColumn();
                        break;
                }
                if (IsInRoom())
                {
                    SnapToWall();
                }


                GetInput();
            }
        }
    }

    private void ConstructColumn()
    {
        throw new NotImplementedException();
    }

    private void ConstructDoor()
    {
        currentObject.transform.position = new Vector3(GetWorldPointFromInputWithRotation(currentObject).x, windowHeight, GetWorldPointFromInputWithRotation(currentObject).z);
    }

    private void ConstructWindow()
    {
        currentObject.transform.position = new Vector3(GetWorldPointFromInputWithRotation(currentObject).x, windowHeight, GetWorldPointFromInputWithRotation(currentObject).z);       
    }

    public void ChooseItem(Button _btn)
    {
        string buttonName = _btn.gameObject.name;

        currentObjectName = buttonName;
        currentObject = Instantiate(constructableObjects.Find(x => x.name == buttonName).constructableObject, GetWorldPointFromInput(), Quaternion.identity);
    }

    public Collider GetTheFrontCollider()
    {
        Ray ray = new Ray(currentObject.transform.position, currentObject.transform.right);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
        {
            return currentObject.GetComponent<Collider>();
        }

        return hit.collider;
    }

    public float GetDistanceFromLeft()
    {
        if (currentWall == null) return 0;

        return Vector3.Distance(currentObject.transform.position, Builder.instance.walls.Find(x => x.wall == currentWall).wallEndPoint);
    }

    public float GetDistanceFromRight()
    {
        if (currentWall == null) return 0;

        return Vector3.Distance(currentObject.transform.position, Builder.instance.walls.Find(x => x.wall == currentWall).wallStartPoint);
    }

    public void PlaceObject()
    {




        currentObject = null; 
        canPlaceObject = false;
    }

    public bool IsInRoom()
    {
        RaycastHit leftHit, rightHit,frontHit,backHit;

        if(Physics.Raycast(currentObject.transform.position, -currentObject.transform.right, out leftHit) &&
           Physics.Raycast(currentObject.transform.position, currentObject.transform.right, out rightHit) &&
           Physics.Raycast(currentObject.transform.position, currentObject.transform.forward, out frontHit) &&
           Physics.Raycast(currentObject.transform.position, -currentObject.transform.forward, out backHit))
        {
            return true;
        }

        return false;
    }

    public void SnapToWall()
    {
        float distance = Vector3.Distance(GetTheFrontCollider().ClosestPoint(currentObject.transform.position), currentObject.transform.position);
        if(distance <= 1)
        {
            //if(GetDistanceFromLeft() >= currentObject.transform.localScale.z/2 + currentWall.transform.localScale.x && 
            //   GetDistanceFromRight() >= currentObject.transform.localScale.z/ 2 + currentWall.transform.localScale.x)
            //{
            currentObject.transform.position = GetTheFrontCollider().ClosestPoint(currentObject.transform.position);

            currentObject.transform.rotation = GetTheFrontCollider().gameObject.transform.rotation;

            canPlaceObject = true;

            Debug.Log(GetDistanceFromRight() + " <- Right");
            Debug.Log(GetDistanceFromLeft() + " <- Left");
            //}

            //else
            //{
            //    canPlaceObject = false;
            //}
        }
        else { canPlaceObject = false; }
    }
    public void GetInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(currentObject);
            currentObjectName = null;
            currentObject = null;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (canPlaceObject) { PlaceObject(); }
        }
    }

    Vector3 GetWorldPointFromInputWithRotation(GameObject objToSnap)//Gets the world point from the mouse cursor
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if(!hit.collider.name.Contains("Ground"))
                objToSnap.transform.rotation = hit.collider.gameObject.transform.rotation;
            return (hit.collider.ClosestPoint(hit.point));
        }

        return hit.point;
    }

    Vector3 GetWorldPointFromInput()//Gets the world point from the mouse cursor
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag == "Ground") 
                return (hit.point);
        }

        return (hit.point);
    }

    private void OnDrawGizmos()
    {
        if (currentWall == null) return;

        Gizmos.DrawLine(currentObject.transform.position, Builder.instance.walls.Find(x => x.wall == currentWall).wallEndPoint);
    }
}

[System.Serializable]
public class ConstructableObject
{
    public string name;
    public GameObject constructableObject;
}