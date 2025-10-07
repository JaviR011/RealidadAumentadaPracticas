using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Vuforia;

public class Move : MonoBehaviour
{
    public GameObject model;
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public float speed2 = 0.02f;
    private bool isMoving = false;
    private int targetnum = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public void moveToNexMarker()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveModel());
        }
    }

    private IEnumerator MoveModel()
    {
        isMoving = true;
        ObserverBehaviour target = GetNextDetectedTarget();

        if (target == null)
        {
            isMoving = false;
            yield break;
        }

        Vector3 starPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;
        float journey = 0;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(starPosition, endPosition, journey);
            yield return null;
        }

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false;
    }

    private ObserverBehaviour GetNextDetectedTarget()
    {
        targetnum++;
        if (targetnum > 3)
        {
            targetnum = 0;
        }
        switch (targetnum){

            case 0:
                if (ImageTargets[0] != null && (ImageTargets[0].TargetStatus.Status == Status.TRACKED || ImageTargets[0].TargetStatus.Status == Status.EXTENDED_TRACKED))
                {
                    return ImageTargets[0];
                    
                }
                goto case 1;
            case 1:
                if (ImageTargets[1] != null && (ImageTargets[1].TargetStatus.Status == Status.TRACKED || ImageTargets[1].TargetStatus.Status == Status.EXTENDED_TRACKED))
                {
                    return ImageTargets[1];
                   
                }
                goto case 2;
            case 2:
                if (ImageTargets[2] != null && (ImageTargets[2].TargetStatus.Status == Status.TRACKED || ImageTargets[2].TargetStatus.Status == Status.EXTENDED_TRACKED))
                {
                    return ImageTargets[2];
                   
                }
                goto case 3;
            case 3:
                if (ImageTargets[3] != null && (ImageTargets[3].TargetStatus.Status == Status.TRACKED || ImageTargets[3].TargetStatus.Status == Status.EXTENDED_TRACKED))
                {
                    return ImageTargets[3];

                }
                goto case 0;

        }
        return null;


    }
}