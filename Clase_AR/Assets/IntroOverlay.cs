using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroOverlay : MonoBehaviour
{
    [Header("Opciones")]
    [Tooltip("Mostrar solo la primera vez que se abre la app")]
    public bool showOnlyFirstTime = false;

    [Tooltip("Cierra solo después de X segundos (0 = desactivado)")]
    public float autoCloseSeconds = 0f;

    [Header("Referencias")]
    public Button closeButton;              // Asigna tu botón “Entendido”
    public CanvasGroup canvasGroup;         // (Opcional) para fade in/out

    const string PREF_KEY = "intro_seen";

    void Awake()
    {
        if (showOnlyFirstTime && PlayerPrefs.GetInt(PREF_KEY, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        // Mostrar overlay
        gameObject.SetActive(true);

        // Fade in opcional
        if (canvasGroup != null) StartCoroutine(Fade(canvasGroup, 0f, 1f, 0.2f));

        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (autoCloseSeconds > 0f) StartCoroutine(AutoCloseAfter(autoCloseSeconds));
    }

    public void Close()
    {
        if (showOnlyFirstTime) PlayerPrefs.SetInt(PREF_KEY, 1);
        if (canvasGroup != null) StartCoroutine(CloseWithFade());
        else gameObject.SetActive(false);
    }

    IEnumerator AutoCloseAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Close();
    }

    IEnumerator CloseWithFade()
    {
        yield return Fade(canvasGroup, 1f, 0f, 0.2f);
        gameObject.SetActive(false);
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to, float dur)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < dur)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        cg.alpha = to;
    }
}
