using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
public class ServerOnly : NetworkBehaviour
{
    public GameObject _matchManager;
    public GameObject _mainMenu;

    private void Awake()
    {
        if (!IsServer)
            _mainMenu.SetActive(true);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _matchManager.SetActive(true);
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }


}
