using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Project.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public static GameObject Player;
        public static GameObject CurrentPlatform;
        public static bool Dead;

        public float jumpForce = 5;
        public GameObject magic;
        public Transform magicStartPosition;

        public Texture aliveIcon;
        public Texture deadIcon;
        public RawImage[] icons;

        public GameObject gameOverPanel;

        private static readonly int IsJumping = Animator.StringToHash("isJumping");
        private static readonly int IsMagic = Animator.StringToHash("isMagic");
        private static readonly int IsDead = Animator.StringToHash("isDead");
        private static readonly int IsFalling = Animator.StringToHash("isFalling");

        private Animator _anim;
        private Rigidbody _rb;
        private Rigidbody _magicRb;
        private bool _canTurn;
        private Vector3 _startPosition;
        private int _livesLeft;
        private bool _falling;

        private InputControllers _inputControllers;
        private bool delayedDummySpawn;
        
        [SerializeField] private GameObject _gameObjectToInstanciate;
        [SerializeField] private GameObject _startPlatToInstanciate;
        [SerializeField] private GameObject _cubeOut;
        
        private GameObject spawnedObject;
        private GameObject spawnedStartPlat;
        public static GameObject _cubeOutherspace;
        private ARRaycastManager _arRaycastManager;
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

        private ARPlaneManager m_ARPlaneManager;
        private ARSession _arSession;
        private string prova;
        private List<string> _listaPiattaforme =
            new List<string>(new string[] {"platformZThin", "stairsUp", "stairsDown", "platformZ", "platformZSplit", "platformTSection"});

        private void Awake()
        {
            m_ARPlaneManager = GetComponent<ARPlaneManager>();
            _arRaycastManager = GetComponent<ARRaycastManager>();
            _inputControllers = new InputControllers();
            Player = gameObject;
            _anim = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();
            _arSession = GetComponent<ARSession>();
        }
        private void OnEnable()
        {
            _inputControllers.Enable();
            _inputControllers.SceneInteraction.Press.performed += Positioning;
        }

        private void Start()
        {
           
            _startPosition = Player.transform.position;
            _magicRb = magic.GetComponent<Rigidbody>();
            Dead = false;
            _livesLeft = PlayerPrefs.GetInt(PlayerPrefKeys.Lives);

            UpdateLivesLeftUI();
        }

        // ReSharper disable once InconsistentNaming
        private void UpdateLivesLeftUI()
        {
            for (var i = 0; i < icons.Length; ++i)
            {
                if (i >= _livesLeft)
                {
                    icons[i].texture = deadIcon;
                }
            }
        }

        
        private void OnCollisionEnter([CanBeNull] Collision other)
        {
            var isDangerousCollision = other != null && (other.gameObject.CompareTag("Fire") ||
                                                         other.gameObject.CompareTag("Wall") ||
                                                         other.gameObject.CompareTag("OuterSpace"));
            if (other != null)
            {
                prova = other.gameObject.tag;
            }
            var isLifeThreat = _falling || isDangerousCollision;
            if (isLifeThreat && !Dead)
            {
                _anim.SetTrigger(_falling ? IsFalling : IsDead);

                Dead = true;
                GameData.Singleton.SoundDying.Play();

                --_livesLeft;
                PlayerPrefs.SetInt(PlayerPrefKeys.Lives, _livesLeft);
                UpdateLivesLeftUI();

                if (_livesLeft > 0)
                {
                    Invoke(nameof(RestartGame), 2);
                }
                else
                {
                    gameOverPanel.SetActive(true);
                }
            }
            else
            {
                if (other != null) CurrentPlatform = other.gameObject;
            }
         
        }

        private void OnTriggerEnter([NotNull] Collider other)
        {
            count = 0;
            // Boxes mark spawning points.
            // We need to prevent spawning new instances in front of us when exiting into a T-section.
            // We're handling this special case in the movement (rotation) code.
            if (other is BoxCollider && !GenerateWorld.LastPlatform.CompareTag("platformTSection"))
            {
                GenerateWorld.RunDummy();
            }

            // Spheres mark turning points.
            if (other is SphereCollider)
            {
                _canTurn = true;
            }
        }

        private void OnTriggerExit([NotNull] Collider other)
        {
            // Spheres mark turning points.
            // TODO: This means that we can spin forever as long as we're inside the sphere collider. We should deactivate immediately in the Update() method.
            if (other is SphereCollider)
            {
                _canTurn = false;
            }
        }

      
        private void Update()
        {
            Debug.Log("collsione: "+prova);
            
            delayedDummySpawn = false;
            if (spawnedObject != null && spawnedStartPlat != null)
            {
                foreach (var plane in m_ARPlaneManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }
            }
            
           
        } 

        private Vector2 _direction;
        public void OnMove(InputAction.CallbackContext callbackContext)
        {
            //if (count ==1) return;
            _direction = callbackContext.ReadValue<Vector2>();
            transform.Translate(Vector3.right * (_direction.x * Time.deltaTime * 0.3f * GenerateWorld.scale));
        }

        private static Vector2 _jump;


        private int count;
       
        public void OnJump(InputAction.CallbackContext callbackContext)
        {
            count++;
            _jump = callbackContext.ReadValue<Vector2>();
            
            if ( _jump.y > Screen.height / 200f )
            {
                if (prova == "air") return;
                prova = "air";
                _anim.SetBool(IsJumping, true);
                GameData.Singleton.SoundJump.Play();
                _rb.AddForce(Vector3.up * jumpForce * GenerateWorld.scale, ForceMode.Impulse);
            }
            
        }

        private Vector2 _fire;
        public void OnFire(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                _anim.SetBool(IsMagic, true);
            }
        }

        private Vector2 _rotate;
        public void OnRotate(InputAction.CallbackContext callbackContext)
        {
            _rotate = callbackContext.ReadValue<Vector2>();

            if (_rotate.x < Screen.width / 2 && _canTurn)
            {
                transform.Rotate(Vector3.up * -90);
                GameData.Singleton.SoundWhoosh.Play();
                delayedDummySpawn = true;
            }
            else if (_rotate.x > Screen.width / 2 && _canTurn)
            {
                transform.Rotate(Vector3.up * 90);
                GameData.Singleton.SoundWhoosh.Play();
                delayedDummySpawn = true;
            }
            if (!delayedDummySpawn) return;
            var tf = transform;
            GenerateWorld.DummyTraveller.transform.forward = -tf.forward;

            GenerateWorld.RunDummy();

            // Build more platforms into the future, unless we just generated a T-section
            if (!GenerateWorld.LastPlatform.CompareTag("platformTSection"))
            {
                GenerateWorld.RunDummy();
            }

            transform.position = new Vector3(_startPosition.x, tf.position.y, _startPosition.z);
        }
        private void Positioning(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Touch");
            if (_arRaycastManager.Raycast(_inputControllers.SceneInteraction.Positioning.ReadValue<Vector2>(), hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                
                if (spawnedObject is null && spawnedStartPlat is null)
                {
                    _gameObjectToInstanciate.SetActive(true);
                    spawnedStartPlat = Instantiate(_startPlatToInstanciate, hitPose.position, _startPlatToInstanciate.transform.rotation);
                    _cubeOutherspace = Instantiate(_cubeOut,
                        spawnedStartPlat.transform.position + new Vector3(0.0f, -3.5f, 0.0f),
                        _cubeOut.transform.rotation);
                    spawnedObject = Instantiate(_gameObjectToInstanciate,spawnedStartPlat.transform.position + new Vector3(0.0f, 1.8f,0.0f) , _gameObjectToInstanciate.transform.rotation);
                    GenerateWorld.LastPlatform = spawnedStartPlat;
                    /*
                    foreach (var plane in m_ARPlaneManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }
                    */
                }
            }
        }
        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            _arSession.Reset();
        }

        [UsedImplicitly]
        private void PreCastMagic()
        {
            GameData.Singleton.SoundCastMagic.Play();
        }

        [UsedImplicitly]
        private void CastMagic()
        {
            magic.transform.position = magicStartPosition.position;
            magic.SetActive(true);
            _magicRb.AddForce(transform.forward * 20 * GenerateWorld.scale, ForceMode.Impulse);
            Invoke(nameof(KillMagic), 1);
        }

        [UsedImplicitly]
        private void FootStepLeft() => GameData.Singleton.SoundFootstep1.Play();

        [UsedImplicitly]
        private void FootStepRight() => GameData.Singleton.SoundFootstep2.Play();

        private void KillMagic()
        {
            magic.SetActive(false);

            // Reset spell forces.
            _magicRb.velocity = Vector3.zero;
        }

        [UsedImplicitly]
        private void StopJump()
        {
            _anim.SetBool(IsJumping, false);
        }

        [UsedImplicitly]
        private void StopMagic()
        {
            _anim.SetBool(IsMagic, false);
        }
    }
}
