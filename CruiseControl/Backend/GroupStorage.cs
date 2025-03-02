using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CruiseControl
{
    public class GroupStorage
    {
        private string filePath;

        public GroupStorage(string filePath)
        {
            this.filePath = filePath;
        }

        public void CreateGroup(string groupName, string createdByUsername)
        {
            List<Group> groups = LoadGroups();
            groups.Add(new Group { GroupName = groupName, CreatedBy = createdByUsername, Members = new List<string> { createdByUsername } }); // Added Members list

            string jsonString = JsonSerializer.Serialize(groups);
            File.WriteAllText(filePath, jsonString);
        }

        public List<Group> LoadGroups()
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                try
                {
                    return JsonSerializer.Deserialize<List<Group>>(jsonString) ?? new List<Group>();
                }
                catch (JsonException)
                {
                    return new List<Group>();
                }
            }
            return new List<Group>();
        }

        public class Group
        {
            public string GroupName { get; set; }
            public string CreatedBy { get; set; }
            public List<string> Members { get; set; } // List of usernames in the group
        }
    }
}