using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using Lightspeed.Network.Messages;
using MessagePack;
using Network;
using Network.Packets;
using System.Net;
using System.Net.Sockets;

namespace LightspeedNexus.Networking;

/// <summary>
/// The network server
/// </summary>
public static class NetworkService
{
    private static readonly DiscoveryServer _discoveryServer = new(22750);

    /// <summary>
    /// The network socket server
    /// </summary>
    private static ServerConnectionContainer? serverConnectionContainer = null;

    /// <summary>
    /// The number of connections
    /// </summary>
    public static int Connections => serverConnectionContainer?.Count ?? 0;

    /// <summary>
    /// Determines if the server is ready
    /// </summary>
    public static bool IsReady => serverConnectionContainer is not null && serverConnectionContainer.IsTCPOnline;

    /// <summary>
    /// Initializes the server
    /// </summary>
    public static void Start()
    {
        _discoveryServer.Start();

        serverConnectionContainer = ConnectionFactory.CreateServerConnectionContainer(22749, false);
        serverConnectionContainer.ConnectionEstablished += (c, t) =>
        {
            WeakReferenceMessenger.Default.Send(new NetworkConnectionsChangedMessage());

            c.OnScoreboardRegistered(OnScoreboardRegistered);
            c.OnKeeperRegistered(OnKeeperRegistered);
            c.OnRingsRequested(OnRingsRequest);
            c.OnTournamentsRequested(OnOpenTournamentsRequest);
            c.OnMatchGroupsRequested(OnMatchGroupsRequest);
            c.OnMatchSummariesRequested(OnMatchesRequest);
            c.OnGoLiveRequested(OnGoLiveRequest);
            c.OnTimerStateReceived(OnTimerStateReceived);
            c.OnNewActionReceived(OnNewActionReceived);
            c.OnActionModifiedReceived(OnActionModifiedReceived);
            c.OnUndoActionReceived(OnUndoActionReceived);
            c.OnPriorityChangedReceived(OnPriorityChangedReceived);
            c.OnHonorStateReceived(OnHonorStateReceived);
            c.OnOrientationStateReceived(OnOrientationStateReceived);
        };

        serverConnectionContainer.ConnectionLost += (c, t, r) =>
        {
            WeakReferenceMessenger.Default.Send(new NetworkConnectionsChangedMessage());

            if (_connectionToClientId.TryGetValue(c, out Guid clientId))
            {
                if (LiveMatches.TryGetValue(clientId, out Guid matchId))
                {
                    LiveMatches.Remove(clientId);
                    WeakReferenceMessenger.Default.Send(new SetLiveMessage(false), matchId);
                }
            }
            Scoreboards.RemoveAll(s => s.Connection == c);
            ScoreKeepers.RemoveAll(s => s.Connection == c);
        };

        serverConnectionContainer.Start();
    }

    public static void Stop() => _discoveryServer.Stop();

    public static string GetIPAddress()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
        if (endPoint is not null)
            return endPoint.Address.ToString();
        return "127.0.0.1";
    }

    /// <summary>
    /// Registers a handler
    /// </summary>
    public static void Register<T>(Connection connection, string key, Action<Connection, T> handler) =>
        connection.RegisterRawDataHandler(key, (p, c) =>
            {
                try
                {
                    var response = MessagePackSerializer.Deserialize<T>(p.Data);
                    Dispatcher.UIThread.Post(() => handler(c, response));
                }
                catch (Exception)
                {
                }
            });

    /// <summary>
    /// Registers a paramaterless handler
    /// </summary>
    public static void RegisterSimple(Connection connection, string key, Action<Connection> handler) => connection.RegisterRawDataHandler(key, (_, c) => Dispatcher.UIThread.Post(() => handler(c)));

    /// <summary>
    /// Registers a raw data handler
    /// </summary>
    public static void RegisterRaw(Connection connection, string key, Action<RawData, Connection> handler) => connection.RegisterRawDataHandler(key, (p, c) => Dispatcher.UIThread.Post(() => handler(p, c)));

    #region Network Handlers

    public static readonly List<(Connection Connection, Guid RingId)> Scoreboards = [];
    public static readonly List<(Connection Connection, Guid RingId)> ScoreKeepers = [];
    public static readonly Dictionary<Guid, Guid> LiveMatches = [];
    private static readonly Dictionary<Connection, Guid> _connectionToClientId = [];

    /// <summary>
    /// A scoreboard has is registering the ring it belongs to
    /// </summary>
    private static void OnScoreboardRegistered(Connection connection, Guid ringId)
    {
        Scoreboards.RemoveAll(s => s.Connection == connection);
        Scoreboards.Add((connection, ringId));

        // if there is already a keeper associated with this ring, then we need to send it
        // the match info. This means looking up which live match it is attached to
        var keeper = ScoreKeepers.FirstOrDefault(k => k.RingId == ringId);
        if (keeper != default &&
            _connectionToClientId.TryGetValue(keeper.Connection, out Guid clientId) &&
            LiveMatches.TryGetValue(clientId, out Guid matchId))
        {
            MatchState matchState = WeakReferenceMessenger.Default.Send(new RequestMatchState(), matchId);
            connection.SendMatch(matchState);

        }
    }

    /// <summary>
    /// A score keeper app has registered the ring it is keeping score for
    /// </summary>
    private static void OnKeeperRegistered(Connection connection, Guid ringId)
    {
        ScoreKeepers.RemoveAll(s => s.Connection == connection);
        ScoreKeepers.Add((connection, ringId));
    }

    /// <summary>
    /// A client has requested a list of available rings
    /// </summary>
    private static void OnRingsRequest(Connection connection) => connection.SendRings(new FencingRings());

    /// <summary>
    /// A client has requested a list of open tournaments
    /// </summary>
    private static void OnOpenTournamentsRequest(Connection connection)
    {
        OpenTournaments tournaments = WeakReferenceMessenger.Default.Send(new RequestOpenTournaments());
        connection.SendTournaments(tournaments);
    }

    /// <summary>
    /// The client has requested a tournament's active match groups
    /// </summary>
    private static void OnMatchGroupsRequest(Connection connection, Guid tournamentId)
    {
        MatchGroupsState groups = WeakReferenceMessenger.Default.Send(new RequestActiveMatchGroups(), tournamentId);
        connection.SendMatchGroups(groups);
    }

    /// <summary>
    /// A client has request a match group's list of matches
    /// </summary>
    private static void OnMatchesRequest(Connection connection, Guid groupId)
    {
        MatchSummaries summaries = WeakReferenceMessenger.Default.Send(new RequestMatchGroupSummaries(), groupId);
        connection.SendMatchSummaries(summaries);
    }

    /// <summary>
    /// A client has chosen a match to go live with
    /// </summary>
    private static void OnGoLiveRequest(Connection connection, MatchGoLiveRequest request)
    {
        if (!_connectionToClientId.ContainsValue(request.ClientId))
            _connectionToClientId.Add(connection, request.ClientId);

        LiveMatches.TryGetValue(request.ClientId, out Guid currMatchId);
        if (currMatchId != request.MatchId)
        {
            if (currMatchId != Guid.Empty)
            {
                WeakReferenceMessenger.Default.Send(new SetLiveMessage(false), currMatchId);
                LiveMatches.Remove(currMatchId);
            }

            WeakReferenceMessenger.Default.Send(new SetLiveMessage(true), request.MatchId);
            LiveMatches.Add(request.ClientId, request.MatchId);

            MatchState matchState = WeakReferenceMessenger.Default.Send(new RequestMatchState(), request.MatchId);
            matchState.Next = WeakReferenceMessenger.Default.Send(new RequestNextMatch(request.MatchId), request.GroupId).Response;
            connection.SendMatch(matchState);

            // update all relevant Scoreboards
            foreach (var scoreboard in Scoreboards.Where(s => s.RingId == request.RingId))
                scoreboard.Connection.SendMatch(matchState);
        }
    }

    /// <summary>
    /// A client has started, stopped, or modified the time remaining
    /// </summary>
    private static void OnTimerStateReceived(Connection _, TimerState state)
    {
        if (state.Clock is not null)
            WeakReferenceMessenger.Default.Send(new ClockStateMessage(state.Clock), state.MatchId);

        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendTimerState(state);
    }

    /// <summary>
    /// A client has added a new action, which has altered the score and/or players
    /// </summary>
    private static void OnNewActionReceived(Connection _, NewActionState state)
    {
        WeakReferenceMessenger.Default.Send(new NewActionMessage(state), state.MatchId);
        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendNewAction(state);
    }

    /// <summary>
    /// A client has changed an actions point value, which has altered the score and/or players
    /// </summary>
    private static void OnActionModifiedReceived(Connection _, ActionModified state)
    {
        WeakReferenceMessenger.Default.Send(new ActionModifiedMessage(state), state.MatchId);
        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendActionModified(state);
    }

    /// <summary>
    /// A client has undone the last action, which has altered the score and/or players
    /// </summary>
    private static void OnUndoActionReceived(Connection _, UndoActionState state)
    {
        WeakReferenceMessenger.Default.Send(new UndoActionMessage(state), state.MatchId);
        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendUndoAction(state);
    }

    /// <summary>
    /// A client has undone the last action, which has altered the score and/or players
    /// </summary>
    private static void OnPriorityChangedReceived(Connection _, PriorityChanged state)
    {
        WeakReferenceMessenger.Default.Send(new PriorityChangedMessage(state), state.MatchId);
        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendPriorityChanged(state);
    }

    /// <summary>
    /// A client has added an honor point
    /// </summary>
    private static void OnHonorStateReceived(Connection _, HonorState state) => WeakReferenceMessenger.Default.Send(new HonorStateMessage(state), state.PlayerId);

    /// <summary>
    /// When a keeper adjusts orientation, we have to pass it on to the Scoreboards
    /// </summary>
    private static void OnOrientationStateReceived(Connection _, OrientationState state)
    {
        foreach (var scoreboard in Scoreboards.Where(s => s.RingId == state.RingId))
            scoreboard.Connection.SendOrientationState(state);
    }

    #endregion
}
