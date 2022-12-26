using AzureDataFactoryJsonInterpreter;

//ProcessOne();
ProcessMultiple();

void ProcessOne() {

    string jsonFilePath = @"C:\temp\df_Historie_Vernet_Dienstverband.json";
    string outputFolder = @"C:\temp";

    ADFDataFlow dataflow = ADFFunctions.GetDataFlow(jsonFilePath);
    List<ADFNode> nodes = ADFFunctions.GetNodes(dataflow);

    OutputFunctions.GenerateMarkdownDocumentation(nodes, Path.Combine(outputFolder, dataflow.name + ".md"));
}

void ProcessMultiple() {
    string folderPath = @"C:\Users\Joris\source\repos\Dwh\data-factory\dataflow";
    string[] jsonFilePaths = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
    string outputFolder = @"C:\Users\Joris\source\repos\Overige-werkzaamheden.wiki\Testpagina\Dataflows";

    foreach (string jsonFilePath in jsonFilePaths) {

        ADFDataFlow dataflow = ADFFunctions.GetDataFlow(jsonFilePath);
        List<ADFNode> nodes = ADFFunctions.GetNodes(dataflow);

        OutputFunctions.GenerateMarkdownDocumentation(nodes, Path.Combine(outputFolder, dataflow.name + ".md"));
    }

    // Create .order file
    using (StreamWriter sw = new StreamWriter(Path.Combine(outputFolder, ".order"))) {
        foreach (var item in jsonFilePaths) {
            string path1 = Path.GetFileNameWithoutExtension(item);
            sw.WriteLine(path1);
        }
    }
}

