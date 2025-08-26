public interface IRespawnable
{
    // 스폰포인트가 소유권 전달
    void Init(SpawnPoint owner);

    // 이 개체가 파괴되면 몇 초 뒤 리스폰할지(개별 오브젝트가 결정하게도 가능)
    float GetRespawnDelay();

    void OnSpawned();
}
