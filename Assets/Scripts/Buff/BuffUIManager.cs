using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class BuffUIManager : MonoBehaviour
{
    [SerializeField] Transform _Container;
    [SerializeField] BuffSlot _BuffSlotPrefab;

    private class BuffInstance
    {
        public BuffDataSO buffData;
        public BuffSlot slot;
    }

    private Dictionary<BuffDataSO, BuffInstance> _ActiveBuffs
    = new Dictionary<BuffDataSO, BuffInstance>();

    void OnEnable()
    {
        GameEvents.OnRequestApplyBuff += HandleApplyBuff;
    }

    void OnDisable()
    {
        GameEvents.OnRequestApplyBuff -= HandleApplyBuff;
    }

    void HandleApplyBuff(BuffDataSO buff, GameObject user)
    {
        // 이미 존재한다면
        if (_ActiveBuffs.TryGetValue(buff, out var inst))
        {
            // 타이머 리셋
            inst.slot.SetSlot(buff);
        }
        else
        {
            // 신규 
            var slot = Instantiate(_BuffSlotPrefab, _Container);
            slot.SetSlot(buff);
            _ActiveBuffs[buff] = new BuffInstance { buffData = buff, slot = slot };
        }
    }

    void Update()
    {
        var expired = new List<BuffDataSO>();

        foreach (var buff in _ActiveBuffs)
        {
            if (!buff.Value.slot.UpdateSlot())
            {
                expired.Add(buff.Key);
            }
        }

        foreach (var buff in expired)
        {
            Destroy(_ActiveBuffs[buff].slot.gameObject);
            _ActiveBuffs.Remove(buff);
        }
    }
}
