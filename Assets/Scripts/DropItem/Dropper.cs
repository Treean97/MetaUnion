using Photon.Pun;
using UnityEngine;

public class Dropper : MonoBehaviourPun
{
    [SerializeField] private DropItemTableSO _DropTable;

    private IDestructible _Target;

    void Awake()
    {
        _Target = GetComponent<IDestructible>();
        _Target.OnDestroyed += HandleDeath;
    }

    void OnDestroy()
    {
        if (_Target != null)
            _Target.OnDestroyed -= HandleDeath;
    }

    private void HandleDeath()
    {
        foreach (var entry in _DropTable.Entries)
        {
            if (Random.value <= entry.DropChance)
            {
                int amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);

                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);

                var prefabRot = entry.ItemPrefab.transform.rotation;
                
                // 네트워크 동기화된 인스턴스 생성
                object[] instData = new object[] { amount };
                PhotonNetwork.Instantiate(
                    "Items/" + entry.ItemPrefab.name,
                    pos,
                    prefabRot,
                    data: instData
                );            
            }
        }
    }
}
