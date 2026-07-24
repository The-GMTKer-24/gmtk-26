using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class GnomeTracker : MonoBehaviour
    {
        public static GnomeTracker Instance;
        
        private static Dictionary<String, SortedSet<EntityId>> _gnomeDict = new();

        public void Awake()
        {
            Instance = this;
        }
        
        public bool IsGnomeOfType(EntityId id, String type)
        {
            return _gnomeDict.ContainsKey(type) && _gnomeDict[type].Contains(id);
        }

        public bool IsGnome(EntityId id)
        {
            return IsGnomeOfType(id, null);
        }

        public void AddGnome(EntityId id, HashSet<String> types)
        {
            HashSet<String> typesCopy = new HashSet<String>(types);
            typesCopy.Add(null);

            foreach (String type in typesCopy)
            {
                if (!_gnomeDict.ContainsKey(type)) _gnomeDict.Add(type, new SortedSet<EntityId>());
                _gnomeDict[type].Add(id);
            }
        }

        public void AddGnome(EntityId id)
        {
            AddGnome(id, new HashSet<String>());
        }

        public void RemoveGnome(EntityId id)
        {
            // Possibly a source of performance issues. Better to try every possible tag, or to store tags per entityid in another dict?
            foreach (KeyValuePair<String, SortedSet<EntityId>> entry in _gnomeDict)
            {
                entry.Value.Remove(id);
            }
        }
    }
}