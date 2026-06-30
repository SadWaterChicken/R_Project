using System.Collections;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField] private float typeSpeed = 50f;
    public Coroutine Run(string textToType, TMP_Text textLabel)
    {
        return StartCoroutine(TypeText(textToType, textLabel)); // returning the Courotine instead of nothing so can't be void
    }
    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        textLabel.text = string.Empty;//clear leftover text by the TMP

        float t = 0;
        int charIndex = 0;

        while (charIndex < textToType.Length) 
        {
            t += Time.deltaTime * typeSpeed;
            charIndex = Mathf.FloorToInt(t);//Store floor value 
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);//ensure lower than TextToType

            textLabel.text = textToType.Substring(0, charIndex);//Display the substring of the text

            yield return null;//wait 1 frame
        }

        textLabel.text = textToType;
    }
}
