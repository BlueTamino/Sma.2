using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using System.Threading.Tasks;

public class Relay : MonoBehaviour
{
    public TMP_InputField joinCodeInputField;
    public TMP_Text Code;
    public async void CreateRelay_P()
    {
        await CreateRelay();
    }
    public async Task<string> CreateRelay()
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                return null;
            }
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(40);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            Code.text = joinCode;
            return joinCode;
        }catch(RelayServiceException e)
        {

            Debug.LogError(e);
            return null;
        }
    }
    public async void joinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
        
    }
    public void joinRelay_Ui()
    {
        joinRelay(joinCodeInputField.text);
    }
}
