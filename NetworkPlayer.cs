using Photon.Pun;
using UnityEngine;

namespace UnderdogCity
{

    public class NetworkPlayer : MonoBehaviourPun, IPunObservable
    {

        protected Player Player;
        protected Vector3 RemotePlayerPosition;
        protected float RemoteLookX;
        protected float RemoteLookZ;
        protected float LookXVel;
        protected float LookZVel;

        private void Awake()
        {
            Player = GetComponent<Player>();

            //自分でキャラクターをコントロールしていない時、Controllerは破壊される
            if (!photonView.IsMine && GetComponent<Controller>() != null)
                Destroy(GetComponent<Controller>());
        }

        public void Update()
        {
            if (photonView.IsMine)
                return;

            var LagDistance = RemotePlayerPosition - transform.position;

            if (LagDistance.magnitude > 5f)
            {
                transform.position = RemotePlayerPosition;
                LagDistance = Vector3.zero;
            }

            //距離を無視する
            LagDistance.y = 0;

            if (LagDistance.magnitude < 0.11f)
            {
                //プレーヤーはポイントから近い場合で
                Player.Input.RunX = 0;
                Player.Input.RunZ = 0;
            }
            else
            {
                //プレーヤーはポイントに行かなければならない
                Player.Input.RunX = LagDistance.normalized.x;
                Player.Input.RunZ = LagDistance.normalized.z;
            }

            //remoteplayerはcurrentclientより高かったら、ジャンプする
            Player.Input.Jump = RemotePlayerPosition.y - transform.position.y > 0.2f;

            Player.Input.LookX = Mathf.SmoothDamp(Player.Input.LookX, RemoteLookX, ref LookXVel, 0.2f);
            Player.Input.LookZ = Mathf.SmoothDamp(Player.Input.LookZ, RemoteLookZ, ref LookZVel, 0.2f);

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(Player.Input.LookX);
                stream.SendNext(Player.Input.LookZ);
                stream.SendNext(Player.State);
            }
            else
            {
                RemotePlayerPosition = (Vector3)stream.ReceiveNext();
                RemoteLookX = (float)stream.ReceiveNext();
                RemoteLookZ = (float)stream.ReceiveNext();
                var state = (Player.PlayerState)stream.ReceiveNext();

                //車に入る時
                if (state == Player.PlayerState.TRANSITION && Player.State == Player.PlayerState.NORMAL)
                    Player.StartCoroutine(Player.EnterCarAnimation());
                //車から出る時
                if (state == Player.PlayerState.TRANSITION && Player.State == Player.PlayerState.IN_CAR)
                    Player.StartCoroutine(Player.ExitCarAnimation());

                Player.State = state;
            }
        }
    }
}