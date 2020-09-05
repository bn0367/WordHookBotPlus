using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WordHookBotPlus
{
    [Serializable]
    internal class Exclusion : ISerializable
    {
        public string Id { get; set; }

        public HashSet<ulong> Channels { get; set; }

        public Exclusion(string id)
        {
            Channels = new HashSet<ulong>();
            Id = id;
        }

        //parameterless constructor for json deserialization
        public Exclusion()
        {
            
        }

        public Exclusion(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetString("id");
            Channels = info.GetValue("channels", typeof(HashSet<ulong>)) as HashSet<ulong>;
        }

        public bool IsExcluded(ulong channel)
        {
            return Channels.Contains(channel);
        }

        public bool AddExclusion(ulong channel)
        {
            return Channels.Add(channel);
        }

        public bool RemoveExclusion(ulong channel)
        {
            return Channels.Remove(channel);
        }

        public async Task<string> PrintExclusions()
        {
            var id = 1;
            var ret = $"{(await Program.Discord.GetGuildAsync(ulong.Parse(Id))).Name}'s excluded channels: \n```";
            foreach (var c in Channels)
            {
                ret += $"{id++}: #{(await Program.Discord.GetChannelAsync(c)).Name}\n";
            }

            return ret + "\n```";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", Id);
            info.AddValue("channels", Channels);
        }
    }
}