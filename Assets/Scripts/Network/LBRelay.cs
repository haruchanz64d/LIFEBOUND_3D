using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

namespace LB.Network
{
    public class LBRelay : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Canvas lanCanvas;
        private string joinCode;

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async void CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.Log(joinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                    );

                NetworkManager.Singleton.StartHost();
                lanCanvas.enabled = false;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void JoinRelay()
        {
            try
            {
                JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(inputField.text);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    join.RelayServer.IpV4,
                    (ushort)join.RelayServer.Port,
                    join.AllocationIdBytes,
                    join.Key,
                    join.ConnectionData,
                    join.HostConnectionData
                    );
                NetworkManager.Singleton.StartClient();
                lanCanvas.enabled = false;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
