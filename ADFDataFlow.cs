
// These classes are used for interpreting the JSON directly.

internal class ADFDataFlow
{
    public string? name { get; set; }
    public ADFProperties? properties { get; set; }
}

internal class ADFProperties
{
    public string? description { get; set; }
    public string? type { get; set; }
    public ADFTypeProperties? typeProperties { get; set; }
}

internal class ADFTypeProperties
{
    public List<ADFSource>? sources { get; set; }
    public List<ADFSink>? sinks { get; set; }
    public List<ADFTransformation>? transformations { get; set; }
    public string? script { get; set; } // Older files.
    public string[] ScriptLines { get; set; } // Newer files.
}

public class ADFSource
{
    public string? name { get; set; }
    public string? description { get; set; }
    public ADFDataSet? dataset { get; set; }
}

public class ADFSink
{
    public string? name { get; set; }
    public string? description { get; set; }
    public ADFDataSet? dataset { get; set; }
}

public class ADFDataSet
{
    public string? referenceName { get; set; }
    public string? type { get; set; }
}

public class ADFTransformation
{
    public string? name { get; set; }
}

// These classes are used for interpreting the source under properties.typeProperties.script.

public class ADFNodeRaw
{
    // These are filled in ADFFunctions.GetRawNodes.
    public string? name { get; set; }
    public string? lines { get; set; }

    // These are filled in ADFFunctions.ExtractInfo.
    public string? definition { get; set; }
    public string? contents { get; set; }

    public string? nodeType { get; set; }
    public string[] InputStreams { get; set; }
    public Dictionary<string, string> contentDict { get; internal set; }
}

public class ADFNode
{
    public string? ID { get; set; }
    public string Name { get; set; }
    public string NodeType { get; set; }
    public List<ADFNode>? Children { get; set; }

    // Temporary until I finish creating a class for every node type.
    public Dictionary<string, string> NodeInfo { get; internal set; }
}

//public class ADFNodeSource : ADFNode
//{
//    // TODO: A class for every node type.
//}
