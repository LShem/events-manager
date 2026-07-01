using System.Xml.Linq;

namespace EventsManager.UnitTests.Architecture;

/// <summary>
/// Garde-fou au niveau des &lt;ProjectReference&gt; des .csproj (graphe de projets),
/// complémentaire des <see cref="ArchitectureTests"/> qui, eux, vérifient les dépendances
/// de types au niveau IL.
///
/// Utilité propre : interdire une référence de projet *directe* qui ne devrait pas exister,
/// même quand l'usage transitif des types est légitime. Exemple : Api utilise des types du
/// Domain à travers Application (donc NetArchTest ne peut pas interdire la dépendance de type
/// Api → Domain), mais une référence de projet *directe* Api → Domain est un saut de couche
/// interdit — c'est exactement ce que ce test attrape.
/// </summary>
public class ProjectReferenceTests
{
    // Couche → projets qu'elle a le droit de référencer directement (cf. CLAUDE.md).
    private static readonly Dictionary<string, string[]> AllowedReferences = new()
    {
        ["EventsManager.Domain"] = [],
        ["EventsManager.Application"] = ["EventsManager.Domain"],
        ["EventsManager.Infrastructure"] = ["EventsManager.Application"],
        ["EventsManager.Api"] = ["EventsManager.Application", "EventsManager.Infrastructure"],
    };

    [Theory]
    [InlineData("EventsManager.Domain")]
    [InlineData("EventsManager.Application")]
    [InlineData("EventsManager.Infrastructure")]
    [InlineData("EventsManager.Api")]
    public void Project_MustOnlyReference_AllowedProjects(string project)
    {
        var allowed = AllowedReferences[project];
        var actual = GetProjectReferences(project);

        var forbidden = actual.Where(reference => !allowed.Contains(reference)).ToArray();

        forbidden.Should().BeEmpty(
            $"{project} ne doit référencer que [{string.Join(", ", allowed)}]. " +
            $"Références de projet interdites détectées : {string.Join(", ", forbidden)}");
    }

    private static string[] GetProjectReferences(string project)
    {
        var folder = project.Replace("EventsManager.", string.Empty);
        var csproj = Path.Combine(SolutionRoot(), "src", folder, $"{project}.csproj");
        var document = XDocument.Load(csproj);

        return document.Descendants("ProjectReference")
                       .Select(element => element.Attribute("Include")!.Value)
                       .Select(path => Path.GetFileNameWithoutExtension(path))
                       .ToArray();
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && directory.GetFiles("events-manager.slnx").Length == 0)
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException(
                "Racine de la solution introuvable (events-manager.slnx non trouvé en remontant les dossiers).");
        }

        return directory.FullName;
    }
}
