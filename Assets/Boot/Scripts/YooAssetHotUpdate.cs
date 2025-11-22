using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class YooAssetHotUpdate : Singleton<YooAssetHotUpdate>
{
    private string remoteURL = "127.0.0.1:6080";
    private EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
    private string packageVersion;
    private ResourceDownloaderOperation Download;
    
    public void Init(EPlayMode mode, string url)
    {
        remoteURL = url;
        PlayMode = mode;
    }

    

    private IEnumerator InitPackage()
    {
        yield return new WaitForSeconds(1);
        var playMode = this.PlayMode;

        var packageName = ResMgr.DefaultPackage;

        // 创建资源包裹类
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            Debug.Log("if if ######");
            package = YooAssets.CreatePackage(packageName);
            YooAssets.SetDefaultPackage(package);
        }
        else
        {
            Debug.Log("else else ######");
        }

        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            // createParameters.Decryption
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (playMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // WebGL运行模式
        if (playMode == EPlayMode.WebPlayMode)
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            initializationOperation = package.InitializeAsync(createParameters);
#endif
        }

        yield return initializationOperation;

        // 如果初始化失败弹出提示界面
        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"{initializationOperation.Error}");
            // PatchEventDefine.InitializeFailed.SendEventMessage();
        }
        else
        {
            // _machine.ChangeState<FsmRequestPackageVersion>();
            Debug.Log($"initialization finished");
        }
    }
    
    private IEnumerator UpdatePackageVersion()
    {
        // var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var packageName = "DefaultPackage";
        var package = YooAssets.GetPackage(packageName);
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            // PatchEventDefine.PackageVersionRequestFailed.SendEventMessage();
        }
        else
        {
            Debug.Log($"Request package version : {operation.PackageVersion}");
            this.packageVersion = operation.PackageVersion;
            // _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
            // _machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
    
    private IEnumerator UpdateManifest()
    {
        // var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var packageName = "DefaultPackage";
        // var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            // PatchEventDefine.PackageManifestUpdateFailed.SendEventMessage();
            yield break;
        }
        else
        {
            // _machine.ChangeState<FsmCreateDownloader>();
            Debug.Log("Update manifest finished");
        }
    }
    
    IEnumerator CreateDownloader()
    {
        // var packageName = (string)_machine.GetBlackboardValue("PackageName");
        
        // var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        // var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        var downloader  = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        // _machine.SetBlackboardValue("Downloader", downloader);
        this.Download = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            yield break;
            // _machine.ChangeState<FsmStartGame>(); 
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            // int totalDownloadCount = downloader.TotalDownloadCount;
            // long totalDownloadBytes = downloader.TotalDownloadBytes;
            // PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            // downloader.DownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
            // downloader.DownloadUpdateCallback = PatchEventDefine.DownloadUpdate.SendEventMessage;
            downloader.DownloadErrorCallback = this.OnDownloadError;
            downloader.DownloadUpdateCallback = this.OnDownloadProgress;
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                yield break;

            // _machine.ChangeState<FsmDownloadPackageOver>(); 
        }
        
        
    }

    private void OnDownloadProgress(DownloadUpdateData data)
    {
        
    }

    private void OnDownloadError(DownloadErrorData data)
    {
        
    }

    void OnDownloadError(string error)
    {
        
    }
      
    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {
        //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
        string hostServerIP = "http://127.0.0.1";
        string appVersion = "v1.0";

#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
    }
    
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    
    IEnumerator ClearCache()
    {
        var package = YooAssets.GetPackage("DefaultPackage");
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        // operation.Completed += Operation_Completed;
        yield return operation;
    }
    
    
    public IEnumerator GameHotUpdate()
    {
        yield return this.InitPackage();
        yield return this.UpdatePackageVersion();
        yield return this.UpdateManifest();
        yield return this.CreateDownloader();
        yield return this.ClearCache();
    }
}


