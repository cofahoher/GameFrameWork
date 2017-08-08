using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class UnityResourceManager : Singleton<UnityResourceManager>
    {
        Dictionary<string, GameObject> m_loaded_prefab = new Dictionary<string, GameObject>();

        private UnityResourceManager()
        {
        }

        public override void Destruct()
        {
        }

        public GameObject CreateGameObject(string asset_name)
        {
            GameObject prefab;
            if (!m_loaded_prefab.TryGetValue(asset_name, out prefab))
            {
                prefab = Resources.Load(asset_name, typeof(GameObject)) as GameObject;
                prefab.SetActive(false);
                m_loaded_prefab[asset_name] = prefab;
            }
            if (prefab == null)
                return null;
            GameObject go = GameObject.Instantiate(prefab);
            go.SetActive(false);
            Transform tf = go.transform;
            tf.localScale = Vector3.one;
            tf.localPosition = Vector3.zero;
            int index = go.name.IndexOf("(Clone)");
            if (index > 0)
                go.name = go.name.Substring(0, index);
            go.SetActive(true);
            return go;
        }

        public void RecycleGameObject(string asset_name, GameObject go)
        {
            if (go != null)
                GameObject.Destroy(go);
        }
    }
}