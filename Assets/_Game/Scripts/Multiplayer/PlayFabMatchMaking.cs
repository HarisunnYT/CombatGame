using PlayFab;
using PlayFab.Json;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabMatchMaking : PersistentSingleton<PlayFabMatchMaking>
{
    public string CurrentTickedID { get; private set; }
    public string CurrentMatchID { get; private set; }

    private const int secondsBetweenTicketCheck = 6;
    private const string queueName = "MatchMakingQueue";

    private float previousTicketCheckTimer = -1;

    public void SearchForMatch()
    {
        CancelAllMatchmakingTicketsForPlayerRequest request = new CancelAllMatchmakingTicketsForPlayerRequest()
        {
            QueueName = queueName,
            Entity = new EntityKey
            {
                Id = PlayFabLogin.Instance.EntityID,
                Type = PlayFabLogin.Instance.EntityType
            }
        };

        PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(request, MatchSearch, OnMatchmakingError);
    }

    private void MatchSearch(CancelAllMatchmakingTicketsForPlayerResult obj)
    {
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
        new CreateMatchmakingTicketRequest
        {
            // The ticket creator specifies their own player attributes.
            Creator = new MatchmakingPlayer
            {
                Entity = new EntityKey
                {
                    Id = PlayFabLogin.Instance.EntityID,
                    Type = PlayFabLogin.Instance.EntityType
                },

                // Here we specify the creator's attributes.
                Attributes = new MatchmakingPlayerAttributes
                {
                    DataObject = new
                    {
                        
                    },
                },
            },

            // Cancel matchmaking if a match is not found after 120 seconds.
            GiveUpAfterSeconds = 120,

            // The name of the queue to submit the ticket into.
            QueueName = queueName,
        },

        // Callbacks for handling success and error.
        OnMatchmakingTicketCreated,
        OnMatchmakingError);
    }

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult obj)
    {
        Debug.Log("began match make");

        CurrentTickedID = obj.TicketId;
        previousTicketCheckTimer = Time.time + secondsBetweenTicketCheck;
    }

    private void Update()
    {
        if (previousTicketCheckTimer != -1 && Time.time > previousTicketCheckTimer)
        {
            GetTicketProgress();
        }
    }

    private void GetTicketProgress()
    {
        PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest
        {
            TicketId = CurrentTickedID,
            QueueName = queueName
        },
          this.OnGetMatchmakingTicket,
          this.OnMatchmakingError);

        previousTicketCheckTimer = Time.time + secondsBetweenTicketCheck;
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult obj)
    {
        if (obj.Status == "Matched")
        {
            previousTicketCheckTimer = -1;
            CurrentMatchID = obj.MatchId;

            PlayFabMultiplayerAPI.GetMatch(
                new GetMatchRequest
                {
                    MatchId = CurrentMatchID,
                    QueueName = queueName,
                    ReturnMemberAttributes = true
                },
                OnGetMatch,
                OnMatchmakingError);
        }
    }

    private void CancelMatchMaking()
    {
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(
        new CancelMatchmakingTicketRequest
        {
            QueueName = queueName,
            TicketId = CurrentTickedID,
        },
        OnTicketCanceled,
        OnMatchmakingError);
    }

    private void OnTicketCanceled(CancelMatchmakingTicketResult obj)
    {
        Debug.Log("cancelled match make");
    }

    private void OnGetMatch(GetMatchResult obj)
    {
        //TEMP
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

        Debug.Log("match successful");
    }

    private void OnMatchmakingError(PlayFabError obj)
    {
        previousTicketCheckTimer = -1;

        Debug.LogError("failed to match make");
        Debug.LogError(obj.ErrorMessage);
    }
}
