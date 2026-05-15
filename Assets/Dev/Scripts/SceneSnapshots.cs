using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntitySnapshot
{
    public string saveId;
    public string prefabKey;
    public float posX;
    public float posY;
    public float posZ;
    public float rotZ;
    public bool isActive = true;
    public int currentHealth = 0;
    public string customJson = ""; // optional custom data
}

[Serializable]
public class SceneSnapshot
{
    public string sceneId;
    public List<EntitySnapshot> entities = new List<EntitySnapshot>();
}
