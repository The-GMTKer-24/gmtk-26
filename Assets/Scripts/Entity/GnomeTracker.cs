using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class GnomeTracker : MonoBehaviour
    {
        public static GnomeTracker Instance;
        
        [SerializeField] public int gnomeCount; // Readout only
        
        private SortedSet<EntityId> _gnomeSet = new SortedSet<EntityId>();
        private Dictionary<String, SortedSet<EntityId>> _gnomeDict = new();
        private Dictionary<EntityId, GnomeAI> _gnomes = new();

        //public const int CycleSize = 4;
        //public int cycleIndex = 0;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError(
                    "Multiple GnomeTracker instances are active. The newer instance was disabled.",
                    this
                );
                enabled = false;
                return;
            }

            Instance = this;
        }

        public void FixedUpdate()
        {
            //cycleIndex++; cycleIndex %= CycleSize;
        }
        
        public bool IsGnomeOfTag(GnomeAI gnomeAI, String tag)
        {
            return gnomeAI != null &&
                   tag != null &&
                   _gnomeDict.TryGetValue(tag, out SortedSet<EntityId> ids) &&
                   ids.Contains(gnomeAI.gameObject.GetEntityId());
        }

        public bool IsGnome(GnomeAI gnomeAI)
        {
            return gnomeAI != null &&
                   _gnomeSet.Contains(gnomeAI.gameObject.GetEntityId());
        }

        public void AddGnome(GnomeAI gnomeAI, HashSet<String> tags)
        {
            if (gnomeAI == null)
            {
                return;
            }

            EntityId entityId = gnomeAI.gameObject.GetEntityId();
            _gnomeSet.Add(entityId);
            _gnomes[entityId] = gnomeAI;
            
            if (tags != null)
            {
                foreach (String gnomeTag in tags)
                {
                    if (String.IsNullOrEmpty(gnomeTag))
                    {
                        continue;
                    }

                    if (!_gnomeDict.TryGetValue(
                            gnomeTag,
                            out SortedSet<EntityId> taggedIds
                        ))
                    {
                        taggedIds = new SortedSet<EntityId>();
                        _gnomeDict.Add(gnomeTag, taggedIds);
                    }

                    taggedIds.Add(entityId);
                }
            }
            
            gnomeCount = _gnomeSet.Count;
        }

        public void AddGnome(GnomeAI gnomeAI)
        {
            AddGnome(gnomeAI, new HashSet<String>());
        }

        public void RemoveGnome(GnomeAI gnomeAI)
        {
            // Unity objects can compare equal to null while OnDestroy is
            // running, but their managed reference is still usable here.
            if (ReferenceEquals(gnomeAI, null))
            {
                return;
            }

            EntityId entityId = gnomeAI.gameObject.GetEntityId();

            // Possibly a source of performance issues. Better to try every possible tag, or to store tags per entityid in another dict?
            foreach (KeyValuePair<String, SortedSet<EntityId>> entry in _gnomeDict)
            {
                entry.Value.Remove(entityId);
            }
            
            _gnomeSet.Remove(entityId);
            _gnomes.Remove(entityId);
            
            gnomeCount = _gnomeSet.Count;
        }

        public SortedSet<EntityId> GetGnomeIds(String tag)
        {
            return tag != null &&
                   _gnomeDict.TryGetValue(tag, out SortedSet<EntityId> ids)
                ? new SortedSet<EntityId>(ids)
                : new SortedSet<EntityId>();
        }

        public SortedSet<EntityId> GetGnomeIds()
        {
            return new SortedSet<EntityId>(_gnomeSet);
        }

        /*public HashSet<GnomeAI> GetGnomes()
        {
            return _gnomes;
        }*/

        public Dictionary<EntityId, GnomeAI>.ValueCollection
            GetGnomeEnumerator()
        {
            return _gnomes.Values;
        }

        public GnomeAI GetGnome(EntityId entityId)
        {
            return _gnomes.TryGetValue(entityId, out GnomeAI gnome)
                ? gnome
                : null;
        }

        public bool DoesGnomeExist(EntityId entityId)
        {
            return _gnomes.ContainsKey(entityId);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
