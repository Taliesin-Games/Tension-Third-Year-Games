using System.Collections.Generic;

public class FbxScanResult
{
    public string FbxPath;
    public string FbxName;
    public string FolderPath;
    public List<string> MaterialSlots = new();
    public List<TextureMatchInfo> TextureMatches = new();
}