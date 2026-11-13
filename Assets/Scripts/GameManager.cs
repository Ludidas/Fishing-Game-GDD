using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action OnPlayerRespawn;

    [Header("Player")]
    public Transform playerSpawnPoint;
    public MovementComponent player;

    /// <summary>
    /// Initialize the game manager singleton
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Handle the player's death
    /// </summary>
    public void PlayerDeath()
    {
        RespawnPlayer();
    }

    /// <summary>
    /// Respawn the player at the spawn point
    /// </summary>
    public void RespawnPlayer()
    {
        if (player == null || playerSpawnPoint == null) return;

        OnPlayerRespawn?.Invoke();

        player.ResetSpeed();
        player.SetPosition(playerSpawnPoint.position);
        player.ResetGrapple();
    }
}