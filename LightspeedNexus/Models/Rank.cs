using System;

namespace LightspeedNexus.Models;

/// <summary>
/// A player's rank
/// </summary>
public readonly struct Rank : IComparable<Rank>
{
    /// <summary>
    /// The rank, repesented by a single letter
    /// </summary>
    private readonly char _rank;

    /// <summary>
    /// The rank's value
    /// </summary>
    public int Value => _rank switch
    {
        'A' => 50,
        'B' => 40,
        'C' => 30,
        'D' => 20,
        'E' => 10,
        _ => 1
    };

    /// <summary>
    /// The letter representation of the rank
    /// </summary>
    public char Letter => _rank;

    /// <summary>
    /// Contruct from character
    /// </summary>
    public Rank(char rank)
    {
        _rank = rank switch
        {
            'a' or 'A' => 'A',
            'b' or 'B' => 'B',
            'c' or 'C' => 'C',
            'd' or 'D' => 'D',
            'e' or 'E' => 'E',
            _ => 'U',
        };
    }

    /// <summary>
    /// Contruct from string, using only first letter
    /// </summary>
    public Rank(string? rank)
    {
        _rank = 'U';
        if (!string.IsNullOrEmpty(rank))
        {
            var r = rank.ToUpperInvariant();
            if (r[0] == 'A' || r[0] == 'B' || r[0] == 'C' || r[0] == 'D' || r[0] == 'E')
                _rank = r[0];
        }
    }

    /// <summary>
    /// Converts a character to a rank.
    /// </summary>
    public static implicit operator Rank(char rank) => new(rank);

    /// <summary>
    /// Converts a string to a rank.
    /// </summary>
    public static implicit operator Rank(string? rank) => new(rank);

    /// <summary>
    /// Converts a rank to a character
    /// </summary>
    public static implicit operator char(Rank rank) => rank._rank;

    /// <summary>
    /// Converts a rank to a string
    /// </summary>
    public override string ToString() => _rank.ToString();

    /// <summary>
    /// Compares ranks
    /// </summary>
    public override bool Equals(object? obj) => obj is Rank rank && _rank == rank._rank;

    /// <summary>
    /// Hashes a rank
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(_rank);

    /// <summary>
    /// True if ranks are the same
    /// </summary>
    public static bool operator ==(Rank left, Rank right) => left._rank == right._rank;

    /// <summary>
    /// True if ranks are different
    /// </summary>
    public static bool operator !=(Rank left, Rank right) => !(left == right);

    /// <summary>
    /// True if left rank is less than the right rank
    /// </summary>
    public static bool operator <(Rank left, Rank right) => left.Value < right.Value;

    /// <summary>
    /// True if the left rank is greater than the right rank
    /// </summary>
    public static bool operator >(Rank left, Rank right) => left.Value > right.Value;

    /// <summary>
    /// True if the left rank is less than or equal to the right rank
    /// </summary>
    public static bool operator <=(Rank left, Rank right) => left.Value <= right.Value;

    /// <summary>
    /// True if the left rank is greater than or equal to the right rank
    /// </summary>
    public static bool operator >=(Rank left, Rank right) => left.Value >= right.Value;

    /// <summary>
    /// Increases the rank by one level, up to A.
    /// </summary>
    public static Rank operator ++(Rank rank) => rank._rank switch
    {
        'U' => E,
        'E' => D,
        'D' => C,
        'C' => B,
        _ => A
    };

    /// <summary>
    /// Decreases the rank by one level, down to U
    /// </summary>
    public static Rank operator --(Rank rank) => rank._rank switch
    {
        'A' => B,
        'B' => C,
        'C' => D,
        'D' => E,
        _ => U
    };

    /// <summary>
    /// Compares ranks
    /// </summary>
    public int CompareTo(Rank other) => Value - other.Value;

    /// <summary>
    /// Combines one's highest rank with another rank, returning a
    /// somewhat average of the two.
    /// </summary>
    public static Rank Combine(Rank highest, Rank weapon)
    {
        if (weapon > highest)
            return weapon;

        switch (highest._rank)
        {
            case 'A': return weapon._rank switch
                {
                    'A' or 'B' => A,
                    'C' or 'D' => B,
                    _ => C
                };
            case 'B':
                return weapon._rank switch
                {
                    'B' or 'C' => B,
                    'D' or 'E' => C,
                    _ => D
                };
            case 'C':
                return weapon._rank switch
                {
                    'C' => C,
                    'D' or 'E' => D,
                    _ => E
                };
            case 'D':
                return weapon._rank switch
                {
                    'D' => D,
                    'E' => E,
                    _ => U
                };
            default: return U;
        }
    }

    public static readonly Rank A = new('A');
    public static readonly Rank B = new('B');
    public static readonly Rank C = new('C');
    public static readonly Rank D = new('D');
    public static readonly Rank E = new('E');
    public static readonly Rank U = new('U');
}
