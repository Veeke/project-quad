using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ProjectQuad
{
    public class Movement : MonoBehaviour
    {
        [SerializeField]
        private float maxSpeed;
        InputAction moveAction;
        private CharacterController cc;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
        }
        private void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
        }
        void Update()
        {
            Vector2 inputDir = moveAction.ReadValue<Vector2>();
            Vector3 moveDir = new(inputDir.x, 0, inputDir.y);

            Vector3 step = Vector3.MoveTowards(transform.position, transform.position + moveDir, maxSpeed * Time.deltaTime);
            cc.Move(step - transform.position);
            transform.position = new(transform.position.x, 0, transform.position.z);
        }
    }
}