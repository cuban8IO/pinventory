namespace Pinventory.Data;

public record TreeEntry<T>(T Node, int Depth, string Path) where T : ITreeNode;

public static class Tree
{
    public const string Separator = " › ";

    /// <summary>Sortiert die Knoten als Baum (Eltern vor Kindern, alphabetisch) mit Tiefe und vollem Pfad.</summary>
    public static List<TreeEntry<T>> Flatten<T>(IEnumerable<T> nodes) where T : ITreeNode
    {
        var byParent = nodes.ToLookup(n => n.ParentId);
        var result = new List<TreeEntry<T>>();

        void Walk(int? parentId, int depth, string prefix)
        {
            foreach (var node in byParent[parentId].OrderBy(n => n.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var path = prefix.Length == 0 ? node.Name : prefix + Separator + node.Name;
                result.Add(new TreeEntry<T>(node, depth, path));
                Walk(node.Id, depth + 1, path);
            }
        }

        Walk(null, 0, "");
        return result;
    }

    /// <summary>Id des Knotens und aller Unterknoten.</summary>
    public static HashSet<int> SelfAndDescendants<T>(IEnumerable<T> nodes, int id) where T : ITreeNode
    {
        var byParent = nodes.ToLookup(n => n.ParentId);
        var result = new HashSet<int>();
        var stack = new Stack<int>([id]);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!result.Add(current)) continue;
            foreach (var child in byParent[current]) stack.Push(child.Id);
        }
        return result;
    }

    public static Dictionary<int, string> Paths<T>(IEnumerable<T> nodes) where T : ITreeNode =>
        Flatten(nodes).ToDictionary(e => e.Node.Id, e => e.Path);
}
