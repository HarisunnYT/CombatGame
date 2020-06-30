using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ErrorManager : PersistentSingleton<ErrorManager>
{
    [SerializeField]
    private ErrorData[] errors;

    private ErrorData unshownError;

    #region ERROR_CODES

    public const string HostDisconnectedCode = "101";
    public const string UserDisconnectedCode = "102";

    #endregion

    private void Start()
    {
        SceneManager.activeSceneChanged += ActiveSceneChanged;
    }

    private void ActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (unshownError != null && arg1.name.Contains("MainMenu"))
        {
            PanelManager.Instance.GetPanel<ErrorPanel>().ShowPanel(unshownError);
            unshownError = null;
        }
    }

    public void EncounteredError(string errorCode)
    {
        unshownError = GetErrorData(errorCode);
    }

    public void DisconnectedError()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            EncounteredError(UserDisconnectedCode);
        else
            EncounteredError(HostDisconnectedCode);
    }

    private ErrorData GetErrorData(string errorCode)
    {
        foreach(var error in errors)
        {
            if (error.ErrorCode == errorCode)
            {
                return error;
            }
        }

        throw new System.Exception("Error, error doesn't exist... ");
    }
}
