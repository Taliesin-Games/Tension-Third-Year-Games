using Neuston.UnusedAssetFinder;
using System.Collections.Generic;

public class UnusedAssetConfiguration : IUnusedAssetConfiguration
{
    public void FilterAssetPaths(List<string> assetPaths)
    {
        assetPaths.RemoveAll(path => path.StartsWith("Assets/Plugins/"));
        assetPaths.RemoveAll(path => path.StartsWith("Assets/_2ndParty/"));
        assetPaths.RemoveAll(path => path.StartsWith("Assets/_3rdParty/"));
        assetPaths.RemoveAll(path => path.StartsWith("Assets/_Scenes/Debug Scennes/"));
        assetPaths.RemoveAll(path => path.StartsWith("Assets/_Scenes/Demo/"));
        assetPaths.RemoveAll(path => path.Contains("credits.txt"));
    }
}