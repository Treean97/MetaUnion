using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hanzzz.MeshDemolisher
{
    public class MeshDemolisherExample : MonoBehaviour
    {
        [Header("Hint: right click on the script to call Demolish and Reset\nin the editor mode.")]
        [Space]
        [SerializeField] private GameObject targetGameObject;
        [SerializeField] private Transform breakPointsParent;
        [SerializeField] private Material interiorMaterial;

        [SerializeField] [Range(0f,1f)] private float resultScale;
        [SerializeField] private Transform resultParent;
        private IDestructible _Destrutible;


        // [SerializeField] private TMP_Text logText;

        private static MeshDemolisher meshDemolisher = new MeshDemolisher();

        void Start()
        {
            _Destrutible = targetGameObject.GetComponentInParent<IDestructible>();
            _Destrutible.OnDestroyed += Demolish;
        }

        void OnDestroyed()
        {
            _Destrutible.OnDestroyed -= Demolish;
        }

        [ContextMenu("Verify Demolish Input")]
        public void VerifyDemolishInput()
        {
            List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

            // Passing this verification does not mean the input is valid.
            // Refer to the documentation to see all input requirements.
            bool res = meshDemolisher.VerifyDemolishInput(targetGameObject, breakPoints);
            if(res)
            {
                Debug.Log("Demolish input looks good.");
            }
        }

        [ContextMenu("Demolish")]
        public void Demolish()
        {
            Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<GameObject> res = meshDemolisher.Demolish(targetGameObject, breakPoints, interiorMaterial);
            watch.Stop();
            // logText.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

            res.ForEach(x=>x.transform.SetParent(resultParent, true));
            Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=resultScale*Vector3.one);
            AddSimplePhysics(res, targetGameObject.transform.position);
            targetGameObject.SetActive(false);
        }

        [ContextMenu("Demolish Async")]
        public async void DemolishAsync()
        {
            Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<GameObject> res = await meshDemolisher.DemolishAsync(targetGameObject, breakPoints, interiorMaterial);
            watch.Stop();
            // logText.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

            res.ForEach(x=>x.transform.SetParent(resultParent, true));
            Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>x.localScale=resultScale*Vector3.one);

            targetGameObject.SetActive(false);
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            //Enumerable.Range(0,breakPointsParent.childCount).Select(i=>breakPointsParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
            Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));

            targetGameObject.SetActive(true);
        }

        public void OnValidate()
        {
            Enumerable.Range(0, resultParent.childCount).Select(i => resultParent.GetChild(i)).ToList().ForEach(x => x.localScale = resultScale * Vector3.one);
        }
        
        void AddSimplePhysics(List<GameObject> pieces, Vector3 center)
        {
            if (pieces == null) return;

            const float mass = 0.25f;
            const float fMin = 3.0f;
            const float fMax = 6.0f;
            const float torque = 2.0f;

            float lifeSec = Mathf.Max(0.1f, RespawnManager.GlobalBreakFxSeconds);

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

                rb.mass = mass;
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

                rb.AddForce(dir * Random.Range(fMin, fMax), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);

                // 수명 정리
                if (lifeSec > 0f) Destroy(go, lifeSec);
            }
        }


    }

}
