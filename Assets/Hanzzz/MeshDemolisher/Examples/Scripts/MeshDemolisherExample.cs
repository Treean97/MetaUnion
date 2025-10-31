using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hanzzz.MeshDemolisher
{
    public class MeshDemolisherExample : MonoBehaviour
    {
        [Header("Hint: right click on the script to call Demolish and Reset\nin the editor mode.")]
        [Space]
        [SerializeField] private GameObject _TargetGameObject;
        [SerializeField] private Transform _BreakPointsParent;
        [SerializeField] private Material _InteriorMaterial;

        [SerializeField] [Range(0f,1f)] private float _ResultScale;
        [SerializeField] private Transform _ResultParent;

        [Header("Break Power")]
        [SerializeField] float _Mass = 0.25f;
        [SerializeField] float _EjectForceMin = 2.0f;
        [SerializeField] float _EjectForceMax = 3.0f;
        [SerializeField] float _EjectTorque   = 1.5f;
        private IDestructible _Destrutible;
        readonly List<GameObject> _Pieces = new();

        // [SerializeField] private TMP_Text logText;

        private static MeshDemolisher _MeshDemolisher = new MeshDemolisher();

        void Awake()
        {
            _Destrutible = GetComponentInParent<IDestructible>();
        }

        void OnEnable()
        {
            if (_Destrutible != null) _Destrutible.OnDestroyed += Demolish;
        }

        void OnDisable()
        {
            if (_Destrutible != null) _Destrutible.OnDestroyed -= Demolish;
        }

        [ContextMenu("Verify Demolish Input")]
        public void VerifyDemolishInput()
        {
            List<Transform> breakPoints = Enumerable.Range(0,_BreakPointsParent.childCount).Select(x=>_BreakPointsParent.GetChild(x)).ToList();

            // Passing this verification does not mean the input is valid.
            // Refer to the documentation to see all input requirements.
            bool res = _MeshDemolisher.VerifyDemolishInput(_TargetGameObject, breakPoints);
            if(res)
            {
                Debug.Log("Demolish input looks good.");
            }
        }

        [ContextMenu("Demolish")]
        public void Demolish()
        {
            Enumerable.Range(0,_ResultParent.childCount).Select(i=>_ResultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            List<Transform> breakPoints = Enumerable.Range(0,_BreakPointsParent.childCount).Select(x=>_BreakPointsParent.GetChild(x)).ToList();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<GameObject> res = _MeshDemolisher.Demolish(_TargetGameObject, breakPoints, _InteriorMaterial);
            watch.Stop();
            // logText.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

            res.ForEach(x=>x.transform.SetParent(_ResultParent, true));
            Enumerable.Range(0,_ResultParent.childCount).Select(i=>_ResultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=_ResultScale*Vector3.one);
            AddSimplePhysics(res, _TargetGameObject.transform.position);
            _TargetGameObject.SetActive(false);
        }

        [ContextMenu("Demolish Async")]
        public async void DemolishAsync()
        {
            Enumerable.Range(0,_ResultParent.childCount).Select(i=>_ResultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            List<Transform> breakPoints = Enumerable.Range(0,_BreakPointsParent.childCount).Select(x=>_BreakPointsParent.GetChild(x)).ToList();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<GameObject> res = await _MeshDemolisher.DemolishAsync(_TargetGameObject, breakPoints, _InteriorMaterial);
            watch.Stop();
            // logText.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

            res.ForEach(x=>x.transform.SetParent(_ResultParent, true));
            Enumerable.Range(0,_ResultParent.childCount).Select(i=>_ResultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=_ResultScale*Vector3.one);

            _TargetGameObject.SetActive(false);
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            //Enumerable.Range(0,breakPointsParent.childCount).Select(i=>breakPointsParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            Enumerable.Range(0,_ResultParent.childCount).Select(i=>_ResultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));

            _TargetGameObject.SetActive(true);
        }

        public void OnValidate()
        {
            Enumerable.Range(0, _ResultParent.childCount).Select(i => _ResultParent.GetChild(i)).ToList().ForEach(x => x.localScale = _ResultScale * Vector3.one);
        }
        
        void AddSimplePhysics(List<GameObject> pieces, Vector3 center)
        {
            if (pieces == null) return;

            foreach (var go in pieces)
            {
                if (!go) continue;

                // 정적이면 컴포넌트 추가가 실패할 수 있으니 해제
                if (go.isStatic) go.isStatic = false;

                // Rigidbody 보장
                if (!go.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb = go.AddComponent<Rigidbody>();
                    if (rb == null)
                    {
                        Debug.LogWarning($"[Break] Rigidbody 추가 실패: {go.name}");
                        continue;
                    }
                }

                rb.mass = _Mass;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

                // Collider 보장 (없으면 가벼운 BoxCollider 추가)
                if (!go.TryGetComponent<Collider>(out var col))
                {
                    col = go.AddComponent<BoxCollider>();
                }

                // 힘+토크
                var dir = (go.transform.position - center).normalized;
                if (dir.sqrMagnitude < 1e-4f) dir = Random.onUnitSphere;

                rb.AddForce(dir * Random.Range(_EjectForceMin, _EjectForceMax), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * _EjectTorque, ForceMode.Impulse);
            }
        }


    }

}
