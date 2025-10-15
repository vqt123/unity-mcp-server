using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Rotates entity to face movement direction or target
    /// </summary>
    public class EntityRotation : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool faceMovementDirection = true;
        
        private EntityView entityView;
        private Vector3 lastPosition;
        private Vector3 targetDirection;
        
        void Start()
        {
            entityView = GetComponent<EntityView>();
            lastPosition = transform.position;
        }
        
        void Update()
        {
            if (entityView == null) return;
            
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - lastPosition;
            
            if (movement.sqrMagnitude > 0.001f)
            {
                targetDirection = movement.normalized;
            }
            
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            
            lastPosition = currentPosition;
        }
    }
}

