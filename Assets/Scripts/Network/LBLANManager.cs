using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System;

namespace LB.Network
{
    public class LBLANManager : MonoBehaviour
    {
        [SerializeField] private Canvas LANCanvas;
        [SerializeField] private TMP_InputField inputField;
        private LBRelay relay;
        public void StartHost()
        {
            relay.CreateRelay();
            Destroy(LANCanvas);
        }

        public void StartClient()
        {
            if (inputField.Equals(null)) Debug.LogError("Input Field must not be empty!");
            else
            {
                relay.JoinRelay();
                Destroy(LANCanvas);
            }
        }
    }
}
