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
        private HarvestableObject _HarvestObj;


        // [SerializeField] private TMP_Text logText;

        private static MeshDemolisher meshDemolisher = new MeshDemolisher();

        void Start()
        {
            _HarvestObj = targetGameObject.GetComponent<HarvestableObject>();
        }

        private void OnEnable()
        {
            _HarvestObj.OnDestroyed += Demolish;
        }

        void OnDisable()
        {
            _HarvestObj.OnDestroyed -= Demolish;
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

            const float mass = 0.25f;      // 가벼운 파편
            const float fMin = 3.0f;       // 힘 최소
            const float fMax = 6.0f;       // 힘 최대
            const float torque = 2.0f;       // 토크 크기(랜덤)
            const float lifeSec = 6.0f;       // 자동 정리

            foreach (var go in pieces)
            {
                if (!go) continue;

                // Rigidbody
                var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
                rb.mass = mass;

                // Collider (가볍게 BoxCollider 권장; 필요하면 MeshCollider(convex)로 교체)
                var col = go.GetComponent<Collider>();
                if (!col)
                {
                    // BoxCollider가 없으면 추가
                    col = go.AddComponent<BoxCollider>();
                    // MeshCollider를 쓰고 싶다면:
                    // var mc = go.AddComponent<MeshCollider>();
                    // mc.convex = true;
                    // col = mc;
                }

                // 랜덤 힘 + 토크
                var dir = (go.transform.position - center).normalized; // 바깥 방향
                if (dir.sqrMagnitude < 1e-4f) dir = Random.onUnitSphere; // 동일위치 보정
                float force = Random.Range(fMin, fMax);
                rb.AddForce(dir * force, ForceMode.Impulse);

                var randTorque = Random.insideUnitSphere * torque;
                rb.AddTorque(randTorque, ForceMode.Impulse);

                // 4) 자동 정리
                if (lifeSec > 0f) Destroy(go, lifeSec);
            }
        }

    }

}
