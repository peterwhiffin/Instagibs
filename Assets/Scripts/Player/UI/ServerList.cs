using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using FishNet.Managing;
using System.Threading;
using UnityEngine.UI;
using System.Net.NetworkInformation;

public class ServerList : MonoBehaviour
{
    private static string API_URL = "https://api.cloud.playflow.app/";
    private static string version = "2";

    public GameObject serverBar;
    public GameObject serverWindow;
    public GameObject _mainMenu;
    public GameObject _noServersGraphic;
    public Button _startServerButton;

    public ServerBar _selectedServer;

    public int serverCount;
    public float lastServerY;

    public NetworkManager _networkManager;

    private List<string> active_servers = new List<string>();
    private List<GameObject> _serverBars = new List<GameObject>();

    public string selectedServerIP;

    private void Start()
    {
        
    }

    public async void GetServers()
    {
        await get_server_list(true);
    }

    private async Task get_server_list(bool printOutput)
    {
        active_servers.Clear();
        string response = await GetActiveServers("029e7f5c3792c24ecd311f0a94df3140", "us-east", false);
        ServerStatus[] servers = JsonHelper.FromJson<ServerStatus>(response);
        active_servers = new List<string>();
        serverCount = 0;

        foreach (ServerStatus server in servers)
        {
            serverCount += 1;
            string serverInfo = server.match_id;

            if (server.ssl_enabled)
            {
                serverInfo = server.match_id + " -> (SSL) " + server.ssl_port;
            }
            active_servers.Add(serverInfo);

            var serverBarPrefab = Instantiate(serverBar, serverWindow.transform);
            serverBarPrefab.GetComponent<ServerBar>().serverNumber = serverCount;
            serverBarPrefab.GetComponent<ServerBar>().serverIP = serverInfo;
            serverBarPrefab.GetComponent<ServerBar>().serverStatus = server.status;
            _serverBars.Add(serverBarPrefab);

            
            TMP_Text[] serverBarTexts = serverBarPrefab.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text text in serverBarTexts)
            {
                if (text.tag == "IPText")
                {
                    text.text = serverInfo + " - " + server.status + " - Ping: ";
                    break;
                }
            }

            if (serverCount > 1)
            {
                serverBarPrefab.GetComponent<RectTransform>().localPosition = new Vector3(0, lastServerY - 50f, 0);
            }

            lastServerY = serverBarPrefab.GetComponent<RectTransform>().localPosition.y;
        }

        active_servers.Sort();

        if (active_servers == null || active_servers.Count.Equals(0))
        {
            _noServersGraphic.SetActive(true);
            _startServerButton.interactable = true;
        }
        else
        {
            _noServersGraphic.SetActive(false);
            _startServerButton.interactable = false;
        }
    }

    public static async Task<string> GetActiveServers(string token, string region, bool includelaunchingservers)
    {
        String output = "";
        try
        {
            string actionUrl = API_URL + "list_servers";

            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Headers.Add("token", token);
                formData.Headers.Add("region", region);
                formData.Headers.Add("version", version);
                formData.Headers.Add("includelaunchingservers", includelaunchingservers.ToString());

                var response = await client.PostAsync(actionUrl, formData);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.Log(System.Text.Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync()));
                }

                output = System.Text.Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return output;
    }


    public async void RefreshServers()
    {
        foreach(GameObject serverBar in _serverBars)
        {
            Destroy(serverBar);
        }

        await get_server_list(true);
    }

    public async void OnStartPressed()
    {
        _startServerButton.interactable = false;
        string response = "";
        try
        {           
            response = await StartServer("029e7f5c3792c24ecd311f0a94df3140", "us-west", "", "false", "17753", "", false);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<string> StartServer(string token, string region, string arguments, string ssl, string port, string instanceType, bool isProduction)
    {

        string actionUrl = API_URL + "start_game_server";
        string output = "";
        try
        {
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                formData.Headers.Add("token", token);
                formData.Headers.Add("region", region);
                formData.Headers.Add("version", version);
                formData.Headers.Add("arguments", arguments);
                formData.Headers.Add("ssl", ssl);
                formData.Headers.Add("type", instanceType);

                if (true.ToString().Equals(ssl) && isProduction)
                {
                    formData.Headers.Add("sslport", port);
                }

                var response = await client.PostAsync(actionUrl, formData);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.Log(await response.Content.ReadAsStringAsync());
                }
               
                output = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        GetServers();
        return output;
    }



    public void OnClick_Connect()
    {
        if (_networkManager == null)
            return;

        if (_selectedServer.serverStatus == "running")
        {
            _mainMenu.SetActive(false);
            _networkManager.ClientManager.StartConnection(_selectedServer.serverIP);
            Destroy(_mainMenu);
        }
        else
            return;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.servers;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.servers = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.servers = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] servers;
        }
    }
}
