using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

public class PlayFabLogin : PersistentSingleton<PlayFabLogin>
{
    public string ClientsCustomID { get; private set; }
    public string EntityID { get; private set; }
    public string EntityType { get; private set; }

    private const string customIdKey = "client_custom_id";

    public void Start()
    {
        if (ServerManager.Instance.IsServer)
            return;

        ClientsCustomID = PlayerPrefs.GetString(customIdKey, "EmptyID");

        var request = new LoginWithCustomIDRequest { CreateAccount = true, CustomId = ClientsCustomID };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        GetAccountInfoRequest request = new GetAccountInfoRequest() { PlayFabId = result.PlayFabId };
        PlayFabClientAPI.GetAccountInfo(request, GetAccountInfo, null);

        EntityID = result.EntityToken.Entity.Id;
        EntityType = result.EntityToken.Entity.Type;
    }

    private void GetAccountInfo(GetAccountInfoResult obj)
    {
        PlayerPrefs.SetString(customIdKey, obj.AccountInfo.CustomIdInfo.CustomId);
        PlayerPrefs.Save();

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