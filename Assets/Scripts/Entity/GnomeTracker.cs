using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
            Instance = this;
        }

        public void FixedUpdate()
        {
            //cycleIndex++; cycleIndex %= CycleSize;
        }
        
        public bool IsGnomeOfTag(GnomeAI gnomeAI, String tag)
        {
            return _gnomeDict.ContainsKey(tag) && _gnomeDict[tag].Contains(gnomeAI.GetEntityId());
        }

        public bool IsGnome(GnomeAI gnomeAI)
        {
            return _gnomeSet.Contains(gnomeAI.GetEntityId());
        }

        public void AddGnome(GnomeAI gnomeAI, HashSet<String> tags)
        {
            _gnomeSet.Add(gnomeAI.GetEntityId());
            _gnomes.Add(gnomeAI.GetEntityId(), gnomeAI);
            
            foreach (String gnomeTag in tags)
            {
                if (!_gnomeDict.ContainsKey(gnomeTag)) _gnomeDict.Add(gnomeTag, new SortedSet<EntityId>());
                _gnomeDict[gnomeTag].Add(gnomeAI.GetEntityId());
            }
            
            gnomeCount = _gnomeSet.Count;
        }

        public void AddGnome(GnomeAI gnomeAI)
        {
            AddGnome(gnomeAI, new HashSet<String>());
        }

        public void RemoveGnome(GnomeAI gnomeAI)
        {
            // Possibly a source of performance issues. Better to try every possible tag, or to store tags per entityid in another dict?
            foreach (KeyValuePair<String, SortedSet<EntityId>> entry in _gnomeDict)
            {
                entry.Value.Remove(gnomeAI.GetEntityId());
            }
            
            _gnomeSet.Remove(gnomeAI.GetEntityId());
            _gnomes.Remove(gnomeAI.GetEntityId());
            
            gnomeCount = _gnomeSet.Count;
        }

        public SortedSet<EntityId> GetGnomeIds(String tag)
        {
            return _gnomeDict[tag];
        }

        public SortedSet<EntityId> GetGnomeIds()
        {
            return _gnomeSet;
        }

        /*public HashSet<GnomeAI> GetGnomes()
        {
            return _gnomes;
        }*/

        public IEnumerable<GnomeAI> GetGnomeEnumerator()
        {
            return _gnomes.Values.AsEnumerable();
        }

        public GnomeAI GetGnome(EntityId entityId)
        {
            return _gnomes.ContainsKey(entityId) ? _gnomes[entityId] : null;
        }

        public bool DoesGnomeExist(EntityId entityId)
        {
            return _gnomes.ContainsKey(entityId);
        }
    }
}