using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Vuforia;
public class Armar : MonoBehaviour
{
    public GameObject[] models;  // Lista de modelos
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

    public void moveToNextMarker()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveModels());
        }
    }

    private IEnumerator MoveModels()
    {
        isMoving = true;
        ObserverBehaviour target = GetNextDetectedTarget();

        if (target == null)
        {
            isMoving = false;
            yield break;
        }

        // Guardamos las posiciones de inicio de todos los modelos
        Vector3[] startPositions = new Vector3[models.Length];
        for (int i = 0; i < models.Length; i++)
        {
            startPositions[i] = models[i].transform.position;
        }

        Vector3 endPosition = target.transform.position;
        float journey = 0;

        // Movimiento conjunto de los modelos
        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            for (int i = 0; i < models.Length; i++)
            {
                models[i].transform.position = Vector3.Lerp(startPositions[i], endPosition, journey);
            }
            yield return null;
        }

        // Al llegar, hacemos que los modelos se conviertan en hijos del target
        for (int i = 0; i < models.Length; i++)
        {
            models[i].transform.SetParent(target.transform);
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
        switch (targetnum)
        {
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
