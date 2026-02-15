using System.Collections.Generic;
using UnityEditor;
public class FbxScanResult
{
    public string FbxPath;                              // Full path and filename
    public string FbxName;                              // Filename Only
    public string FolderPath;                           // Path without file name

    public ModelImporterMaterialImportMode importMode;
    public List<ChannelInfo> MaterialSlots = new();
    public List<TextureMatchInfo> TextureMatches = new();
}