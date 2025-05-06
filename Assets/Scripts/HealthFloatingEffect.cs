using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeDuration = 1f;
    private TextMeshProUGUI text;
    private Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
        StartCoroutine(FadeOutAndMove());
    }

    IEnumerator FadeOutAndMove()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration); // Fade out
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            transform.position = startPos + new Vector3(0, moveSpeed * elapsed, 0); // Move upwards
            yield return null;
        }

        Destroy(gameObject);
    }
}