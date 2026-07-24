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
        
        private Dictionary<String, SortedSet<EntityId>> _gnomeDict = new();
        private Dictionary<EntityId, GnomeAI> _gnomes = new();

        public void Awake()
        {
            Instance = this;
        }
        
        public bool IsGnomeOfTag(GnomeAI gnomeAI, String tag)
        {
            return _gnomeDict.ContainsKey(tag) && _gnomeDict[tag].Contains(gnomeAI.GetEntityId());
        }

        public bool IsGnome(GnomeAI gnomeAI)
        {
            return IsGnomeOfTag(gnomeAI, null);
        }

        public void AddGnome(GnomeAI gnomeAI, HashSet<String> tags)
        {
            HashSet<String> tagsCopy = new HashSet<String>(tags);
            tagsCopy.Add(null);

            foreach (String tag in tagsCopy)
            {
                if (!_gnomeDict.ContainsKey(tag)) _gnomeDict.Add(tag, new SortedSet<EntityId>());
                _gnomeDict[tag].Add(gnomeAI.GetEntityId());
            }
            
            _gnomes.Add(gnomeAI.GetEntityId(), gnomeAI);
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
            
            _gnomes.Remove(gnomeAI.GetEntityId());
        }

        public SortedSet<EntityId> GetGnomeIds(String tag)
        {
            return _gnomeDict[tag];
        }

        public SortedSet<EntityId> GetGnomeIds()
        {
            return GetGnomeIds(null);
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