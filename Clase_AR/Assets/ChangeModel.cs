using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeModel : MonoBehaviour
{
    public GameObject gashtM;
    public GameObject MapaM;
    public GameObject EndermanM;
    public GameObject SnowGolemM;
    public GameObject CreeperM;

    public int actual = 1;
    private int siguiente;
    private GameObject[] modelos;
    public float speed = 2.0f; // velocidad de la animación
    private Coroutine running;
    // Start is called before the first frame update
    void Start()
    {
        
        Vector3 ceros = new Vector3(0, 0, 0);
        MapaM.transform.localScale = ceros;
        CreeperM.transform.localScale = ceros;
        EndermanM.transform.localScale = ceros;
        SnowGolemM.transform.localScale = ceros;
        actual = 1;

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeM_BTN()
    {
        if (running != null) StopCoroutine(running);
        siguiente = Random.Range(1, 6);
        running = StartCoroutine(CambiarModelo());
    }

    private IEnumerator CambiarModelo()
    {
        GameObject modeloActual = GetModel(actual);
        GameObject modeloSiguiente = GetModel(siguiente);

        Vector3 original = modeloActual.transform.localScale;
        Vector3 target = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            modeloActual.transform.localScale = Vector3.Lerp(original, target, t);
            yield return null; // esperar al siguiente frame
        }

        modelos = new GameObject[] { gashtM, MapaM, CreeperM, SnowGolemM, EndermanM };

        for (int i = 0; i < 5; i++)
        {
            GameObject model = modelos[i];
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * speed * 3;
                model.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }
            
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * speed * 3;
                model.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                yield return null; // esperar al siguiente frame
            }
            yield return null;
        }

        
    


    modeloSiguiente.transform.localScale = Vector3.zero;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            modeloSiguiente.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        actual = siguiente;
        running = null; // marcamos que terminó
    }
    
        
    private GameObject GetModel(int idx)
    {
        switch (idx)
        {
            case 1: return gashtM;
            case 2: return MapaM;
            case 3: return CreeperM;
            case 4: return SnowGolemM;
            case 5: return EndermanM;
            default: return gashtM;
        }
    }
}

