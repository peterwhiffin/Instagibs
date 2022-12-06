using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using TMPro;
using System.Collections.Generic;

public class PlayerOld : NetworkBehaviour
{
    public struct ReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 crosshairPos)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }

    public struct ReplicateData
    {
        public int methodIndex;
        public Vector2 moveDir;
        public Vector2 lookDir;
        public bool jump;
        public bool Shoot;
        public Vector3 crosshairPos;
        public Ray ray;
        public bool respawn;
        public bool dead;
        public Vector3 spawnPos;
        public NetworkConnection owner;
        public Vector3 forceDirection;
    }

    [SerializeField]
    private PlayerData _playerData;
    [SerializeField]
    private GameObject _camObject;
    [SerializeField]
    private Camera _cam;
    private InputHandler _inputHandler;
    private Rigidbody _rb;
    private float cameraYRot = 0f;

    [SerializeField]
    private GameObject hitSphere;

    [SerializeField]
    private TMP_Text _timeText;

    [SerializeField]
    private Image _crosshair;
    
    
    public Image _deathOverLay;

    [SerializeField]
    private GameObject playerPrefab;


    private Vector3 spawnPos;

    [SyncVar]
    public bool playerDead;

    [SyncVar]
    public double respawnTimer;

    public bool helpMe;

    [SerializeField]
    private GameObject _laser;

    public GameObject _testSpawn;

    public LineRenderer _lineRenderer;

    [SyncVar]
    public Vector3 forceDir;

    [SyncVar]
    private double shootTimer;

    private bool inMenu;
    public GameObject _mainMenu;
    public MainMenuManager _mainMenuManager;

    public GameObject _playerModel;

    public Vector3 _camOffset;

    public Transform[] _spawns;
    public GameObject spawnsParent;
    public AudioClip[] _audioClips;
    public AudioSource _audioSource;
    public Animator _anim;
    public Material _playerMat;
    public Transform _deathCamPos;
    private Vector3 _camDefaultPos;
    public bool _camReset;
    private float _animMoveBlend;
    public Renderer _playerRenderer;

    private void Awake()
    {
        helpMe = false;
        spawnPos = transform.position;
        _rb = GetComponent<Rigidbody>();
        _inputHandler = GetComponent<InputHandler>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        //InstanceFinder.TimeManager.OnLateUpdate += TimeManager_OnLateUpdate;
        


    }

    private void LateUpdate()
    {
        if (base.IsOwner && !playerDead)
        {
            if (!_camReset)
            {
                _camObject.transform.localPosition = _camDefaultPos;
                _camReset = true;
            }


            //_camObject.transform.position = _playerModel.transform.position + _camOffset;
            cameraYRot = Mathf.Clamp(cameraYRot - (_inputHandler.LookInput.y * .1f * _playerData.lookSpeed), -85f, 85f);
            

            _camObject.transform.localEulerAngles = new Vector3(Mathf.MoveTowardsAngle(_camObject.transform.eulerAngles.x, cameraYRot, 360f), 0f, 0f);

            if(respawnTimer >= 5f)
            {
                playerDead = false;
            }
        }
        else if(base.IsOwner)
        {
            _camReset = false;
            _camObject.transform.position = Vector3.Lerp(_camObject.transform.position, _deathCamPos.position, .1f);
        }
    }

    [ServerRpc]
    public void Tester()
    {
        playerDead = false;
        respawnTimer = 0f;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            transform.eulerAngles = Vector3.zero;
            _camObject.SetActive(true);
            _camDefaultPos = _camObject.transform.localPosition;
            _camReset = true;
            inMenu = false;

            _mainMenu = Instantiate(_mainMenu, new Vector3(0, -400f, 0), Quaternion.identity);
            _mainMenuManager = _mainMenu.GetComponentInChildren<MainMenuManager>();

            spawnsParent = GameObject.FindWithTag("Spawns");
            _spawns = spawnsParent.GetComponentsInChildren<Transform>();
            _playerMat = _playerModel.GetComponent<SkinnedMeshRenderer>().material;
            
            
        }

    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        shootTimer = 2d;
    }


    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }  
    }

    [ServerRpc]
    public void ServerTimer()
    {
        respawnTimer += TimeManager.TickDelta;
        PlayerDeathFX();
    }

    
    private void TimeManager_OnTick()
    {

        if (base.IsOwner && !inMenu)
        {
            if (!playerDead)
            {
                Reconciliation(default, false);
                BuildRepData(out ReplicateData repData, 1, false);
                ReplicateFunctions(repData, false);
                _deathOverLay.gameObject.SetActive(false);
                _anim.SetFloat("Blend", Mathf.Lerp(_anim.GetFloat("Blend"), _inputHandler.MoveInput.x + _inputHandler.MoveInput.y, .1f));              
            }
            else
            {
                if (!_deathOverLay.gameObject.activeSelf)
                {
                    Reconciliation(default, false);
                    BuildRepData(out ReplicateData repData, 2, false);
                    ReplicateFunctions(repData, false);
                    _deathOverLay.gameObject.SetActive(true);
                    
                }
                
                Reconciliation(default, false);
                ServerTimer();
                

                if (respawnTimer >= 5f)
                {
                    ResetDeathFX();
                    Reconciliation(default, false);
                    BuildRepData(out ReplicateData repData, 3, false);
                    ReplicateFunctions(repData, false);
                    

                }
            }
        }

        if (base.IsServer)
        {

            ReplicateFunctions(default, true);
            shootTimer += TimeManager.TickDelta;


        }
    }

    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rb.velocity, _rb.angularVelocity, _crosshair.transform.position);
            Reconciliation(rd, true);
        }
    }



    [ObserversRpc]
    public void PlayerDeathFX()
    {
        float _dissolveOffset = _playerMat.GetFloat("_Dissolve");
        float _newOffset = Mathf.Lerp(_dissolveOffset, 1f, .1f);
        _playerMat.SetFloat("_Dissolve", _newOffset);
    }

    [ServerRpc]
    public void ResetDeathFX()
    {
        playerDead = false;
        ResetPlayerFXClient();
    }

    [ObserversRpc]
    public void ResetPlayerFXClient()
    {
        _playerMat.SetFloat("_Dissolve", 0f);
    }

    public void BuildRepData(out ReplicateData repData, int method, bool dead)
    {
        repData = default;
        repData.methodIndex = method;
        repData.jump = _inputHandler.JumpQueued;
        repData.Shoot = _inputHandler.ShootQueued;
        repData.moveDir = new Vector2(_inputHandler.MoveInput.x, _inputHandler.MoveInput.y);
        repData.lookDir = new Vector2(_inputHandler.LookInput.x, _inputHandler.LookInput.y) * _playerData.lookSpeed;
        
        repData.dead = dead;
        repData.spawnPos = _spawns[Random.Range(0, _spawns.Length)].position;
        repData.owner = Owner;
        repData.forceDirection = forceDir;

        
        repData.ray = _cam.ScreenPointToRay(_crosshair.transform.position);

        //_inputHandler._jumpQueud = false;
        //_inputHandler._shootQueued = false;

        if (repData.jump)
        {
            _anim.SetBool("Idle", false);
            _anim.SetBool("Jump", true);
        }

        if (repData.Shoot && !dead)
            Shoot(repData.ray);
    }

    [Replicate]
    private void ReplicateFunctions(ReplicateData repData, bool asServer, bool replaying = false)
    {
        if (repData.methodIndex == 1)
            Move(repData.moveDir, repData.lookDir, repData.jump);
        
        if(repData.methodIndex == 2)
            DeathForce(repData.forceDirection);
        
        if(repData.methodIndex == 3)
            ResetPostition(repData.dead, repData.spawnPos, asServer);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        _rb.velocity = rd.Velocity;
        _rb.angularVelocity = rd.AngularVelocity;
    }

    public void ResetPostition(bool dead, Vector3 spawnPos, bool asServer)
    {
        transform.position = spawnPos;
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        
        if(asServer)
            playerDead = dead;
    }

    public void JumpDone()
    {
        _anim.SetBool("Jump", false);
        _anim.SetBool("Idle", true);
    }

    public void Move(Vector2 moveDir, Vector2 lookDir, bool jump)
    {
        if (!playerDead)
        {
            float delta = (float)base.TimeManager.TickDelta;
            
            if (jump)
                _rb.AddForce(transform.up * _playerData.jumpHeight, ForceMode.Impulse);

            Vector3 newVel = ((transform.forward * moveDir.y) + (transform.right * moveDir.x)) * delta * _playerData.moveSpeed;
            _rb.velocity = Vector3.Lerp(_rb.velocity, new Vector3(newVel.x, _rb.velocity.y, newVel.z), .1f);
            transform.Rotate(transform.up, lookDir.x * .1f);
        }
    }


    public void DeathForce(Vector3 direction)
    {
        
        _rb.AddForce(direction * 4000f, ForceMode.Impulse);
    }

    [ServerRpc]
    public void Shoot(Ray ray)
    {

        if (shootTimer > .6d)
        {
            shootTimer = 0f;
            

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                GameObject laser = Instantiate(_laser, ray.origin, Quaternion.identity);
                Spawn(laser, Owner);
                LaserGraphic(laser, ray.origin, hit.point);


                if (hit.transform.CompareTag("Player"))
                {
                    var hitPlayer = hit.transform.GetComponent<PlayerOld>();

                    if (!hitPlayer.playerDead)
                    {
                        hitPlayer.respawnTimer = 0f;
                        hitPlayer._rb.constraints = RigidbodyConstraints.None;
                        NetworkConnection conn = hitPlayer.Owner;
                        hitPlayer.playerDead = true;
                        hitPlayer.forceDir = ray.direction;
                        
                    }
                }
            }
            else
            {
                GameObject laser = Instantiate(_laser, ray.origin, Quaternion.identity);
                Spawn(laser, Owner);
                LaserGraphic(laser, ray.origin, ray.origin + (ray.direction * 500f));
            }
        }
    }

    [ObserversRpc]
    private void LaserGraphic(GameObject laser, Vector3 start, Vector3 end)
    {
        _audioSource.PlayOneShot(_audioClips[0]);
        laser.GetComponent<LineRenderer>().SetPosition(0, start);
        laser.GetComponent<LineRenderer>().SetPosition(1, end);
    }

    public void MenuToggle()
    {
        inMenu = !inMenu;

        if (inMenu)
        {
            _camObject.SetActive(false);
            _mainMenu.SetActive(true);
            _mainMenuManager.SetStartUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _crosshair.enabled = false;
        }
        else
        {
            //_inputHandler._shootQueued = false;
            _camObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _mainMenu.SetActive(false);
            _crosshair.enabled = true;
        }
    }
}
