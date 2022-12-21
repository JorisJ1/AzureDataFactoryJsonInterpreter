using System.Text;
using System.Text.Json;

namespace AzureDataFactoryJsonInterpreter
{
    public class ADFFunctions
    {
        public static List<ADFNode> GetNodes(string path) {

            string jsonString = File.ReadAllText(path);

            // Read the JSON with case-insentive fields.
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            ADFDataFlow dataFlow = JsonSerializer.Deserialize<ADFDataFlow>(jsonString, options);

            string source = "";
            if (dataFlow.properties != null && dataFlow.properties.typeProperties != null) {
                if (dataFlow.properties.typeProperties.script != null) {
                    source = dataFlow.properties.typeProperties.script;
                } else if (dataFlow.properties.typeProperties.ScriptLines != null) {
                    string[] scriptLines = dataFlow.properties.typeProperties.ScriptLines;
                    source = string.Join('\n', scriptLines);
                }
            }

            // Convert the source script to a list of raw node data.
            List<ADFNodeRaw> rawNodes = GetRawNodes(source);

            ExtractInfo(rawNodes);

            return GetNodes(rawNodes);
        }

        public static List<ADFNodeRaw> GetRawNodes(string source) {
            List<ADFNodeRaw> rawNodes = new List<ADFNodeRaw>();

            StringBuilder currentNodeLines = new StringBuilder();
            ADFNodeRaw currentNode = new ADFNodeRaw();

            string[] lines = source.Replace("\t", "").Split('\n');
            foreach (string line in lines) {

                if (line.Contains("~> ")) {
                    // This line contains the name of the node.

                    // Split to separate the name from the rest of the line.
                    string[] lineSplit = line.Split("~> ");
                    currentNode.name = lineSplit[1];

                    // The rest of the line is added to the 'content' lines.
                    currentNodeLines.AppendLine(lineSplit[0]);

                    // Add to the list and start anew.
                    currentNode.lines = currentNodeLines.ToString();
                    currentNodeLines.Clear();
                    rawNodes.Add(currentNode);
                    currentNode = new ADFNodeRaw();

                } else {
                    // This is a 'content' line.
                    currentNodeLines.AppendLine(line);
                }
            }

            return rawNodes;
        }

        public static void ExtractInfo(List<ADFNodeRaw> rawNodes) {
            foreach (ADFNodeRaw node in rawNodes) {

                // In case of ConditionalSplit1@(Insert, Update, ...
                if (node.name != null) {
                    string[] nameParts = node.name.Split('@');
                    if (nameParts.Length > 1) {
                        // Maybe do something here later with the other name parts.
                        node.name = nameParts[0];
                    }
                }

                string[] parts = node.lines.Split('(', 2, StringSplitOptions.None);
                node.definition = parts[0];
                node.contents = parts[1];

                // Extract info from the definition.
                string[] definitionParts = node.definition.Replace(",", "").Split(' ');
                node.nodeType = definitionParts[definitionParts.Length - 1];
                string[] inputStreams = new string[definitionParts.Length - 1];
                Array.Copy(definitionParts, 0, inputStreams, 0, definitionParts.Length - 1);
                node.InputStreams = inputStreams;
            }
        }

        internal static List<ADFNode> GetNodes(List<ADFNodeRaw> rawNodes) {

            Dictionary<string, ADFNode> dict = new Dictionary<string, ADFNode>();

            // Create an ADFNode for each ADFNodeRaw and add to the dictionary.
            foreach (ADFNodeRaw nodeRaw in rawNodes) {
                ADFNode node = new ADFNode(nodeRaw.name, nodeRaw.nodeType);
                dict.Add(node.Name, node);
            }

            // Find out the children.
            foreach (ADFNodeRaw nodeRaw in rawNodes) {
                ADFNode currentNode = dict[nodeRaw.name];

                foreach (string inputStream in nodeRaw.InputStreams) {

                    string inputNodeName;
                    string[] inputStreamParts = inputStream.Split('@');
                    // In case of ConditionalSplit1@...
                    inputNodeName = inputStreamParts[0];

                    ADFNode parentNode = dict[inputNodeName];
                    if (parentNode.Children == null) parentNode.Children = new List<ADFNode>();
                    parentNode.Children.Add(currentNode);
                }
            }

            return dict.Values.ToList<ADFNode>();
        }
    }
}
