using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton<Ty> : Singleton<Ty> where Ty : MonoBehaviour
{
    protected virtual bool NeverReload { get { return false; } }

    protected override void Register()
    {
        if (Instance == null)
        {
            var transform = this.transform;
            var root = transform.root;
            if (transform != root)
            {
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);

        }
        base.Register();
    }

    public void DestroyInstance()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
