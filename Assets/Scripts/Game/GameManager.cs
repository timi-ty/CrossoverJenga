using System.Collections.Generic;
using System.Linq;
using Client;
using TMPro;
using UnityEngine;
using Utility;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private List<Transform> _stackBases;
        
        [SerializeField] private List<TextMeshPro> _stackBasesTMPs;

        [SerializeField] private JengaBlock _jengaBlockPrefab;
        
        private readonly Dictionary<string, List<JengaBlockData>> _jengaGradeStacks = new();


        [Header("CameraSettings")] 
        
        [SerializeField] private Camera _camera;
        
        [SerializeField] private Transform _cameraPivot;
        
        [SerializeField] private Vector3 _cameraPivotOffset;
        
        [SerializeField] private Vector3 _orbitSensitivity;
        
        [SerializeField] private float _panSensitivity;

        [SerializeField] private float _zoom;
        
        private int _cameraFocus;
        
        private Vector3 _mouseDelta;
        
        private Vector3 _lastMousePos;

        private Dictionary<GameObject, JengaBlock> _jengaBlocks = new();

        [Header("UI")] [SerializeField] private TextMeshProUGUI _detailsTMP; 

        private void Start()
        {
            JengaClient.GetJengaBlocks(OnGetJengaStackData, OnRequestFailed);
        }

        private void InstantiateJengaBlocks()
        {
            int i = 0;

            var blockParent = new GameObject("JengaBlocks");
            
            foreach (var gradeStack in _jengaGradeStacks)
            {
                //There seems to be a mistake in the data. "Algebra 1" is a grade which causes there to be 4 grades instead of 3.
                //The line below is a hack to overcome this.
                if(i > 2) return;
                
                _stackBasesTMPs[i].SetText(gradeStack.Key);
                    
                IEnumerable<JengaBlockData> sortedBlockData =
                    from blockData in gradeStack.Value
                    orderby blockData.domain ascending, blockData.cluster ascending,  blockData.standardid ascending 
                    select blockData;

                int blockIndex = 0;

                foreach (var blockData in sortedBlockData)
                {
                    var jengaBlock = Instantiate(_jengaBlockPrefab);
                    jengaBlock.Initialize(blockIndex, blockData, _stackBases[i], blockParent.transform);

                    _jengaBlocks.Add(jengaBlock.gameObject, jengaBlock);
                    
                    blockIndex++;
                }

                i++;
            }
        }

        private void Update()
        {
           ProcessCameraControls();
           
           ShowExtraDetails();
        }

        private void ShowExtraDetails()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            bool hit = Physics.Raycast(ray, out var hitInfo, 200);

            if (!hit)
            {
                _detailsTMP.SetText("");
                return;
            }
            
            if (_jengaBlocks.TryGetValue(hitInfo.collider.gameObject, out var jengaBlock))
            {
                var blockData = jengaBlock.BlockData;
                string detailsText =
                    $"{blockData.grade}: {blockData.domain}\n\n{blockData.cluster}\n\n{blockData.standardid}: {blockData.standarddescription}";
                _detailsTMP.SetText(detailsText);
            }
            else
            {
                _detailsTMP.SetText("");
            }
        }

        public void TestStack()
        {
            foreach (var jengaBlock in _jengaBlocks)
            {
                jengaBlock.Value.TestBlock();
            }
        } 

        private void OnGetJengaStackData(JengaStackData jengaStackData)
        {
            foreach (var jengaBlockData in jengaStackData.jengaStackData)
            {
                if (!_jengaGradeStacks.ContainsKey(jengaBlockData.grade))
                {
                    _jengaGradeStacks.Add(jengaBlockData.grade, new List<JengaBlockData>());
                }
                _jengaGradeStacks[jengaBlockData.grade].Add(jengaBlockData);
            }
            
            InstantiateJengaBlocks();
        }
        
        private void OnRequestFailed()
        {
            Debug.LogError("Failed to fetch Jenga Blocks");
        }

        private void ProcessCameraControls()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _cameraFocus++;
                _cameraFocus %= _stackBases.Count;
            }

            _cameraPivot.position = _stackBases[_cameraFocus].position + _cameraPivotOffset;

            if (Input.GetMouseButton(0))
            {
                var angleDelta = _mouseDelta.OnlyYX();
                angleDelta.Scale(_orbitSensitivity);
                var eulerAngles = _cameraPivot.eulerAngles;
                eulerAngles += angleDelta;
                //Clamp euler angles to avoid gimbal lock;
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, 5f, 89f);
                _cameraPivot.eulerAngles = eulerAngles;
            }

            if (Input.GetMouseButton(2))
            {
                _cameraPivotOffset.y -= _panSensitivity * _mouseDelta.y;

                _cameraPivotOffset.y = Mathf.Clamp(_cameraPivotOffset.y, 0f, 3f);
            }
            
            if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
            {
                _mouseDelta = Input.mousePosition - _lastMousePos;

                _lastMousePos = Input.mousePosition;
            }
            else
            {
                _mouseDelta = Vector3.zero;
                _lastMousePos = Input.mousePosition;
            }
            
            _zoom += Input.mouseScrollDelta.y;

            _zoom = Mathf.Clamp(_zoom, -5f, -1.5f);

            var cameraTransform = _camera.transform;
            var localPosition = cameraTransform.localPosition;
            localPosition.z = _zoom;
            cameraTransform.localPosition = localPosition;
        }
    }
}