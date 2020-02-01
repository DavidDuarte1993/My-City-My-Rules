using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

namespace UnderdogCity
{
    public class NetworkConnectionManager : MonoBehaviourPunCallbacks
    {

        public Button BtnConnectMaster;
        public Button BtnConnectRoom;

        protected bool TriesToConnectToMaster;
        protected bool TriesToConnectToRoom;

        void Start()
        {
            DontDestroyOnLoad(this);
            TriesToConnectToMaster = false;
            TriesToConnectToRoom   = false;
        }

        void Update()
        {
                if (BtnConnectMaster != null)
                    BtnConnectMaster.gameObject.SetActive(!PhotonNetwork.IsConnected && !TriesToConnectToMaster);
                if (BtnConnectRoom != null)
                    BtnConnectRoom.gameObject.SetActive(PhotonNetwork.IsConnected && !TriesToConnectToMaster && !TriesToConnectToRoom);

        }

        public void OnClickConnectToMaster()
        {
            TriesToConnectToMaster = true;

            PhotonNetwork.OfflineMode = false;          
            PhotonNetwork.NickName = "PlayerName";       
            PhotonNetwork.AutomaticallySyncScene = true; 
            PhotonNetwork.GameVersion = "v1";            

            if(!PhotonNetwork.OfflineMode)
                PhotonNetwork.ConnectUsingSettings();         

        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            TriesToConnectToMaster = false;
            Debug.Log("Connected to Master!");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            TriesToConnectToMaster = false;
            TriesToConnectToRoom   = false;
            Debug.Log(cause);
        }

        public void OnClickConnectToRoom()
        {
            if (!PhotonNetwork.IsConnected)
                return;

            TriesToConnectToRoom = true;
            PhotonNetwork.JoinRandomRoom();              
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 });
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log(message);
            base.OnCreateRoomFailed(returnCode, message);
            TriesToConnectToRoom = false;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            TriesToConnectToRoom = false;

            
            Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players In Room: " + PhotonNetwork.CurrentRoom.PlayerCount + " | RoomName: " + PhotonNetwork.CurrentRoom.Name + " Region: " + PhotonNetwork.CloudRegion);
            if(PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name != "Main")
                PhotonNetwork.LoadLevel("Main");
        }
    }
}
