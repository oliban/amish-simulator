using UnityEngine;
using System;

namespace AmishSimulator
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public Gender Gender { get; private set; }
        public bool IsMarried { get; private set; }
        public int ChildrenCount { get; private set; }
        public string SpouseNpcId { get; private set; }

        public event Action OnMarried;
        public event Action OnChildBorn;

        private AgingSystem _agingSystem;
        private BeardSystem _beardSystem;

        [SerializeField] private float moveSpeed = 4f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _agingSystem = GetComponent<AgingSystem>();
            _beardSystem = GetComponent<BeardSystem>();
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            var move = new Vector3(h, 0, v).normalized;
            if (move != Vector3.zero)
            {
                transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.LookRotation(move);
            }
        }

        public void Initialize(Gender g)
        {
            Gender = g;
            IsMarried = false;
            ChildrenCount = 0;
        }

        public Gender GetGender() => Gender;
        public int GetAge() => _agingSystem != null ? _agingSystem.CurrentAge : 18;
        public bool GetIsMarried() => IsMarried;

        public void Marry(string spouseNpcId)
        {
            if (IsMarried) return;
            IsMarried = true;
            SpouseNpcId = spouseNpcId;
            OnMarried?.Invoke();

            // Trigger beard growth for male
            if (Gender == Gender.Male && _beardSystem != null)
                _beardSystem.OnMarried();
        }

        public void HaveChild()
        {
            ChildrenCount++;
            OnChildBorn?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.GameStats?.IncrementChildren();
        }
    }
}
