using AzureDataFactoryJsonInterpreter;

string path = @"C:\temp\dataflow1.json";
string outputFolder = @"c:\temp\";

List<ADFNode> nodes = ADFFunctions.GetNodes(path);
OutputFunctions.GenerateMarkdownDocumentation(nodes, outputFolder);
//OutputFunctions.GenerateMermaidFlowChart(nodes, outputFolder);
//OutputFunctions.GenerateHtmlDoc(nodes, outputFolder);