using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityObjectBinding : MonoBehaviour
{
    public int entity_id = -1;  //ZZWTODO 为了在编辑器里方便的看到，因此这样命名并且public
    public int EntityID
    {
        get { return entity_id; }
        set { entity_id = value; }
    }
}