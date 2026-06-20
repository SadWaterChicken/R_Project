using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent<string> OnEventTriggered;

    // Hàm này sẽ được gán trong Animation tab của Unity
    public void TriggerEvent(string eventName)
    {
        OnEventTriggered?.Invoke(eventName);
    }
}
