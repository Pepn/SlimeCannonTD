using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

/// <summary>
/// Removes roselyn analyzer warnings that for some reason check the unity engine code
/// </summary>
public class RemoveUnitySourceGeneratorsWarning : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        var document = XDocument.Parse(content);
        document.Root.Descendants()
            .Where(x => x.Name.LocalName == "Analyzer")
            .Where(x => x.Attribute("Include").Value.Contains("Unity.SourceGenerators"))
            .Remove();
        return document.Declaration + System.Environment.NewLine + document.Root;
    }
}