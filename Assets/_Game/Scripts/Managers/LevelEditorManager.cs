using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorManager : Singleton<LevelEditorManager>
{
    [SerializeField]
    private DataList levelObjectsList;
    public DataList LevelObjectsList { get { return levelObjectsList; } }

    public void CreateLevelObject(LevelObjectData levelObject, Cursor cursor, LevelEditorPanel editorPanel)
    {
        if (ServerManager.Instance.IsOnlineMatch)
        {
            NetworkManager.Instance.RoomPlayer.CmdSpawnObject(NetworkManager.Instance.GetPrefabID(levelObject.Prefab.gameObject), NetworkClient.connection as NetworkConnectionToClient);
        }
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
}
