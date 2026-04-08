using Lightspeed.ViewModels;
using LightspeedNexus.ViewModels;
using System.Text.Json.Nodes;

namespace LightspeedNexus.Services;

/// <summary>
/// Converts tournament data to the format required for submission to saber-sports.com. This is a static class
/// and therefore can be accessed directly instead of injected
/// </summary>
public static class SaberSportsSerializer
{
    /// <summary>
    /// Creates a new Fighter instance from a SaberSport-formatted JSON node.
    /// </summary>
    public static Fighter? DeserializeFighter(JsonNode? node)
    {
        if (node is null)
            return null;

        try
        {
            Fighter fighter = new()
            {
                // these can throw because they are required
                OnlineId = node["id"]?.GetValue<int>(),
                FirstName = node["first_name"]?.GetValue<string>() ?? string.Empty,
                LastName = node["last_name"]?.GetValue<string>() ?? string.Empty
            };

            // optional
            if (node["club"] is JsonValue jsonClub && jsonClub.GetValueKind() == System.Text.Json.JsonValueKind.String)
                fighter.Club = jsonClub.GetValue<string>();

            if (node["ranks"] is JsonArray ranks)
            {
                foreach (var rankNode in ranks)
                {
                    var rating = WeaponRating.FromSaberSport(rankNode);
                    if (rating is not null)
                    {
                        switch (rating.Class)
                        {
                            case WeaponClass.Rey when rating.Rank > fighter.Rey:
                                fighter.Rey = rating.Rank;
                                break;
                            case WeaponClass.Ren when rating.Rank > fighter.Ren:
                                fighter.Ren = rating.Rank;
                                break;
                            case WeaponClass.Tano when rating.Rank > fighter.Tano:
                                fighter.Tano = rating.Rank;
                                break;
                            default:
                                Console.WriteLine($"Unknown weapon class: {rating.Class}");
                                break;
                        }
                    }
                }
            }

            // one last check to make sure there's a name
            if (string.IsNullOrEmpty(fighter.FirstName) || string.IsNullOrEmpty(fighter.LastName))
                return null;

            return fighter;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing fighter: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Serializes a tournament into a JSON format suitable for submission to saber-sports.com.
    /// The tournament must have a pools stage, a bracket stage, and a results stage to be serialized successfully.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static JsonObject Serialize(TournamentViewModel tournament)
    {
        PoolsStageViewModel pools = tournament.FindStage<PoolsStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without pools stage.");
        BracketStageViewModel bracket = tournament.FindStage<BracketStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without bracket stage.");
        ResultsStageViewModel results = tournament.FindStage<ResultsStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without results stage.");

        var rounds = new JsonArray();
        for (int i = 0; i < pools.Pools.Count; i++)
            rounds.Add(SerializePool(pools.Pools[i], i));
        rounds.Add(SerializeBracket(bracket));

        var node = new JsonObject
        {
            ["uuid"] = $"lsn{tournament.Guid}",
            ["title"] = tournament.SetupStage.EventName is not null ? $"{tournament.SetupStage.EventName} {tournament.SetupStage.Title}" : tournament.SetupStage.Title,
            ["date"] = tournament.SetupStage.Date?.ToString("yyyy-MM-dd"),
            ["gender"] = tournament.SetupStage.Demographic switch { Demographic.Women => "women", Demographic.Cadet => "cadet", _ => "mixed" },
            ["level"] = tournament.SetupStage.ReyAllowed && !tournament.SetupStage.RenAllowed && !tournament.SetupStage.TanoAllowed ? "rey" :
                        !tournament.SetupStage.ReyAllowed && tournament.SetupStage.RenAllowed && !tournament.SetupStage.TanoAllowed ? "ren" : "mixed",
            ["completed"] = tournament.CurrentStage.IsTournamentCompleted,
            ["participants"] = new JsonArray([.. results.Placements.Select(p => SerializeStats(p))]),
            ["rounds"] = rounds
        };
        return node;
    }

    /// <summary>
    /// Serializes single action type
    /// </summary>
    private static string? SerializeActionType(ActionType type) => type switch
    {
        ActionType.Card => "T",
        ActionType.Clean => "C",
        ActionType.Conceded => "H",
        ActionType.Disarm => "D",
        ActionType.FirstContact => "F",
        ActionType.Headshot => "A",
        ActionType.Indirect => "I",
        ActionType.OutOfBounds => "O",
        ActionType.Penalty => "S",
        ActionType.Priority => "P",
        ActionType.HeadshotOverride => "V",
        _ => null,
    };

    /// <summary>
    /// Serializes actions
    /// </summary>
    private static IEnumerable<string> SerializeActions(IEnumerable<Lightspeed.Action> actions) => actions
        .Select(a => SerializeActionType(a.Type))
        .Where(s => s != null)!;

    /// <summary>
    /// Serializes bracket. Only completed rounds are serialized, so the resulting JSON may not contain all rounds in the bracket stage.
    /// Matches must be standard matches to be serialized.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static JsonObject SerializeBracket(BracketStageViewModel bracket)
    {
        int id = 1;
        var rounds = new JsonArray();
        TimeSpan duration = TimeSpan.Zero;
        int score = 0;
        foreach (var round in bracket.EnumerateGroups())
        {
            if (round.IsCompleted)
            {
                var matches = new JsonArray();
                for (int i = 0; i < round.Matches.Count; i++)
                {
                    StandardMatchViewModel match = round.Matches[i] as StandardMatchViewModel ?? throw new InvalidOperationException("Can only submit standard matches to SaberScore.");
                    var fencers = new JsonArray();
                    if (match.First.Participant is PlayerViewModel firstPlayer)
                    {
                        fencers.Add(new JsonObject
                        {
                            ["uuid"] = GetSaberSportId(firstPlayer),
                            ["score"] = match.First.Points,
                            ["is_winner"] = match.IsFirstWinner,
                            ["actions"] = new JsonArray([.. SerializeActions(match.FirstActions)])
                        });
                    }
                    if (match.Second.Participant is PlayerViewModel secondPlayer)
                    {
                        fencers.Add(new JsonObject
                        {
                            ["uuid"] = GetSaberSportId(secondPlayer),
                            ["score"] = match.Second.Points,
                            ["is_winner"] = match.IsSecondWinner,
                            ["actions"] = new JsonArray([.. SerializeActions(match.SecondActions)])
                        });
                    }

                    var matchNode = new JsonObject
                    {
                        ["id"] = i,
                        ["fencers"] = fencers
                    };
                    matches.Add(matchNode);
                }
                rounds.Add(new JsonObject
                {
                    ["id"] = id++,
                    ["matches"] = matches
                });

                // find longest match and highest score for the configuration
                if (round.Settings.TimeLimit > duration)
                    duration = round.Settings.TimeLimit;
                if (round.Settings.WinningScore > score)
                    score = round.Settings.WinningScore;
            }
        }

        var config = new JsonObject
        {
            ["name"] = "Direct Elimination",
            ["duration"] = duration.ToString("mm\\:ss"),
            ["score"] = score,
            ["disable_rank_promotion"] = false
        };

        var node = new JsonObject
        {
            ["type"] = "bracket",
            ["configuration"] = config,
            ["round"] = new JsonObject
            {
                ["rounds"] = rounds
            }
        };

        return node;
    }

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    private static JsonObject SerializeMatchSettings(MatchSettingsViewModel settings, string name, bool disablePromotion) => new()
    {
        ["name"] = name,
        ["duration"] = settings.TimeLimit.ToString("mm\\:ss"),
        ["score"] = settings.WinningScore,
        ["disable_rank_promotion"] = disablePromotion,
    };

    /// <summary>
    /// Serializes a participant. If the participant is a player, their information is serialized.
    /// If the participant is a team, an exception is thrown, as saber-sports does not support teams.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static JsonObject SerializeParticipant(ParticipantViewModel participant)
    {
        if (participant is PlayerViewModel player)
        {
            return new JsonObject
            {
                ["uuid"] = GetSaberSportId(player),
                ["first_name"] = player.FirstName,
                ["last_name"] = player.LastName,
                ["rating"] = player.Rank.ToString(),
                ["weapon"] = player.WeaponOfChoice switch
                {
                    WeaponClass.Rey => "rey",
                    WeaponClass.Ren => "ren",
                    WeaponClass.Tano => "tano",
                    _ => throw new InvalidOperationException("Invalid weapon class for saber-sports submission")
                },
                ["honor"] = player.Honor
            };
        }
        else
            throw new InvalidOperationException("Saber-sports does not support teams, so only players can be serialized as participants.");
    }

    /// <summary>
    /// Serializes a pool. The pool must have a match group with standard matches to be serialized successfully.
    /// The participants in the pool must be players, as teams are not supported by saber-sports.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static JsonObject SerializePool(PoolViewModel pool, int index)
    {
        JsonArray participantsNode = [];
        JsonArray scores = [];
        JsonArray actions = [];

        var players = pool.Squadron.Participants.OfType<PlayerViewModel>().ToList();

        // build the array of participants
        for (int i = 0; i < players.Count; i++)
        {
            participantsNode.Add(new JsonObject
            {
                ["uuid"] = GetSaberSportId(players[i]),
                ["index"] = i
            });
            scores.Add(new JsonArray([.. Enumerable.Repeat<string?>(null, players.Count)]));
        }

        // create a 2D array of scores and populate them from the matches
        foreach (StandardMatchViewModel match in pool.MatchGroup.Matches.OfType<StandardMatchViewModel>())
        {
            int blueIndex = players.IndexOf(match.First.Participant as PlayerViewModel ?? throw new InvalidOperationException("First participant is not a PlayerViewModel"));
            int redIndex = players.IndexOf(match.Second.Participant as PlayerViewModel ?? throw new InvalidOperationException("Second participant is not a PlayerViewModel"));
            if (blueIndex >= 0 && redIndex >= 0)
            {
                scores[blueIndex]![redIndex] = $"{(match.IsFirstWinner ? "V" : "")}{match.First.Points}";
                scores[redIndex]![blueIndex] = $"{(match.IsSecondWinner ? "V" : "")}{match.Second.Points}";

                actions.Add(new JsonArray([
                    new JsonObject
                    {
                        ["index"] = blueIndex,
                        ["actions"] = new JsonArray([.. SerializeActions(match.FirstActions)]),
                        ["clock"] = match.Clock.TimeRemaining.ToString("mm\\:ss")
                    },
                    new JsonObject
                    {
                        ["index"] = redIndex,
                        ["actions"] = new JsonArray([.. SerializeActions(match.SecondActions)]),
                        ["clock"] = match.Clock.TimeRemaining.ToString("mm\\:ss")
                    }
                ]));
            }
        }

        var node = new JsonObject
        {
            ["type"] = "pool",
            ["configuration"] = SerializeMatchSettings(pool.MatchGroup.Settings, pool.Squadron.Name, false),
            ["round"] = new JsonObject
            {
                ["index"] = index,
                ["participants"] = participantsNode,
                ["scores"] = scores,
                ["actions"] = actions
            }
        };

        return node;
    }

    /// <summary>
    /// Serializes a player's stats for submission to saber-sports. The participant must be a player, as teams are not supported by saber-sports.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static JsonObject SerializeStats(StatisticsViewModel playerStats)
    {
        PlayerViewModel player = playerStats.Participant as PlayerViewModel ?? throw new InvalidOperationException("Participant must be a player to submit to Saber Sport");
        var node = SerializeParticipant(player);
        node["place"] = playerStats.Place;
        return node;
    }

    /// <summary>
    /// Converts a player id
    /// </summary>
    private static string GetSaberSportId(PlayerViewModel player) => player.OnlineId.HasValue ? player.OnlineId.Value.ToString() : $"local{player.Guid.GetHashCode()}";
}
