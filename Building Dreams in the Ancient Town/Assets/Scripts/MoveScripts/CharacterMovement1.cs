using UnityEngine;

namespace Cinemachine.Examples
{
    [AddComponentMenu("")] // Don't display in add component menu
    public class CharacterMovement1 : MonoBehaviour
    {
        public CameraSwitch CameraSwitch;
        public bool useCharacterForward = false;
        public KeyCode sprintJoystick = KeyCode.JoystickButton2;
        public KeyCode sprintKeyboard = KeyCode.Space;

        [Header("台阶爬升设置")]
        public float stepCheckDistance = 1.0f;
        public float stepRayHeight = 0.5f;
        public float maxStepHeight = 0.5f;
        public float climbSpeed = 5f;
        public LayerMask groundLayer = 1 << 0;

        private float speed = 0f;
        private float direction = 0f;
        private bool isSprinting = false;
        private Rigidbody rb;
        private Animator anim;
        private Vector2 input;
        private Camera mainCamera;
        private float velocity;

        void Start()
        {
            anim = GetComponent<Animator>();
#if UNITY_6000_0_OR_NEWER
            anim.updateMode = AnimatorUpdateMode.Fixed;
            anim.animatePhysics = true;
#else
            anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
#endif
            anim.applyRootMotion = true;
            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        }

        void Update()
        {
            if (CameraSwitch.isGodView == true)
                return;

#if ENABLE_LEGACY_INPUT_MANAGER
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");

            if (useCharacterForward)
                speed = Mathf.Abs(input.x) + input.y;
            else
                speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

            speed = Mathf.Clamp(speed, 0f, 1f);
            speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);

            if (input.y < 0f && useCharacterForward)
                direction = input.y;
            else
                direction = 0f;

            isSprinting = (Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard)) && input != Vector2.zero && direction >= 0f;
#else
            InputSystemHelper.EnableBackendsWarningMessage();
#endif
        }

        void FixedUpdate()
        {
            anim.SetFloat("Speed", speed);
            anim.SetFloat("Direction", direction);
            anim.SetBool("isSprinting", isSprinting);

            // 原始旋转/移动逻辑
            var tr = useCharacterForward ? transform : mainCamera.transform;
            var right = tr.right;
            var forward = tr.forward;
            forward.y = 0;
            var targetDir = input.x * right + (useCharacterForward ? Mathf.Abs(input.y) : input.y) * forward;

            if (input == Vector2.zero || targetDir.magnitude < 0.1f)
                rb.angularVelocity = Vector3.zero;
            else
            {
                targetDir = targetDir.normalized;
                var currentDir = rb.rotation * Vector3.forward;
                var angle = Vector3.SignedAngle(currentDir, targetDir, Vector3.up) * Mathf.Deg2Rad / Time.fixedDeltaTime;
                rb.angularVelocity = Vector3.up * angle;
            }

            // 台阶爬升检测（仅向前移动时）
            if (speed > 0.1f && input.y > 0.1f)
            {
                TryStepUp();
            }
        }

        private void TryStepUp()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * stepRayHeight + transform.forward * 0.3f;
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, stepRayHeight + 0.2f, groundLayer))
            {
                float hitY = hit.point.y;
                float currentFootY = transform.position.y;

                if (hitY > currentFootY + 0.05f && hitY - currentFootY < maxStepHeight)
                {
                    float targetY = hitY + 0.1f;
                    float newY = Mathf.MoveTowards(transform.position.y, targetY, climbSpeed * Time.fixedDeltaTime);
                    rb.MovePosition(new Vector3(transform.position.x, newY, transform.position.z));
                }
            }
        }
    }
}