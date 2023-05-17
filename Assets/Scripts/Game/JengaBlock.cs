using System;
using Client;
using TMPro;
using UnityEngine;

namespace Game
{
    public class JengaBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _domainTMP;

        [SerializeField] private Vector3 _blockDimensions;

        [SerializeField] private Rigidbody _rigidbody;

        [Header("Materials")]
        [SerializeField] private Material _woodMat;
        [SerializeField] private Material _stoneMat;
        [SerializeField] private Material _glassMat;

        
        public JengaBlockData BlockData { get; private set; }

        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _rigidbody.isKinematic = true;
        }

        public void Initialize(int blockIndex, JengaBlockData blockData, Transform stackBase, Transform parent)
        {
            BlockData = blockData;
            _domainTMP.text = BlockData.domain;

            int blockLevel = blockIndex / 3;
            int blockSide = (blockIndex % 3) - 1;

            var scopedTransform = transform;
            Vector3 stackBasePosition = stackBase.position;

            var stackAngle = (blockLevel % 2) * 90f;
            stackBase.eulerAngles = new Vector3(0, stackAngle, 0);
            scopedTransform.eulerAngles = new Vector3(0, stackAngle, 0);

            Vector3 position = stackBasePosition + (stackBase.right * (blockSide * _blockDimensions.x));
            position += Vector3.up * (blockLevel * _blockDimensions.y);
            scopedTransform.position = position;

            scopedTransform.parent = parent;
            stackBase.eulerAngles = Vector3.zero;

            switch (BlockData.mastery)
            {
                case 0:
                    GetComponent<Renderer>().material = _glassMat;
                    break;
                case 1:
                    GetComponent<Renderer>().material = _woodMat;
                    break;
                case 2:
                    GetComponent<Renderer>().material = _stoneMat;
                    break;
            }
        }

        public void TestBlock()
        {
            _rigidbody.isKinematic = false;
            
            if(BlockData.mastery == 0) gameObject.SetActive(false);
        }
    }
}