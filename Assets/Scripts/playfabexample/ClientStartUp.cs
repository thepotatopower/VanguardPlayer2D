using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using System;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using TMPro;
using Mirror;
using kcp2k;

public class ClientStartUp : MonoBehaviour
{
	public Configuration configuration;
	public ServerStartUp serverStartUp;
	public NetworkManager networkManager;
	public KcpTransport telepathyTransport;

	public static string SessionTicket;
	public static string EntityId;
	private string ticketId;

	private Coroutine pollTicketCoroutine;

	public void OnLoginUserButtonClick()
	{
		if (configuration.buildType == BuildType.REMOTE_CLIENT)
		{
			if (configuration.buildId == "")
			{
				throw new Exception("A remote client build must have a buildId. Add it to the Configuration. Get this from your Multiplayer Game Manager in the PlayFab web console.");
			}
			else
			{
				LoginRemoteUser();
			}
		}
		else if (configuration.buildType == BuildType.LOCAL_CLIENT)
		{
			networkManager.StartClient();
		}
	}

	public void LoginRemoteUser()
	{
		Debug.Log("[ClientStartUp].LoginRemoteUser");
		
		//We need to login a user to get at PlayFab API's. 
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			TitleId = PlayFabSettings.TitleId,
			CreateAccount = true,
			CustomId = GUIDUtility.getUniqueID()
		};

		PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnLoginError);
	}

	private void OnLoginError(PlayFabError response)
	{
		Debug.Log(response.ToString());
	}

	private void OnPlayFabLoginSuccess(LoginResult response)
	{
		Debug.Log(response.ToString());
		SessionTicket = response.SessionTicket;
		EntityId = response.EntityToken.Entity.Id;
		Debug.Log(EntityId);

		if (configuration.ipAddress == "")
		{   //We need to grab an IP and Port from a server based on the buildId. Copy this and add it to your Configuration.
			RequestMultiplayerServer();
			//StartMatchmaking();
		}
		else
		{
			ConnectRemoteClient();
		}
	}

	public void StartMatchmaking()
    {
		PlayFabMultiplayerAPI.CreateMatchmakingTicket(
			new CreateMatchmakingTicketRequest
			{
				Creator = new MatchmakingPlayer
				{
					Entity = new PlayFab.MultiplayerModels.EntityKey
					{
						Id = EntityId,
						Type = "title_player_account"
					},
					Attributes = new MatchmakingPlayerAttributes
					{
						DataObject = new
						{
                            Latencies = new object[]
                            {
                                new
                                {
                                    region = "EastUs",
                                    latency = 50
                                }
                            }
                        }
					}
				},

				GiveUpAfterSeconds = 60,

				QueueName = "DefaultQueue"
			},
			OnMatchmakingTicketCreated,
			OnMatchmakingError
		) ; 
    }

	private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
    {
		ticketId = result.TicketId;
		Debug.Log("Ticket created: " + result.TicketId);
		//GetQueueStatistics();

		pollTicketCoroutine = StartCoroutine(PollTicket(result.TicketId));
    }

	private void OnMatchmakingError(PlayFabError error)
    {
		Debug.LogError(error.GenerateErrorReport());
    }

	private IEnumerator PollTicket(string ticketId)
    {
		while (true)
        {
			PlayFabMultiplayerAPI.GetMatchmakingTicket(
				new GetMatchmakingTicketRequest
				{
					TicketId = ticketId,
					QueueName = "DefaultQueue"
				},
				OnGetMatchmakingTicket,
				OnMatchmakingError
			);

			yield return new WaitForSeconds(6);
        }
    }

	public void GetQueueStatistics()
    {
		PlayFabMultiplayerAPI.GetQueueStatistics(
			new GetQueueStatisticsRequest
			{
				QueueName = "DefaultQueue"
			},
			OnGetQueueStatisticsResult,
			OnErrorCallBack
		);
	}

	private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
    {
		switch(result.Status)
        {
			case "Matched":
				StopCoroutine(pollTicketCoroutine);
				StartMatch(result.MatchId);
				break;
			case "Canceled":
				StopCoroutine(pollTicketCoroutine);
				Debug.Log("Canceled");
				break;
        }
    }

	private void OnGetQueueStatisticsResult(GetQueueStatisticsResult result)
    {
		Debug.Log("Number of players matching: " + result.NumberOfPlayersMatching);
    }

	private void OnErrorCallBack(PlayFabError error)
    {
		Debug.Log("queue statistics error: " + error.GenerateErrorReport());
    }

	private void StartMatch(string matchId)
    {
		Debug.Log("Starting match");
		PlayFabMultiplayerAPI.GetMatch(
			new GetMatchRequest
			{
				MatchId = matchId,
				QueueName = "DefaultQueue"
			},
			OnGetMatch,
			OnMatchmakingError
		);
    }

	private void OnGetMatch(GetMatchResult result)
    {
		Debug.Log(result.Members[0].Entity.Id + " vs " + result.Members[1].Entity.Id);
		configuration.ipAddress = result.ServerDetails.IPV4Address;
		configuration.port = (ushort)result.ServerDetails.Ports[0].Num;
		ConnectRemoteClient(null);
    }

	private void RequestMultiplayerServer()
	{
		Debug.Log("[ClientStartUp].RequestMultiplayerServer");
		RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
		requestData.BuildId = configuration.buildId;
		//requestData.SessionId = "c03b9f6a-a392-4e72-82c5-655ed6be4395";
		requestData.SessionId = System.Guid.NewGuid().ToString();
		Debug.Log(requestData.SessionId);
		requestData.PreferredRegions = new List<string>() { "EastUs" };
		PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
	}

	private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
	{
		Debug.Log(response.ToString());
		ConnectRemoteClient(response);
	}

	private void ConnectRemoteClient(RequestMultiplayerServerResponse response = null)
	{
		if(response == null) 
		{
			networkManager.networkAddress = configuration.ipAddress;
			telepathyTransport.Port = configuration.port;
		}
		else
		{
			Debug.Log("**** ADD THIS TO YOUR CONFIGURATION **** -- IP: " + response.IPV4Address + " Port: " + (ushort)response.Ports[0].Num);
			networkManager.networkAddress = response.IPV4Address;
			telepathyTransport.Port = (ushort)response.Ports[0].Num;
		}

		networkManager.StartClient();
	}

	private void OnRequestMultiplayerServerError(PlayFabError error)
	{
		Debug.Log(error.ErrorDetails);
	}
}