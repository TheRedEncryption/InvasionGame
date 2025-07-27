using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Describes the current state of the game: "Lobby" phase is when clients can connect and rules can be changed.
/// "Build" phase is whenever there is atleast 1 client still building their towers.
/// "Run" phase goes through the list of players, and generates a map based on what towers they placed where; it is when people can be eliminated.
/// "Results" is when there is only one person left alive and the server needs to be prepared to sent back to "Lobby" mode.
/// </summary>
/// <remarks>
/// 0: Lobby (Initial)
/// 1: Build Phase
/// 2: Run Phase
/// -1: Results (End of game) Phase
/// </remarks>
public static class MatchState
{
    public delegate void MatchPhaseChangeDelegate(ulong[] players);

    public static event MatchPhaseChangeDelegate PhaseChangeCallBack;
    public static event MatchPhaseChangeDelegate EnterLobbyPhaseCallBack;
    public static event MatchPhaseChangeDelegate EnterBuildPhaseCallBack;
    public static event MatchPhaseChangeDelegate EnterRunPhaseCallBack;
    public static event MatchPhaseChangeDelegate EnterResultsPhaseCallBack;

    /// <summary>
    /// Used in describing the current state of the match in more human readable terms.
    /// </summary>
    public enum Phase { EndOfGame = 3, Lobby = 0, Build = 1, Run = 2 }

    /// <summary>
    /// The master variable deciding how basically the entire game runs; tracks the match's state,
    /// </summary>
    private static byte _current = 0;

    /// <summary>
    /// Accessor property for the tracker.
    /// </summary>
    public static byte Current => _current;

    /// <summary>
    /// Determines if the server is currently running a match.
    /// </summary>
    /// <remarks><c>True</c> if the current phase of the server is either Run, or Build.</remarks>
    public static bool IsGame => _current > 0;

    /// <summary>
    /// Determines if the server is currently waiting in a lobby.
    /// </summary>
    /// <remarks><c>True</c> if the current phase of the server is either Lobby, or Results.</remarks>
    public static bool IsLobby => _current <= 0;

    /// <summary>
    /// Gets the current phase of the match in human readable terms.
    /// </summary>
    /// <remarks>Avoid unless human needs to understand, directly, the data stored in <see cref="Current"/>.</remarks>
    /// <returns>The value of <see cref="_current"/> parsed into a more easily understood enum.</returns>
    public static Phase CurrentPhase => (Phase)_current;

    /// <summary>
    /// Changes the state of the match, this is an incribly powerful command and can fundamentally change how things work, so be very careful when using it!
    /// </summary>
    /// <param name="newStateID"></param>
    public static void SetMatchPhase(byte newStateID)
    {
        _current = newStateID;
        PhaseChangeCallBack?.Invoke(NetworkManager.Singleton.ConnectedClientsIds.ToArray());
        PingMatchPhase();
    }

    /// <summary>
    /// Invokes whichever event correlates with the current match phase.
    /// </summary>
    private static void PingMatchPhase()
    {
        ulong[] listOfPlayerIDs = NetworkManager.Singleton.ConnectedClientsIds.ToArray();

        if (Current == 3)
            EnterResultsPhaseCallBack?.Invoke(listOfPlayerIDs);
        else if (Current == 0)
            EnterLobbyPhaseCallBack?.Invoke(listOfPlayerIDs);
        else if (Current == 1)
            EnterBuildPhaseCallBack?.Invoke(listOfPlayerIDs);
        else if (Current == 2)
            EnterRunPhaseCallBack?.Invoke(listOfPlayerIDs);
        else
            Debug.LogError("Match phase is out of range");

    }
}

public class RoleManagement : NetworkBehaviour
{
    private List<Roles> _roles = new();

    /// Server: T - Don't bother doing things to this instance that invole roles, F - Do said things
    /// Host: T - This person can change the can, F - this person is not the mod and therefore has no control over lobby settings
    /// IsAlive: T - This player can make changes to their base
    public enum Roles
    {
        // Raw data
        Server  = 0b_0000_0001,
        Host    = 0b_0000_0010,
        IsAlive = 0b_0000_0100,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SetInitialClientState;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveClientState;
        MatchState.EnterLobbyPhaseCallBack += MarkEveryoneAsAlive;
    }

    /// <summary>
    /// Gets the roles of a player
    /// </summary>
    /// <returns>The roles of a player</returns>
    public Roles GetClientState(ulong id) => _roles[(int)id];

    /// <summary>
    /// Sets a new clients flags when they first join a server.
    /// </summary>
    /// <param name="clientID"></param>
    public void SetInitialClientState(ulong clientID)
    {
        Roles clientStates = MatchState.IsLobby ? Roles.IsAlive : 0;

        if (_roles.Count == 0)
        {
            clientStates |= Roles.Host;
            Debug.Log("You are the host now");
        }

        _roles.Add(clientStates);
    }

    #region change active player flags

    /// <summary>
    /// Completely replaces the flags of a given playerID
    /// </summary>
    public void OverrideFlags(ulong clientID, Roles flags) => _roles[(int)clientID] = flags;

    /// <summary>
    /// Sets a selection of flags to true.
    /// </summary>
    public void SetFlagsTrueClientState(ulong clientID, Roles flags) => _roles[(int)clientID] |= flags;

    /// <summary>
    /// Sets a selection of flags to false.
    /// </summary>
    public void SetFlagsFalseClientState(ulong clientID, Roles flags) => _roles[(int)clientID] &= ~flags;

    /// <summary>
    /// Flips a selection of flags
    /// </summary>
    public void UpdateClientState(ulong clientID, short roles)
    {
        short flags = (short)_roles[(int)clientID];
        flags ^= roles;
        _roles[(int)clientID] = (Roles)flags;
    }

    /// <summary>
    /// Flips a selection of flags
    /// </summary>
    public void UpdateClientState(ulong clientID, Roles roles) => UpdateClientState(clientID, (short)roles);

    #endregion

    /// <summary>
    /// Removes a "role" to avoid clogging when people disconnent. Also handles reassignment of host should they leave.
    /// </summary>
    public void RemoveClientState(ulong clientID)
    {
        // If the host is leaving
        if (clientID == 0)
        {
            if (_roles.Count == 1)
            {
                // End the server here
            }
            else
            {
                SetFlagsTrueClientState(1, Roles.Host);
            }
        }

        _roles.RemoveAt((int)clientID);
    }

    /// <summary>
    /// Sets everyone's flags to show their alive
    /// </summary>
    /// <param name="players"></param>
    public void MarkEveryoneAsAlive(ulong[] players)
    {
        for (int i = 0; i < _roles.Count; i++)
        {
            SetFlagsTrueClientState((ulong)i, Roles.IsAlive);
        }
    }

}
