using System.Text;

namespace AzureDataFactoryJsonInterpreter
{
    internal class OutputFunctions
    {
        internal static void GenerateMarkdownDocumentation(List<ADFNode> nodes, string outputFolder) {

            StringBuilder sb = new StringBuilder();
            foreach (ADFNode node in nodes) {
                sb.Append("## ");
                sb.Append(node.Name);
                sb.Append(" (");
                sb.Append(node.NodeType);
                sb.AppendLine(")");
            }
            File.WriteAllText(Path.Combine(outputFolder, "index.md"), sb.ToString());
        }

        internal static void GenerateMermaidFlowChart(List<ADFNode> nodes, string outputFolder) {

            // Put all nodes in a dictionary by name and assign unique IDs.
            Dictionary<string, ADFNode> nodesDict = new Dictionary<string, ADFNode>();
            for (int i = 0; i < nodes.Count; i++) {
                ADFNode node = nodes[i];
                node.ID = string.Concat("N", i + 1);
                nodesDict.Add(node.Name, node);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("flowchart TD");
            foreach (ADFNode node in nodesDict.Values) {
                sb.Append(node.ID);

                switch (node.NodeType) {
                    case "source":
                    case "sink":
                        sb.Append("[(").Append(node.Name).AppendLine(")]:::C1");
                        break;
                    case "derive":
                    case "aggregate":
                    case "select":
                    case "window":
                        sb.Append("(").Append(node.Name).AppendLine("):::C2");
                        break;
                    case "lookup":
                    case "union":
                    case "split":
                    case "join":
                        sb.Append("(").Append(node.Name).AppendLine("):::C3");
                        break;
                    case "filter":
                    case "alterRow":
                        sb.Append("(").Append(node.Name).AppendLine("):::C4");
                        break;
                    default:
                        sb.Append("[").Append(node.Name).AppendLine("]");
                        break;
                }
            }
            foreach (ADFNode node in nodesDict.Values) {
                if (node.Children != null) {
                    foreach (ADFNode childNode in node.Children) {
                        sb.Append(node.ID);
                        sb.Append("---");
                        sb.AppendLine(childNode.ID);
                    }
                }
            }
            sb.AppendLine("classDef C1 fill:#DDF0FF, stroke:#0078D4"); // Blue: source/sink
            sb.AppendLine("classDef C2 fill:#e8ede2, stroke:#47711C"); // Green: derive/aggregate/select/window
            sb.AppendLine("classDef C3 fill:#e7dcfd, stroke:#B796F9"); // Purple: lookup/union/split/join
            sb.AppendLine("classDef C4 fill:#fef4e8, stroke:#F9981E"); // Orange: filter/alterRow
            File.WriteAllText(Path.Combine(outputFolder, "mermaid.txt"), sb.ToString());
        }
    }
}
