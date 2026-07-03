using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField] private float typeSpeed = 50f;

    private readonly Dictionary<HashSet<char>, float> punctuations = new Dictionary<HashSet<char>, float>()
    {
        { new HashSet<char>(){ '.', '!', '?' }, 0.6f}, //key, time
        { new HashSet<char>(){ ',', ';', ':' }, 0.3f},
    };
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
            int lastCharIndex = charIndex;

            t += Time.deltaTime * typeSpeed;
            charIndex = Mathf.FloorToInt(t);//Store floor value 
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);//ensure lower than TextToType

            for(int i = lastCharIndex; i < charIndex; i++)// loop through character that have been typed since last frame so if the typpe speed is very fast or device is lagging and you might need to type more than 1 character in a single frame help keeping frame rate consitaincy
            {
                bool isLast = i >= textToType.Length - 1;

                textLabel.text = textToType.Substring(0, i + 1);//Display the substring of the text

                //check if we are at the very end of string s,o we dont wanna pause
                if (IsPunctuation(textToType[i], out float  waitTime) && !isLast && !IsPunctuation(textToType[i + 1], out _))//check if currently have punctuation and capture waittime, then check if is the last and check if character is not a punctuatioin
                {
                    yield return new WaitForSeconds(waitTime);//wait for the time defined above
                }

            }

            yield return null;//wait 1 frame
        }

        textLabel.text = textToType;
    }

    private bool IsPunctuation(char character, out float waitTime)
    {
        foreach (KeyValuePair<HashSet<char>, float> punctuationCategory in punctuations)
        {
            if (punctuationCategory.Key.Contains(character))//check if the key have the character set above
            {
                waitTime = punctuationCategory.Value;
                return true;
            }
        }
        waitTime = default; // set to default to not skip the normal dialog speed instead of 0f
        return false;
    }
}
