using System;
using System.Collections.Generic;
using System.Text;

public class GameDebugRegistry
{
    private static List<Func<DebugSection>> providers
           = new List<Func<DebugSection>>();

    public static void Register(Func<DebugSection> provider)
    {
        providers.Add(provider);
    }

    public static void Unregister(Func<DebugSection> provider)
    {
        providers.Remove(provider);
    }

    public static List<DebugSection> GetSections()
    {
        List<DebugSection> sections = new List<DebugSection>();

        foreach (var provider in providers)
        {
            sections.Add(provider.Invoke());
        }

        return sections;
    }
}

public class DebugSection
{
    public string Title;
    public Func<string> ContentProvider;  // evaluated every draw
    public List<DebugSection> Children = new List<DebugSection>();

    public DebugSection(string title, Func<string> provider = null)
    {
        Title = title;
        ContentProvider = provider;
    }
}
