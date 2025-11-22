using YooAsset;
using UnityEngine;
using System;
using System.IO;
using Object = UnityEngine.Object;

public class ResMgr : UnitySingleton<ResMgr>
{
    public const string DefaultPackage = "DefaultPackage";
    public const string AssetsPackageRoot = "Assets/Game/AssetsPackage/";
    public void Init()
    {
         
    } 
    
    //Package + 资源路径
    public AssetHandle LoadAssetAsync<T>(string assetPath, string packageName = null) where T : Object
    {
        assetPath = Path.Combine(AssetsPackageRoot, assetPath);
        ResourcePackage package = null;
        if (packageName == null)
        {
            var handle = YooAssets.LoadAssetAsync<T>(assetPath); 
            return handle;
        }
        
        package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            return null;
        }
        return package.LoadAssetAsync<T>(assetPath);;
    }
    
    public T LoadAssetSync<T>(string assetPath, string packageName = null) where T : Object
    {
        assetPath = Path.Combine(AssetsPackageRoot, assetPath);
        
        ResourcePackage package = null;
        T assetObject = null;
        AssetHandle handle = null;
        if (packageName == null)
        {
            handle = YooAssets.LoadAssetSync<T>(assetPath);
            if (handle != null)
            {
                assetObject = handle.AssetObject as T;
                handle.Dispose();
            }

            return assetObject;
        }
        
        package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            return null;
        }

        handle = package.LoadAssetSync<T>(assetPath);
        if (handle != null)
        {
            assetObject = handle.AssetObject as T;
            handle.Dispose();
        }
        return assetObject;
    }

    public void UnloadUnusedAssets(string packageName = null)
    {
        if (packageName == null)
        {
            packageName = DefaultPackage;
        }
        
        ResourcePackage package = null;
        package = YooAssets.TryGetPackage(packageName);
        if (package == null) return;

        package.UnloadUnusedAssetsAsync();
    }
    
    public SceneHandle LoadSceneAsync(string scenePath, string packageName)
    {
        scenePath = Path.Combine(AssetsPackageRoot, scenePath);
        ResourcePackage package = null;
        if (packageName == null)
        {
            return null;
        }
        
        package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            return null;
        }
        return package.LoadSceneAsync(scenePath);;
    }
    
    public SceneHandle LoadSceneSync<T>(string scenePath, string packageName) where T : Object
    {
        ResourcePackage package = null;
        if (packageName == null)
        {
            return null;
        }
        
        package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            return null;
        }
        return package.LoadSceneSync(scenePath);;
    }
   
}