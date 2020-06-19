using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : PersistentSingleton<PlayFabLogin>
{
    public string ClientsCustomID { get; private set; }
    public string EntityID { get; private set; }
    public string EntityType { get; private set; }

    private const string titleID = "98261";
    private const string customIdKey = "client_custom_id";

    public void Start()
    {
        ClientsCustomID = PlayerPrefs.GetString(customIdKey, "EmptyID");

        PlayFabSettings.TitleId = titleID;
        var request = new LoginWithCustomIDRequest { CreateAccount = true, TitleId = titleID, CustomId = ClientsCustomID };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayerPrefs.SetString(customIdKey, result.PlayFabId);
        PlayerPrefs.Save();

        EntityID = result.EntityToken.Entity.Id;
        EntityType = result.EntityToken.Entity.Type;

        //TODO MOVE
        PlayFabMatchMaking.Instance.SearchForMatch();

        Debug.Log("Login Successful");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login Failed");
        Debug.LogError(error.GenerateErrorReport());
    }
}