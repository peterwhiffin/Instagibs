using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerData", fileName = "NewPlayerData/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpHeight;
    public float maxHealth;
    public float lookSpeed;
    public float inAirSpeed;

    public float respawnTime;
    public string Username;
}
