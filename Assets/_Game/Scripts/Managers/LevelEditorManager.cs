using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorManager : Singleton<LevelEditorManager>
{
    [SerializeField]
    private int roundedGridSize = 5;
    public int RoundedGridSize { get { return roundedGridSize; } }

    [SerializeField]
    private DataList levelObjectsList;
    public DataList LevelObjectsList { get { return levelObjectsList; } }

    [Space()]
    [SerializeField]
    private GameObject spawnParticle;

    private List<LevelObject> levelObjects = new List<LevelObject>();
    private List<LevelObject> recentlyPlacedObjects = new List<LevelObject>();

    public void CreateLevelObject(LevelObjectData levelObject, Cursor cursor, LevelEditorPanel editorPanel)
    {
        if (ServerManager.Instance.IsOnlineMatch)
            NetworkManager.Instance.RoomPlayer.CmdSpawnObject(NetworkManager.Instance.GetPrefabID(levelObject.Prefab.gameObject), NetworkClient.connection as NetworkConnectionToClient);
        else
        {
            LevelObject createdObject = Instantiate(levelObject.Prefab);
            createdObject.Configure(levelObject, cursor, editorPanel);
        }
    }

    public LevelObjectData GetDataFromPrefab(GameObject prefab)
    {
        foreach(var data in levelObjectsList.Items)
        {
            LevelObjectData objData = data as LevelObjectData;
            if (prefab.name.Contains(objData.Prefab.name))
            {
                return objData;
            }
        }

        return null;
    }

    public void AddLevelObject(LevelObject levelObject)
    {
        levelObjects.Add(levelObject);

        if (!recentlyPlacedObjects.Contains(levelObject))
            recentlyPlacedObjects.Add(levelObject);
    }

    public void RevealRecentObjects()
    {
        //hide all objects first
        foreach (var recentObj in recentlyPlacedObjects)
        {
            recentObj.gameObject.SetActive(false);
        }

        StartCoroutine(RevealObjects());
    }

    private IEnumerator RevealObjects()
    {
        float incrementTime = 1.5f / recentlyPlacedObjects.Count;
        for (int i = 0; i < recentlyPlacedObjects.Count; i++)
        {
            GameObject particle = ObjectPooler.GetPooledObject(spawnParticle);
            Vector3 pos = recentlyPlacedObjects[i].transform.position;
            particle.transform.position = new Vector3(pos.x, pos.y, particle.transform.position.z);

            recentlyPlacedObjects[i].gameObject.SetActive(true);

            yield return new WaitForSeconds(incrementTime);
        }

        recentlyPlacedObjects.Clear();
    }
}
