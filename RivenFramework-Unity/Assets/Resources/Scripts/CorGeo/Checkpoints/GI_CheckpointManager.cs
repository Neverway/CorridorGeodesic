using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RivenFramework;
using Newtonsoft.Json.Bson;

public class GI_CheckpointManager : MonoBehaviour
{
    [StringAsGUID] public string currentCheckpointGUID;
    private FPPawn_Player player;
    private bool playerDiedBeforeWorldLoad = false;
    private GI_PawnManager pawnManager;

    public void Start()
    {
        pawnManager = GameInstance.Get<GI_PawnManager>();
        GI_WorldLoader.OnWorldLoaded -= OnWorldLoaded;
        GI_WorldLoader.OnWorldLoaded += OnWorldLoaded;
    }
    public void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<FPPawn_Player>();
            if (player != null)
            {
                player.OnPawnDeath -= OnPlayerDeath;
                player.OnPawnDeath += OnPlayerDeath;
            }
        }
    }

    public void OnPlayerDeath() => playerDiedBeforeWorldLoad = true;
    public void OnWorldLoaded()
    {
        //Debug.Log("WorldLoaded, playerDiedState: " + playerDiedBeforeWorldLoad);
        if (playerDiedBeforeWorldLoad)
        {
            playerDiedBeforeWorldLoad = false;
            GameInstance.SendCoroutine(TryTeleportPlayerToCurrentCheckpoint());
        }
        else
            currentCheckpointGUID = null;
    }

    public bool TryGetCurrentCheckpoint(out Checkpoint checkpoint)
    {
        checkpoint = null;
        foreach (Checkpoint c in FindObjectsOfType<Checkpoint>())
            if (c.GetGUID() == currentCheckpointGUID)
            {
                checkpoint = c;
                return true;
            }
        return false;
    }

    public IEnumerator TryTeleportPlayerToCurrentCheckpoint()
    {
        if (currentCheckpointGUID == null) yield break;
        FPPawn_Player player = null;
        int tries = 0;
        while (player == null)
        {
            if (pawnManager.localPlayerCharacter)
            {
                player = pawnManager.localPlayerCharacter.GetComponent<FPPawn_Player>();
                Debug.Log($"Found player after {tries} tries");
            }
            tries += 1;
            yield return null;
            if (tries > 100)
            {
                Debug.LogError("Could not find player to teleport checkpoint to, gave up after 100 tries");
                yield break;
            }
        }
        player.Pause(out object pauseToken);

        if (TryGetCurrentCheckpoint(out Checkpoint c))
        {
            player.transform.position = c.transform.position;
        }
        else
        {
            Debug.LogError("Could not find checkpoint to teleport to");
        }
        player.Unpause(pauseToken);
    }







    public HashSet<object> pauseTokens = new HashSet<object>();
    public bool IsPaused => pauseTokens.Count > 0;
    public void Pause(out object token) =>
        pauseTokens.Add(token = new object());
    public void UnPause(object token) =>
        pauseTokens.Remove(token);










    public bool TryActivateCheckpoint(Checkpoint c)
    {
        if (c == null) return false;
        if (TryGetCurrentCheckpoint(out Checkpoint currentCheckpoint))
        {
            if (c.IsBeforeCheckpoint(currentCheckpoint))
            {
                if (c.allowBacktrackingTo)
                {
                    SetCurrentCheckpoint(c);
                    return true;
                }
                return false;
            }
            SetCurrentCheckpoint(c);
            return true;
        }
        SetCurrentCheckpoint(c);
        return true;
    }

    public void SetCurrentCheckpoint(Checkpoint checkpoint) =>
        currentCheckpointGUID = checkpoint == null ? null : checkpoint.GetGUID();

    public void ClearCheckpoint() => currentCheckpointGUID = null;
}
