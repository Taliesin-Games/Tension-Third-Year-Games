using System.Collections.Generic;

public class FbxScanResult
{
    public string FbxPath;                      // Full path and filename
    public string FbxName;                      // Filename Only
    public string FolderPath;                   // Path without file name
    public List<string> MaterialSlots = new();
    public List<TextureMatchInfo> TextureMatches = new();
}