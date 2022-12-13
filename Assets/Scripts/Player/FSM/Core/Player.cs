using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using FishNet.Component.Animating;
using FishNet.Broadcast;
using FishNet.Object.Synchronizing.Internal;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public struct RepData
    {
        public Vector3 spawnRot;
        public Vector3 spawnPos;
        public Vector2 moveDir;
        public Vector2 lookDir;
        public double shootTimer;
        public double respawnTimer;
        public bool jump;
        public bool shoot;
        public bool doJump;
        public bool respawn;
        public bool dead;
        public Ray gcRay;
        public float lookSpeed;
    }

    public struct RecData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public bool PlayerDead;
        
        public RecData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, bool playerDead)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            PlayerDead = playerDead;
        }
    }

    public struct PlayerInfo : IBroadcast
    {
        public string Username;
        public int Score;
        public int Ping;
        public int Slot;
    }

    public struct ScoreBroadcast : IBroadcast
    {
        public int ID;
    }

    public struct DisconnectBroadcast : IBroadcast
    {
        public int ID;
    }

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerShootState ShootState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }
    public PlayerInAirState InAirState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    public InputHandler InputHandler { get; private set; }

    [SerializeField] private PlayerData _playerData;
    public Animator _anim;
    public GameObject _camObject;
    public Camera _cam;
    public Rigidbody _rb;
    public Image _crosshair;
    public GameObject _laser;
    public GameObject _mainMenu;
    public MainMenuManager _menuManager;
    public GameObject _playerModel;
    public Renderer _renderer;
    public MaterialPropertyBlock _propBlock;
    public Transform[] _spawns;
    public GameObject _spawnsParent;
    public AudioClip[] _audioClips;
    public AudioSource _audioSource;
    public Transform _deathCamPos;
    public Transform _groundCheckPos;
    public Collider _playerCollider;
    public NetworkAnimator _networkAnim;
    public GameObject _readyUpUI;
    public GameObject _matchTimer;
    public LayerMask _groundMask;
    public RepData _repData;
    public Vector3 _cameDefaultPos;
    public Scoreboard _scoreboard;

    public float _camYRot;

    public bool _camReset;
    public bool _inMenu;    

    [SyncVar]
    public bool _isGrounded;
    [SyncVar]
    public bool _playerDead;
    [SyncVar]
    public double _respawnTimer;
    [SyncVar]
    public double _shootTimer;
    [SyncVar]
    public int _jumpCounter;
    [SyncVar]
    public bool _isShooting;
    
    public bool _startMatch;    
    public bool _matchStarted;
    
    [SyncVar]
    private int _ownerID;

    private void Awake()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        _renderer = _playerModel.GetComponent<SkinnedMeshRenderer>();
        _propBlock = new MaterialPropertyBlock();
        _inMenu = false;    
    }

    public void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnsParent = GameObject.FindWithTag("Spawns");
        _spawns = _spawnsParent.GetComponentsInChildren<Transform>();
        _isGrounded = true;
        _shootTimer = 2d;
        _startMatch = false;
        _matchStarted = true;
        _ownerID = Owner.ClientId;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            _repData = new RepData();
            StateMachine = new PlayerStateMachine();
            IdleState = new PlayerIdleState(this, StateMachine, _playerData, "Idle", _repData);
            MoveState = new PlayerMoveState(this, StateMachine, _playerData, "Idle", _repData);
            ShootState = new PlayerShootState(this, StateMachine, _playerData, "NoAnimate", _repData);
            DeathState = new PlayerDeathState(this, StateMachine, _playerData, "NoAnimate", _repData);
            InAirState = new PlayerInAirState(this, StateMachine, _playerData, "NoAnimate", _repData);
            JumpState = new PlayerJumpState(this, StateMachine, _playerData, "Jump", _repData);
            InputHandler = GetComponent<InputHandler>();
            StateMachine.Initialize(IdleState);

            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.StartMatchBroadcast>(StartMatch);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.CheckMatchStatus>(MatchStatus);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _camObject.SetActive(true);
            //_readyUpUI.SetActive(true);
            _spawnsParent = GameObject.FindWithTag("Spawns");
            _spawns = _spawnsParent.GetComponentsInChildren<Transform>();
            _mainMenu = Instantiate(_mainMenu, new Vector3(0, -400f, 0), Quaternion.identity);
            _menuManager = _mainMenu.GetComponentInChildren<MainMenuManager>();
            _mainMenu.SetActive(false);
            _matchTimer.SetActive(true);
            
            
            PlayerInfo msg = new PlayerInfo()
            {
                Username = _playerData.Username,
                Score = 0
            };

           

            InstanceFinder.ClientManager.Broadcast(msg);
        }
    }

    public void MatchStatus(MatchManager.CheckMatchStatus status)
    {
        _startMatch = status.StartMatch;
        _matchStarted = status.MatchStarted;

        if (!_matchStarted && !_startMatch)
            _readyUpUI.SetActive(true);
    }



    public override void OnStopClient()
    {
        base.OnStopClient();

        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.StartMatchBroadcast>(StartMatch);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.CheckMatchStatus>(MatchStatus);

            
        }
    }

    public void Update()
    {
        if (base.IsOwner)
            StateMachine.CurrentState.OnUpdate();
    }

    private void LateUpdate()
    {
        if (base.IsOwner)
            StateMachine.CurrentState.CameraUpdate();
    }

    public void TimeManager_OnTick()
    {
        if (base.IsOwner)
            StateMachine.CurrentState.TickUpdate(false);

        if (base.IsServer) 
            Replicate(default, true);        
    }

    public void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            RecData rd = new RecData(transform.position, transform.rotation, _rb.velocity, _rb.angularVelocity, _playerDead);
            Reconciliation(rd, true);
        }

        if (base.IsOwner)
            StateMachine.CurrentState.PostTickUpdate();
    }

    
    public void StartMatch(MatchManager.StartMatchBroadcast start)
    {
        _startMatch = start.StartMatch;
        _matchStarted = start.MatchStarted;
    }

    public void Replicate(RepData _data, bool _asServer, bool replaying = false) => ReplicateMethods(_data, _asServer, replaying);

    public void Reconcile(RecData _data, bool _asServer) => Reconciliation(_data, _asServer);

    [Replicate]
    private void ReplicateMethods(RepData _data, bool _asServer, bool replaying = false)
    {
        MovePlayer(_data);
        GroundCheck(_data.gcRay);

        if (_data.doJump)
            JumpPlayer();

        if (_data.dead && _asServer)
        {
            PlayerDeathFX();
            RespawnTimer(false);
        }

        if (_data.respawnTimer >= 5d && _data.dead)
            ResetPlayer(_data, _asServer);
    }

    [Reconcile]
    private void Reconciliation(RecData _data, bool _asServer)
    {
        transform.position = _data.Position;
        transform.rotation = _data.Rotation;
        _rb.velocity = _data.Velocity;
        _rb.angularVelocity = _data.AngularVelocity;
    }

    public void RotateCam(float mouseY)
    {
        float delta = (float)base.TimeManager.TickDelta;
        _camYRot = Mathf.Clamp(_camYRot - (mouseY * _playerData.lookSpeed * delta), -85f, 85f);
        _camObject.transform.localEulerAngles = new Vector3(Mathf.MoveTowardsAngle(_camObject.transform.eulerAngles.x, _camYRot, 360f), 0f, 0f);
    }


    public void MovePlayer(RepData _data)
    {
        float delta = (float)base.TimeManager.TickDelta;       
        Vector3 newVel = ((transform.forward * _data.moveDir.y) + (transform.right * _data.moveDir.x)) * delta * _playerData.moveSpeed;
        _rb.velocity = Vector3.Lerp(_rb.velocity, new Vector3(newVel.x, _rb.velocity.y, newVel.z), .1f);
        transform.Rotate(transform.up, _data.lookDir.x * delta * _data.lookSpeed);
    }

    public void JumpPlayer() => _rb.AddForce(transform.up * _playerData.jumpHeight, ForceMode.Impulse);

    [ServerRpc(RunLocally = false)]
    public void Shoot(Ray ray, NetworkConnection conn = null)
    {        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            _shootTimer = 0d;
            GameObject laser = Instantiate(_laser, ray.origin, Quaternion.identity);
            Spawn(laser, Owner);
            LaserGraphic(laser, ray.origin, hit.point);

            if (hit.transform.CompareTag("Player"))
            {
                var hitPlayer = hit.transform.GetComponent<Player>();

                if (!hitPlayer._playerDead)
                {
                    hitPlayer._respawnTimer = 0d;
                    hitPlayer._rb.constraints = RigidbodyConstraints.FreezeAll;
                    hitPlayer._playerCollider.enabled = false;
                    hitPlayer._playerDead = true;
                    SendScoreBroadcast(conn);
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

    [TargetRpc]
    private void SendScoreBroadcast(NetworkConnection conn)
    {
        var SB = new ScoreBroadcast()
        {
            ID = Owner.ClientId
        };

        InstanceFinder.ClientManager.Broadcast(SB);
    }

    [ObserversRpc]
    private void LaserGraphic(GameObject laser, Vector3 start, Vector3 end)
    {
        _audioSource.PlayOneShot(_audioClips[0]);
        laser.GetComponent<LineRenderer>().SetPosition(0, start);
        laser.GetComponent<LineRenderer>().SetPosition(1, end);
    }

    [ServerRpc]
    public void ShootTimer(bool reset)
    {
        _shootTimer += TimeManager.TickDelta;

        if(reset)
            _shootTimer = 0d;
    }

    public void RespawnTimer(bool reset)
    {
        if (reset)
            _respawnTimer = 0d;
        else
            _respawnTimer += TimeManager.TickDelta;
    }

    public void ResetPlayer(RepData _data, bool asServer)
    {
        _rb.constraints = RigidbodyConstraints.None;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _playerCollider.enabled = true;
        transform.position = _data.spawnPos;
        transform.eulerAngles = _data.spawnRot;

        if (asServer)
        {
            _playerDead = false;
            ResetPlayerFX();
        }
    }

    [ObserversRpc]
    public void ResetPlayerFX()
    {
        _renderer.GetPropertyBlock(_propBlock, 0);
        _propBlock.SetFloat("_Dissolve", 0f);
        _renderer.SetPropertyBlock(_propBlock, 0);
    }

    [ObserversRpc]
    public void PlayerDeathFX()
    {
        _renderer.GetPropertyBlock(_propBlock, 0);
        _propBlock.SetFloat("_Dissolve", Mathf.Lerp(_propBlock.GetFloat("_Dissolve"), 1f, .01f));
        _renderer.SetPropertyBlock(_propBlock, 0);
    }

    public void GroundCheck(Ray _ray)
    {
        if (Physics.Raycast(_ray, out RaycastHit hit, 10f, _groundMask))
        {
            if (hit.distance > .12f)
                _isGrounded = false;
            else
                _isGrounded = true;
        }
    }

    [ServerRpc]
    public void JumpCounter(bool reset)
    {
        if(!reset)
            _jumpCounter++;
        else
            _jumpCounter = 0;
    }

    public void MenuToggle()
    {
        _inMenu = !_inMenu;

        if (_inMenu)
        {
            _camObject.SetActive(false);
            _mainMenu.SetActive(true);
            _menuManager.SetStartUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _crosshair.enabled = false;
        }
        else
        {
            _camObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _mainMenu.SetActive(false);
            _crosshair.enabled = true;
        }
    }
}
