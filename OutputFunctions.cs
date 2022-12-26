using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace AzureDataFactoryJsonInterpreter
{
    internal class OutputFunctions
    {
        internal static void GenerateHtmlDoc(List<ADFNode> nodes, string outputFolder) {
            string htmlTemplate = File.ReadAllText("template.html");

            StringBuilder sb = new StringBuilder();
            GenerateMermaidFlowChart(sb, nodes, includeJsCallbacks:true);
            string jsonInfo = GenerateJsonInfo(nodes);
            string html = htmlTemplate.Replace("$mermaid$", sb.ToString());
            html = html.Replace("$json$", jsonInfo);

            File.WriteAllText(Path.Combine(outputFolder, "index.html"), html);
        }

        private static string GenerateJsonInfo(List<ADFNode> nodes) {
            List<string> nodeStrings = new List<string>();
            StringBuilder sb;
            foreach (ADFNode node in nodes) {
                sb = new StringBuilder();
                sb.Append('\"');
                sb.Append(node.ID);
                sb.Append("\":\"");
                GenerateNodeDescriptionHtml(sb, node);
                sb.Append('\"');
                nodeStrings.Add(sb.ToString());
            }
            return string.Concat('{', string.Join(',', nodeStrings),'}');
        }

        private static void GenerateNodeDescriptionHtml(StringBuilder sb, ADFNode node) {
            sb.Append("<b>");
            sb.Append(node.Name);
            sb.Append("</b>");
            sb.Append("<i>");
            sb.Append(node.NodeType);
            sb.Append("</i>");
            sb.Append("<ul>");
            foreach (var item in node.NodeInfo) {
                sb.Append("<li><b>");
                sb.Append(item.Key);
                sb.Append("</b>: ");
                sb.Append(item.Value);
                sb.Append("</li>");
            }
            sb.Append("<ul>");
        }

        internal static void GenerateMarkdownDocumentation(List<ADFNode> nodes, string outputPath) {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("## Nodes");
            sb.AppendLine();
            GenerateMarkdownText(sb, nodes);

            sb.AppendLine();
            sb.AppendLine("## Flowchart");
            sb.AppendLine();
            sb.AppendLine("::: mermaid");
            GenerateMermaidFlowChart(sb, nodes);
            sb.AppendLine(":::");

            File.WriteAllText(outputPath, sb.ToString());
        }

        internal static void GenerateMarkdownText(StringBuilder sb, List<ADFNode> nodes) {
            foreach (ADFNode node in nodes) {
                sb.Append("### ");
                sb.AppendLine(node.Name);

                if (node.Description != null) {
                    sb.AppendLine(node.Description);
                    sb.AppendLine();
                }

                sb.Append("Type: ");
                sb.AppendLine(GetFullNodeTypeName(node.NodeType));

                if (node.Parents != null && node.Parents.Count > 0) {
                    sb.Append("Input: ");
                    foreach (ADFNode parentNode in node.Parents) {
                        // Anchor link to another section.
                        sb.Append("[");
                        sb.Append(parentNode.Name);
                        sb.Append("](#");
                        sb.Append(parentNode.Name.ToLower().Replace(" ", "-"));
                        sb.Append(") ");
                    }
                    sb.AppendLine();
                }

                if (node.Children != null && node.Children.Count > 0) {
                    sb.Append("Output: ");
                    foreach (ADFNode childNode in node.Children) {
                        // Anchor link to another section.
                        sb.Append("[");
                        sb.Append(childNode.Name);
                        sb.Append("](#");
                        sb.Append(childNode.Name.ToLower().Replace(" ", "-"));
                        sb.Append(") ");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine();
            }
        }

        private static string GetFullNodeTypeName(string nodeType) {
            switch (nodeType) {
                case "derive": return "Derived column";
                case "alterRow": return "Alter row";
                default: return char.ToUpper(nodeType[0]) + nodeType.Substring(1);
            }
        }

        private static void GenerateMermaidFlowChart(StringBuilder sb , List<ADFNode> nodes, bool includeJsCallbacks = false) {

            // FIXME: Why are Mermaid comments (lines starting with '%%') not working?

            // Put all nodes in a dictionary by name and assign unique IDs.
            Dictionary<string, ADFNode> nodesDict = new Dictionary<string, ADFNode>();
            for (int i = 0; i < nodes.Count; i++) {
                ADFNode node = nodes[i];
                node.ID = string.Concat("N", i + 1);
                nodesDict.Add(node.Name, node);
            }

            sb.AppendLine("flowchart TD");
            //sb.AppendLine("%% Declaration of nodes in format: <id>[(<name>)]:::<style classname>");
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

            //sb.AppendLine("%% Declaration of node connections.");
            foreach (ADFNode node in nodesDict.Values) {
                if (node.Children != null) {
                    foreach (ADFNode childNode in node.Children) {
                        sb.Append(node.ID);
                        sb.Append("---");
                        sb.AppendLine(childNode.ID);
                    }
                }
            }

            //sb.AppendLine("%% Style classes.");
            sb.AppendLine("classDef C1 fill:#DDF0FF, stroke:#0078D4"); // Blue: source/sink
            sb.AppendLine("classDef C2 fill:#e8ede2, stroke:#47711C"); // Green: derive/aggregate/select/window
            sb.AppendLine("classDef C3 fill:#e7dcfd, stroke:#B796F9"); // Purple: lookup/union/split/join
            sb.AppendLine("classDef C4 fill:#fef4e8, stroke:#F9981E"); // Orange: filter/alterRow

            if (includeJsCallbacks) {
                //sb.AppendLine("%% Click events.");
                foreach (ADFNode node in nodesDict.Values) {
                    sb.Append("click ");
                    sb.Append(node.ID);
                    sb.AppendLine(" doCb");
                }
            }
        }
    }
}
