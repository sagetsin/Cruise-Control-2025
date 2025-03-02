using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace CruiseControl
{
    public class GroupStorage
    {
        private string filePath;
        private Random random = new Random();

        public GroupStorage(string filePath)
        {
            this.filePath = filePath;
        }

        public (bool success, string message, string groupCode) CreateGroup(string groupName, string createdByUsername)
        {
            List<Group> groups = LoadGroups();

            if (groups.Any(g => g.CreatedBy == createdByUsername && g.Members.Count > 6))
            {
                return (false, "You cannot create more than 6 groups.", null);
            }

            string groupCode = GenerateGroupCode();
            groups.Add(new Group { GroupName = groupName, CreatedBy = createdByUsername, Members = new List<string> { createdByUsername }, GroupCode = groupCode });

            string jsonString = JsonSerializer.Serialize(groups);
            File.WriteAllText(filePath, jsonString);

            return (true, "Group created successfully!", groupCode);
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

        public List<Group> GetGroupsForUser(string username)
        {
            List<Group> groups = LoadGroups();
            return groups.Where(g => g.Members.Contains(username)).ToList();
        }

        public void DeleteGroup(string groupName, string username)
        {
            List<Group> groups = LoadGroups();
            groups.RemoveAll(g => g.GroupName == groupName && g.CreatedBy == username);

            string jsonString = JsonSerializer.Serialize(groups);
            File.WriteAllText(filePath, jsonString);
        }

        public Group GetGroup(string groupName)
        {
            List<Group> groups = LoadGroups();
            return groups.FirstOrDefault(g => g.GroupName == groupName);
        }
        public (bool success, string message) JoinGroup(string groupCode, string username)
        {
            List<Group> groups = LoadGroups();
            Group group = groups.FirstOrDefault(g => g.GroupCode == groupCode);

            if (group == null)
            {
                return (false, "Invalid group code.");
            }

            if (group.Members.Count >= 6)
            {
                return (false, "Group is full.");
            }

            if (group.Members.Contains(username))
            {
                return (false, "You are already a member of this group.");
            }

            if (groups.Count(g => g.Members.Contains(username)) >= 6)
            {
                return (false, "You cannot join more than 6 groups.");
            }

            group.Members.Add(username);
            string jsonString = JsonSerializer.Serialize(groups);
            File.WriteAllText(filePath, jsonString);

            return (true, "Joined group successfully.");
        }

        private string GenerateGroupCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void LeaveGroup(string groupName, string username)
        {
            List<Group> groups = LoadGroups();
            Group group = groups.FirstOrDefault(g => g.GroupName == groupName);

            if (group == null || !group.Members.Contains(username))
            {
                return; // Group not found or user not a member
            }

            if (group.CreatedBy == username)
            {
                if (group.Members.Count > 1)
                {
                    // Pass ownership to the last member
                    group.CreatedBy = group.Members[1];
                }
                else
                {
                    // If the creator is the only member, delete the group
                    DeleteGroup(groupName, username);
                    return;
                }
            }

            group.Members.Remove(username);
            string jsonString = JsonSerializer.Serialize(groups);
            File.WriteAllText(filePath, jsonString);
        }

        public class Group
        {
            public string GroupName { get; set; }
            public string CreatedBy { get; set; }
            public List<string> Members { get; set; } // List of usernames in the group
            public string GroupCode { get; set; }
        }
    }
}