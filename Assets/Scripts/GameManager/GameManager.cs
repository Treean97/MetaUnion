using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


namespace Com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager _Inst { get; private set; }

        [SerializeField] private GameObject _PlayerPrefab;        
        
        
        
        private void Awake()
        {
            // 이미 인스턴스가 존재하고, 이 인스턴스가 아니라면 자기 자신을 파괴
            if (_Inst != null && _Inst != this)
            {
                Destroy(gameObject);
                return;
            }

            // 최초 인스턴스로 등록
            _Inst = this;
            DontDestroyOnLoad(gameObject);

            // 씬 로드 완료 콜백 등록
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("✅ GameManager 초기화 완료");
        }


        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }



        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("MainScene"); // 메인화면으로 이동
        }


        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            GameEvents.RaiseLeaveRoom();
        }


        #endregion

        #region Private Methods

        // void LoadArena() //멀티이기 때문에 씬을 변경해도 다른 사람이 같이 되도록 해야 한다.- 포톤 네트워크 쓰기
        // {
        //     if (!PhotonNetwork.IsMasterClient) //방장이 없을 경우, 로드할 수 없다고 알림.
        //     {
        //         Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        //     }
        // }

        #endregion

        #region 방 출입

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // ① Launcher에서 공개한 프로퍼티로 SceneListSO를 꺼내 오고
            var sceneListSO = Launcher._Inst.GetGameSceneListSO;
            var list = sceneListSO._SceneList;

            // ② 로드된 씬 이름이 리스트에 있으면 게임 방. 없으면 게임 방 아님.
            bool isGameRoom = list.Any(e => e.SceneName == scene.name);
            if (!isGameRoom)
            {
                Debug.Log("게임 방이 아닙니다. 플레이어를 생성하지 않습니다.");
                return;
            }
            else
            {
                Debug.Log("게임 방입니다. 플레이어를 생성합니다.");
                PhotonNetwork.Instantiate(_PlayerPrefab.name, Vector3.zero, Quaternion.identity);
            }
            
        }


        /// <summary>
        /// 로컬 플레이어가 방을 나갔을 때 호출됩니다. 런처 장면을 로드해야 합니다.
        /// </summary>

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // 플레이어가 연결중일 때 뜨지 않음


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); //OnPlayerLeftRoom 전에 호출
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // 다른 사람의 연결이 끊겼을 때


            if (PhotonNetwork.IsMasterClient) //만약 방장이라면
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // 방장인 경우에만 룸을 떠나는 사람을 통보해줌.
            }
        }

        #endregion
    }
}