#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor helper: adds animation events to selected AnimationClips in the Project window.
/// - If clip name contains "special" uses OnSpecialAttackHit, otherwise OnNormalAttackHit
/// - Adds an event at clip.length * 0.5f if not already present
/// </summary>
public static class AttackAnimationEventAdder
{
    [MenuItem("Tools/Combat/Add Attack Events to Selected Clips")]
    public static void AddEventsToSelected()
    {
        var objs = Selection.objects;
        if (objs == null || objs.Length == 0)
        {
            Debug.LogWarning("No assets selected. Select AnimationClips in the Project window.");
            return;
        }

        int added = 0;
        foreach (var obj in objs)
        {
            var clip = obj as AnimationClip;
            if (clip == null) continue;

            string func = clip.name.ToLower().Contains("special") ? "OnSpecialAttackHit" : "OnNormalAttackHit";
            float time = Mathf.Clamp(clip.length * 0.5f, 0f, clip.length);

            var events = AnimationUtility.GetAnimationEvents(clip).ToList();
            bool exists = events.Any(e => e.functionName == func);
            if (!exists)
            {
                AnimationEvent ev = new AnimationEvent();
                ev.functionName = func;
                ev.time = time;
                events.Add(ev);
                AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                Debug.Log($"Added animation event '{func}' at {time:F2}s to clip {clip.name}");
                added++;
            }
            else
            {
                Debug.Log($"Clip {clip.name} already contains event '{func}'");
            }
        }

        if (added > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"AttackAnimationEventAdder: processed {objs.Length} assets, added {added} events.");
    }
}
#endif