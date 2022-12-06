using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using TMPro;
using FishNet.Observing;

public class Laser : NetworkBehaviour
{
    
    private Material _mat;
    
    [SyncVar]
    float _alpha;

    private void Awake()
    {
        _mat = GetComponent<LineRenderer>().material; 
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _alpha = _mat.color.a;
    }

    private void Update()
    {
        if(base.IsOwner && IsSpawned)
            LaserTimer();

        if (_mat.color.a < .0001f && base.IsOwner && this.IsSpawned)
            DestroyLaser();
    }

    [ServerRpc]
    public void DestroyLaser()
    {
        Despawn(DespawnType.Destroy);
    }


    [ServerRpc]
    public void LaserTimer()
    {
        LaserFade(_alpha);
    }

    [ObserversRpc]
    public void LaserFade(float alpha)
    {
        _mat.color = new Color(_mat.color.r, _mat.color.g, _mat.color.b, Mathf.MoveTowards(_mat.color.a, 0f, .008f));
    }
}
