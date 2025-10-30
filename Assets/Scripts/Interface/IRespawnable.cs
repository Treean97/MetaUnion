using UnityEngine;

public interface IRespawnable
{
    // 전역 매니저가 필요한 정보를 제공
    string PrefabName { get; } // PhotonNetwork.Instantiate용(Resources 프리팹 이름)
    float RespawnDelay { get; } // 리스폰 지연(초)
    Transform RespawnAnchor { get; } // 리스폰 위치/회전(보통 최초 자리)
}
