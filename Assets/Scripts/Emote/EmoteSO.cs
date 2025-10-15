using UnityEngine;

[CreateAssetMenu (menuName = "EmoteData")]
public class EmoteSO : ScriptableObject
{
    [Header("Animator")]
    [SerializeField] string _ID;
    public string ID => _ID;
    [SerializeField] string _StateName;   // Animator 상태 이름
    public string StateName => _StateName;
    [SerializeField] InfoDataSO _InfoDataSO;
    public InfoDataSO InfoDataSO => _InfoDataSO;
    [SerializeField] int _Layer = 0; // 재생 레이어
    public int Layer => _Layer;
    [SerializeField] float _LengthSeconds; // 총 길이(초) - 정확히 입력
    public float Length => Mathf.Max(0.01f, _LengthSeconds);
    [SerializeField] GameObject _EmoteAnchor; // 대형 프리팹
    public GameObject EmoteAnchor => _EmoteAnchor;
    [SerializeField] string _SFXKey;
    public string SFXKey => _SFXKey;
    [SerializeField] Sprite _Icon;
    public Sprite Icon => _Icon;
    

}
