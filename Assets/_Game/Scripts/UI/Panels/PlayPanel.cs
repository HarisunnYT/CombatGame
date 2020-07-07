using InControl;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayPanel : Panel
{
    [System.Serializable]
    struct MatchType
    {
        public GameObject Parent;
        public Animator[] characterAnimators;
        public Image[] characterImages;
        public TMP_Text Title;
        public GameObject SpotLight;
    }

    [SerializeField]
    private MatchType online;

    [SerializeField]
    private MatchType local;

    protected override void OnShow()
    {
        OnlineSelected();
    }

    public void Online()
    {
        SteamLobbyManager.Instance.CreatePrivateLobby();
        ServerManager.Instance.IsOnlineMatch = true;
    }

    public void Local()
    {
        ServerManager.Instance.IsOnlineMatch = false;
        SceneLoader.Instance.LoadScene("Lobby");
    }

    public void OnlineSelected()
    {
        SetSelected(online, true);
        SetSelected(local, false);
    }

    public void LocalSelected()
    {
        SetSelected(online, false);
        SetSelected(local, true);
    }

    private void SetSelected(MatchType matchType, bool selected)
    {
        for (int i = 0; i < matchType.characterAnimators.Length; i++)
        {
            if (selected)
                matchType.characterAnimators[i].StopPlayback(); //for some reason the animators work when set like this, weird
            else
                matchType.characterAnimators[i].StartPlayback();

            matchType.characterImages[i].color = selected ? Color.white : Color.grey;
        }

        matchType.Title.color = selected ? Color.white : Color.grey;
        matchType.SpotLight.SetActive(selected);

        matchType.Parent.transform.DOScale(selected ? 1.2f : 1.0f, 0.2f);
    }
}
