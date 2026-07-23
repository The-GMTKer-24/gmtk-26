using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{

    public enum PlayerStat
    {
        Damage,
        BulletSpeed,
        TimeSteal,
        ReloadSpeed,
        MaxBullets,
        TimeBetweenShots,
        
    }

    public enum ModifierType
    {
        Additive,
        Multiplicative
    }
    
    [Serializable]
    public struct PlayerStatEntry
    {
        [SerializeField]
        private PlayerStat stat;

        [SerializeField]
        private float baseValue;

        [Tooltip("Round the evaluated value to a whole number.")]
        [SerializeField]
        private bool wholeNumber;

        public PlayerStat Stat
        {
            get { return stat; }
        }

        public float BaseValue
        {
            get { return baseValue; }
        }

        public bool WholeNumber
        {
            get { return wholeNumber; }
        }

        public PlayerStatEntry(
            PlayerStat stat,
            float baseValue,
            bool wholeNumber = false)
        {
            this.stat = stat;
            this.baseValue = baseValue;
            this.wholeNumber = wholeNumber;
        }

        internal void SetBaseValue(float value)
        {
            baseValue = value;
        }

        internal void SetWholeNumber(bool value)
        {
            wholeNumber = value;
        }
    }
    
    [Serializable]
    public sealed class PlayerStats
    {
        [SerializeField]
        private List<PlayerStatEntry> stats = new List<PlayerStatEntry>
        {
            new PlayerStatEntry(PlayerStat.Damage, 0f),
            new PlayerStatEntry(PlayerStat.ReloadSpeed, 0f),
            new PlayerStatEntry(PlayerStat.MaxBullets, 0f, true),
            new PlayerStatEntry(PlayerStat.TimeBetweenShots, 0f, true)
        };

        internal IReadOnlyList<PlayerStatEntry> Entries
        {
            get { return stats; }
        }

        internal bool Contains(PlayerStat stat)
        {
            return FindIndex(stat) >= 0;
        }

        internal bool TryGetEntry(
            PlayerStat stat,
            out PlayerStatEntry entry)
        {
            int index = FindIndex(stat);

            if (index < 0)
            {
                entry = default(PlayerStatEntry);
                return false;
            }

            entry = stats[index];
            return true;
        }

        internal bool Add(
            PlayerStat stat,
            float baseValue,
            bool wholeNumber)
        {
            if (Contains(stat))
            {
                return false;
            }

            stats.Add(
                new PlayerStatEntry(stat, baseValue, wholeNumber));

            return true;
        }

        internal bool Remove(PlayerStat stat)
        {
            int index = FindIndex(stat);

            if (index < 0)
            {
                return false;
            }

            stats.RemoveAt(index);
            return true;
        }

        internal bool SetBaseValue(PlayerStat stat, float value)
        {
            int index = FindIndex(stat);

            if (index < 0)
            {
                return false;
            }
            PlayerStatEntry entry = stats[index];
            entry.SetBaseValue(value);
            stats[index] = entry;

            return true;
        }

        internal bool SetWholeNumber(PlayerStat stat, bool wholeNumber)
        {
            int index = FindIndex(stat);

            if (index < 0)
            {
                return false;
            }

            PlayerStatEntry entry = stats[index];
            entry.SetWholeNumber(wholeNumber);
            stats[index] = entry;

            return true;
        }

        internal bool TryFindDuplicate(out PlayerStat duplicate)
        {
            HashSet<PlayerStat> found = new HashSet<PlayerStat>();

            for (int i = 0; i < stats.Count; i++)
            {
                if (!found.Add(stats[i].Stat))
                {
                    duplicate = stats[i].Stat;
                    return true;
                }
            }

            duplicate = default(PlayerStat);
            return false;
        }

        private int FindIndex(PlayerStat stat)
        {
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].Stat == stat)
                {
                    return i;
                }
            }

            return -1;
        }
    }
    
    public sealed class Modifier
    {
        public ModifierType Type { get; private set; }

        public float Factor { get; private set; }
        
        public object Source { get; private set; }

        public Modifier(
            ModifierType type,
            float factor,
            object source = null)
        {
            if (float.IsNaN(factor) || float.IsInfinity(factor))
            {
                throw new ArgumentOutOfRangeException(
                    "factor",
                    "A modifier factor must be a finite number.");
            }

            Type = type;
            Factor = factor;
            Source = source;
        }
    }

    public sealed class PlayerModifier : MonoBehaviour
    {
        private static readonly Modifier[] EmptyModifiers =
            new Modifier[0];

        [Header("Player Base Stats")]
        [SerializeField]
        private PlayerStats baseStats = new PlayerStats();

        [NonSerialized]
        private Dictionary<PlayerStat, List<Modifier>> modifiers;

        private void Awake()
        {
            EnsureInitialized();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureInitialized();

            PlayerStat duplicate;

            if (baseStats.TryFindDuplicate(out duplicate))
            {
                Debug.LogWarning(
                    "PlayerStats contains more than one entry for " +
                    duplicate +
                    ". Only the first entry will be used.",
                    this);
            }
        }
#endif
        
        public bool HasStat(PlayerStat stat)
        {
            EnsureInitialized();
            return baseStats.Contains(stat);
        }
        
        public bool AddStat(
            PlayerStat stat,
            float baseValue,
            bool wholeNumber = false)
        {
            EnsureInitialized();

            return baseStats.Add(
                stat,
                baseValue,
                wholeNumber);
        }
        
        public bool RemoveStat(PlayerStat stat)
        {
            EnsureInitialized();

            bool removed = baseStats.Remove(stat);

            if (removed)
            {
                modifiers.Remove(stat);
            }

            return removed;
        }

        public float GetBaseValue(PlayerStat stat)
        {
            EnsureInitialized();

            PlayerStatEntry entry;

            if (!baseStats.TryGetEntry(stat, out entry))
            {
                throw CreateMissingStatException(stat);
            }

            return entry.BaseValue;
        }

        public bool TryGetBaseValue(
            PlayerStat stat,
            out float baseValue)
        {
            EnsureInitialized();

            PlayerStatEntry entry;

            if (!baseStats.TryGetEntry(stat, out entry))
            {
                baseValue = 0f;
                return false;
            }

            baseValue = entry.BaseValue;
            return true;
        }

        public void SetBaseValue(PlayerStat stat, float value)
        {
            EnsureInitialized();

            if (!baseStats.SetBaseValue(stat, value))
            {
                throw CreateMissingStatException(stat);
            }
        }

        public void SetWholeNumber(
            PlayerStat stat,
            bool wholeNumber)
        {
            EnsureInitialized();

            if (!baseStats.SetWholeNumber(stat, wholeNumber))
            {
                throw CreateMissingStatException(stat);
            }
        }
        
        public Modifier AddModifier(
            PlayerStat stat,
            ModifierType type,
            float factor,
            object source = null)
        {
            Modifier modifier = new Modifier(
                type,
                factor,
                source);

            AddModifier(stat, modifier);
            return modifier;
        }

        public void AddModifier(
            PlayerStat stat,
            Modifier modifier)
        {
            EnsureInitialized();

            if (modifier == null)
            {
                throw new ArgumentNullException("modifier");
            }

            if (!baseStats.Contains(stat))
            {
                throw CreateMissingStatException(stat);
            }

            GetOrCreateModifierList(stat).Add(modifier);
        }
        
        public bool RemoveModifier(
            PlayerStat stat,
            Modifier modifier)
        {
            EnsureInitialized();

            if (modifier == null)
            {
                return false;
            }

            List<Modifier> statModifiers;

            if (!modifiers.TryGetValue(stat, out statModifiers))
            {
                return false;
            }

            bool removed = statModifiers.Remove(modifier);

            if (statModifiers.Count == 0)
            {
                modifiers.Remove(stat);
            }

            return removed;
        }
        
        public int RemoveModifiersFromSource(object source)
        {
            EnsureInitialized();

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int removedCount = 0;

            foreach (
                KeyValuePair<PlayerStat, List<Modifier>> pair
                in modifiers)
            {
                removedCount += pair.Value.RemoveAll(
                    delegate(Modifier modifier)
                    {
                        return Equals(modifier.Source, source);
                    });
            }

            RemoveEmptyModifierLists();
            return removedCount;
        }

        public void ClearModifiers(PlayerStat stat)
        {
            EnsureInitialized();
            modifiers.Remove(stat);
        }

        public void ClearAllModifiers()
        {
            EnsureInitialized();
            modifiers.Clear();
        }
        
        public IReadOnlyList<Modifier> GetModifiers(PlayerStat stat)
        {
            EnsureInitialized();

            List<Modifier> statModifiers;

            if (!modifiers.TryGetValue(stat, out statModifiers))
            {
                return EmptyModifiers;
            }

            return statModifiers;
        }
        
        public float Evaluate(PlayerStat stat)
        {
            float value;

            if (!TryEvaluate(stat, out value))
            {
                throw CreateMissingStatException(stat);
            }

            return value;
        }

        public bool TryEvaluate(
            PlayerStat stat,
            out float value)
        {
            EnsureInitialized();

            PlayerStatEntry entry;

            if (!baseStats.TryGetEntry(stat, out entry))
            {
                value = 0f;
                return false;
            }

            float additiveTotal = 0f;
            float multiplierTotal = 1f;

            List<Modifier> statModifiers;

            if (modifiers.TryGetValue(stat, out statModifiers))
            {
                for (int i = 0; i < statModifiers.Count; i++)
                {
                    Modifier modifier = statModifiers[i];

                    switch (modifier.Type)
                    {
                        case ModifierType.Additive:
                            additiveTotal += modifier.Factor;
                            break;

                        case ModifierType.Multiplicative:
                            multiplierTotal *= modifier.Factor;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            value =
                (entry.BaseValue + additiveTotal) *
                multiplierTotal;

            if (entry.WholeNumber)
            {
                value = Mathf.Round(value);
            }

            return true;
        }

        public int EvaluateInt(PlayerStat stat)
        {
            return Mathf.RoundToInt(Evaluate(stat));
        }
        
        public Dictionary<PlayerStat, float> EvaluateAll()
        {
            EnsureInitialized();

            Dictionary<PlayerStat, float> result =
                new Dictionary<PlayerStat, float>(
                    baseStats.Entries.Count);

            for (int i = 0; i < baseStats.Entries.Count; i++)
            {
                PlayerStat stat = baseStats.Entries[i].Stat;


                if (!result.ContainsKey(stat))
                {
                    result.Add(stat, Evaluate(stat));
                }
            }

            return result;
        }
        
        private void EnsureInitialized()
        {
            if (baseStats == null)
            {
                baseStats = new PlayerStats();
            }

            if (modifiers == null)
            {
                modifiers =
                    new Dictionary<PlayerStat, List<Modifier>>();
            }
        }

        private List<Modifier> GetOrCreateModifierList(
            PlayerStat stat)
        {
            List<Modifier> statModifiers;

            if (!modifiers.TryGetValue(stat, out statModifiers))
            {
                statModifiers = new List<Modifier>();
                modifiers.Add(stat, statModifiers);
            }

            return statModifiers;
        }

        private void RemoveEmptyModifierLists()
        {
            List<PlayerStat> emptyStats =
                new List<PlayerStat>();

            foreach (
                KeyValuePair<PlayerStat, List<Modifier>> pair
                in modifiers)
            {
                if (pair.Value.Count == 0)
                {
                    emptyStats.Add(pair.Key);
                }
            }

            for (int i = 0; i < emptyStats.Count; i++)
            {
                modifiers.Remove(emptyStats[i]);
            }
        }

        private static KeyNotFoundException
            CreateMissingStatException(PlayerStat stat)
        {
            return new KeyNotFoundException(
                "The stat '" +
                stat +
                "' has not been configured in PlayerStats.");
        }
    }
}