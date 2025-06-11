using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;


namespace Com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {


        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0); //빌드 세팅 0번 씬으로 이동
        }


        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        #endregion

        #region Private Methods

        void LoadArena() //멀티이기 때문에 씬을 변경해도 다른 사람이 같이 되도록 해야 한다.- 포톤 네트워크 쓰기
        {
            if (!PhotonNetwork.IsMasterClient) //방장이 없을 경우, 로드할 수 없다고 알림.
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }


            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount); //현재 인원에 맞는 룸 배정
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount); //플레이어의 수에 맞느 룸 배정하기(방장)
        }

        #endregion

        #region Photon Callbacks


        /// <summary>
        /// 로컬 플레이어가 방을 나갔을 때 호출됩니다. 런처 장면을 로드해야 합니다.
        /// </summary>

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // 플레이어가 연결중일 때 뜨지 않음


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); //OnPlayerLeftRoom 전에 호출


                LoadArena(); //다른 사람에게도 플레이어가 방을 떠났는 것을 알려준다. 
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // 다른 사람의 연결이 끊겼을 때


            if (PhotonNetwork.IsMasterClient) //만약 방장이라면
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // 방장인 경우에만 룸을 떠나는 사람을 통보해줌.


                LoadArena(); //다른 사람에게도 플레이어가 방을 떠났는 것을 알려준다. 
            }
        }

        #endregion
    }
}