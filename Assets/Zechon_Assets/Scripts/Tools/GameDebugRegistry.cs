using System;
using System.Collections.Generic;
using System.Text;

public class GameDebugRegistry
{
    private static Dictionary<string, Func<DebugSection>> providers
            = new Dictionary<string, Func<DebugSection>>();

    public static void Register(string key, Func<DebugSection> provider)
    {
        providers[key] = provider;
    }

    public static void Unregister(string key)
    {
        providers.Remove(key);
    }

    public static List<DebugSection> BuildSnapshot()
    {
        List<DebugSection> sections = new List<DebugSection>();

        foreach (var pair in providers)
        {
            try
            {
                sections.Add(pair.Value.Invoke());
            }
            catch (Exception e)
            {
                sections.Add(new DebugSection(pair.Key + "_error", pair.Key + " Error", e.Message));
            }
        }

        sections.Sort((a, b) => a.Order.CompareTo(b.Order));

        return sections;
    }
}

public class DebugSection
{
    public string Id;
    public string Title;
    public string Content;
    public int Order;
    public List<DebugSection> Children = new List<DebugSection>();

    public DebugSection(string id, string title, string content = "", int order = 0)
    {
        Id = id;
        Title = title;
        Content = content;
        Order = order;
    }
}
