using System.Collections.Generic;
using System.Linq;

namespace WordHookBotPlus
{
    class Guild
    {
        public string Id { get; set; }
        public Dictionary<string, List<ulong>> Hooks { get; set; }
        public Exclusion Exclusions { get; set; }

        public Guild(string id)
        {
            Hooks = new Dictionary<string, List<ulong>>();
            Exclusions = new Exclusion(id);
            Id = id;
        }

        public bool AddHook(string hook, ulong id)
        {
            if (!Hooks.TryGetValue(hook, out List<ulong> current)) current = new List<ulong>();
            if (current.Contains(id)) return false;
            current.Add(id);
            Hooks[hook] = current;
            return true;
        }

        public bool RemoveHook(string hook, ulong id)
        {
            if (!Hooks.ContainsKey(hook)) return false;
            if (!Hooks[hook].Contains(id)) return false;
            Hooks[hook].Remove(id);
            return true;
        }

        public IEnumerable<string> GetHooksForUser(ulong id)
        {
            foreach (KeyValuePair<string, List<ulong>> pair in Hooks)
            {
                if (pair.Value.Contains(id)) yield return pair.Key;
            }
        }

        public int CountHooks(ulong id) => Hooks.Values.Count(e => e.Contains(id));
    }
}