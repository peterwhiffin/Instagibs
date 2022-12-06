using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ServerBar : EventTrigger
{
    public int serverNumber;
    public string serverIP;
    public string serverStatus;
    public Button connectButton;
    

    public ServerList _serverList;

    private void Start()
    {
        var buttons = FindObjectsOfType<Button>();

        foreach (var button in buttons)
        {
            if(button.name == "ConnectButton")
            {
                connectButton = button;
            }
        }

        _serverList = GetComponentInParent<ServerList>();
    }

    private void Update()
    {
        GetComponent<Button>().onClick.AddListener(On_Click);
              
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
    }

    public void On_Click()
    {
        connectButton.interactable = true;
        _serverList._selectedServer = this;
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(On_Click);
    }
}
