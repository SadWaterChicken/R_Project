using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public HashSet<string> completed = new HashSet<string>();
    public bool HasCompleted(string id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        return completed.Contains(id);
    }
    // for debug
    public void Complete(string id) { if (!string.IsNullOrEmpty(id)) completed.Add(id); }
}
