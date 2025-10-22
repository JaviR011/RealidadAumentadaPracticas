using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ArmarYControlar : MonoBehaviour
{
    [Header("Modelos y sus ImageTargets de origen (mismo orden)")]
    public GameObject[] models;
    public ObserverBehaviour[] homeTargets;

    [Tooltip("Índice en 'models' del modelo1 (el pivote de unión).")]
    public int model1Index = 0;

    [Header("Velocidades")]
    public float assembleSpeed = 1.0f;  // Lerp al unir
    public float moveSpeed = 1.0f;      // Lerp al mover al siguiente target

    [Header("Botones UI")]
    public Button assembleButton;  // +
    public Button moveButton;      // →
    public Button resetButton;     // ↻

    // --- Estado interno ---
    class PoseData
    {
        public Transform originalParent;
        public Vector3 localPos;
        public Quaternion localRot;
        public Vector3 localScale;
        public bool assembled = false;   // si ya se unió al pivote
    }

    private PoseData[] originals;        // Pose original por modelo
    private bool isMoving = false;       // para bloquear acciones concurrentes
    private ObserverBehaviour currentPivotTarget;  // target donde está el conjunto unido ahora

    void Awake()
    {
        // Sanidad básica
        if (models == null || homeTargets == null || models.Length != homeTargets.Length)
        {
            Debug.LogError("Configura 'models' y 'homeTargets' con el mismo tamaño y orden.");
            enabled = false;
            return;
        }

        // Guardar poses originales
        originals = new PoseData[models.Length];
        for (int i = 0; i < models.Length; i++)
        {
            originals[i] = new PoseData
            {
                originalParent = models[i].transform.parent,
                localPos = models[i].transform.localPosition,
                localRot = models[i].transform.localRotation,
                localScale = models[i].transform.localScale,
                assembled = false
            };
        }

        // Suscribirnos a cambios de tracking para recalcular habilitación del botón →
        for (int i = 0; i < homeTargets.Length; i++)
        {
            if (homeTargets[i] != null)
                homeTargets[i].OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    void Start()
    {
        // Estado inicial: no unidos, mover deshabilitado
        UpdateMoveButtonState();
    }

    void OnDestroy()
    {
        // Limpieza de eventos
        for (int i = 0; i < homeTargets.Length; i++)
        {
            if (homeTargets[i] != null)
                homeTargets[i].OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    // ==== BOTÓN "+" ====
    public void UnirModelos()
    {
        if (isMoving) return;

        // 1) Elegir pivote: el ImageTarget ACTUAL del modelo1 si está visible;
        // si no, el primer target visible encontrado.
        ObserverBehaviour pivot = GetCurrentTargetOfModel(model1Index);
        if (!IsTracked(pivot))
            pivot = GetFirstTrackedTarget();

        if (pivot == null)
        {
            Debug.Log("No hay ImageTarget visible para unir.");
            return;
        }

        StartCoroutine(Co_AssembleToPivot(pivot));
    }

    IEnumerator Co_AssembleToPivot(ObserverBehaviour pivot)
    {
        isMoving = true;

        // Posiciones iniciales de todos
        Vector3[] startPos = new Vector3[models.Length];
        for (int i = 0; i < models.Length; i++)
            startPos[i] = models[i].transform.position;

        Vector3 end = pivot.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * assembleSpeed;

            // Mover SOLO los modelos cuyas tarjetas estén visibles (los presentes en escena)
            for (int i = 0; i < models.Length; i++)
            {
                if (IsTracked(homeTargets[i])) // aparece en escena
                {
                    models[i].transform.position = Vector3.Lerp(startPos[i], end, t);
                }
            }
            yield return null;
        }

        // Al terminar, hacerlos hijos del pivote (manteniendo la pose resultante)
        for (int i = 0; i < models.Length; i++)
        {
            if (IsTracked(homeTargets[i]))
            {
                models[i].transform.SetParent(pivot.transform, true);
                originals[i].assembled = true;
            }
        }

        currentPivotTarget = pivot;
        isMoving = false;

        // Recalcular habilitación del botón →
        UpdateMoveButtonState();
    }

    // ==== BOTÓN "→" ====
    public void MoverAlSiguienteTarget()
    {
        if (isMoving) return;
        if (!AllVisibleModelsAssembled())
        {
            Debug.Log("Aún hay modelos visibles sin unir. No se puede mover.");
            return;
        }

        // Buscar siguiente target visible distinto del actual
        ObserverBehaviour next = GetNextTrackedTargetAfter(currentPivotTarget);
        if (next == null || next == currentPivotTarget)
        {
            Debug.Log("No hay siguiente ImageTarget visible para mover.");
            return;
        }

        StartCoroutine(Co_MoveEnsembleTo(next));
    }

    IEnumerator Co_MoveEnsembleTo(ObserverBehaviour nextTarget)
    {
        isMoving = true;

        // Guardar posiciones iniciales
        Vector3[] startPos = new Vector3[models.Length];
        for (int i = 0; i < models.Length; i++)
            startPos[i] = models[i].transform.position;

        // Todos se moverán hacia la posición del siguiente target (misma meta para el conjunto)
        Vector3 end = nextTarget.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            for (int i = 0; i < models.Length; i++)
            {
                if (originals[i].assembled) // solo los que ya están unidos
                {
                    models[i].transform.position = Vector3.Lerp(startPos[i], end, t);
                }
            }
            yield return null;
        }

        // Re-parent a nuevo target
        for (int i = 0; i < models.Length; i++)
        {
            if (originals[i].assembled)
                models[i].transform.SetParent(nextTarget.transform, true);
        }

        currentPivotTarget = nextTarget;
        isMoving = false;

        UpdateMoveButtonState();
    }

    // ==== BOTÓN "↻" ====
    public void Reiniciar()
    {
        if (isMoving) return;

        // Regresar cada modelo a su ImageTarget de origen y pose original
        for (int i = 0; i < models.Length; i++)
        {
            var tr = models[i].transform;
            tr.SetParent(originals[i].originalParent, true);

            tr.localPosition = originals[i].localPos;
            tr.localRotation = originals[i].localRot;
            tr.localScale = originals[i].localScale;

            originals[i].assembled = false;
        }

        currentPivotTarget = null;
        UpdateMoveButtonState();
    }

    // ====== LÓGICA DE HABILITACIÓN DEL BOTÓN "→" ======

    void OnTargetStatusChanged(ObserverBehaviour target, TargetStatus status)
    {
        // Cada vez que aparece/desaparece un target, reevaluamos.
        UpdateMoveButtonState();
    }

    void UpdateMoveButtonState()
    {
        bool allReady = AllVisibleModelsAssembled();

        // si no están todos unidos, el botón → se deshabilita
        if (moveButton != null)
            moveButton.interactable = allReady;

        // (opcional) también puedes ocultarlo si prefieres:
        // if (moveButton != null) moveButton.gameObject.SetActive(allReady);
    }

    bool AllVisibleModelsAssembled()
    {
        // Reglas:
        // - Para cada modelo cuyo ImageTarget esté TRACKEADO/EXTENDED,
        //   debe estar "assembled == true".
        // - Si aparece un nuevo target visible (4to, 5to, ...), y su modelo NO está unido,
        //   debe devolver false para deshabilitar el botón →.
        for (int i = 0; i < models.Length; i++)
        {
            if (IsTracked(homeTargets[i]) && !originals[i].assembled)
                return false;
        }
        // además, al menos 1 unido:
        bool alMenosUno = false;
        for (int i = 0; i < models.Length; i++)
            if (originals[i].assembled) { alMenosUno = true; break; }

        return alMenosUno;
    }

    // ====== UTILIDADES DE VUFORIA ======
    bool IsTracked(ObserverBehaviour ob)
    {
        if (ob == null) return false;
        var s = ob.TargetStatus.Status;
        return s == Status.TRACKED || s == Status.EXTENDED_TRACKED;
    }

    ObserverBehaviour GetCurrentTargetOfModel(int modelIndex)
    {
        if (modelIndex < 0 || modelIndex >= homeTargets.Length) return null;
        return homeTargets[modelIndex];
    }

    ObserverBehaviour GetFirstTrackedTarget()
    {
        for (int i = 0; i < homeTargets.Length; i++)
            if (IsTracked(homeTargets[i])) return homeTargets[i];
        return null;
    }

    ObserverBehaviour GetNextTrackedTargetAfter(ObserverBehaviour current)
    {
        if (homeTargets.Length == 0) return null;
        int start = 0;
        if (current != null)
        {
            for (int i = 0; i < homeTargets.Length; i++)
                if (homeTargets[i] == current) { start = i; break; }
        }

        // buscar el siguiente visible (circular)
        for (int k = 1; k <= homeTargets.Length; k++)
        {
            int idx = (start + k) % homeTargets.Length;
            if (IsTracked(homeTargets[idx])) return homeTargets[idx];
        }
        return current;
    }
}
