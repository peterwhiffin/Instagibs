using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using System;

[Serializable]
public class ServerStatus
{
    public string ssl_port;
    public bool ssl_enabled;
    public string server_arguments;
    public string status;
    public string port;
    public string match_id;
    public string ssl_url;
}
