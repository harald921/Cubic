using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCommander : Photon.MonoBehaviour
{
    public void ServerMovePlayer()
    {
        // Check if can move
        // If true, begin moving player
        // Tell the other clients to move the player

        // If not, tell the player who asked this to undo
    }

    public void LocalMovePlayer()
    {
        // Check if can move
        // If true, begin moving the player locally
        // Tell the server to move the player the same way
    }
}
