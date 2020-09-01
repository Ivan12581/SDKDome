using UnityEngine;
using UnityEditor;
using celia.game;
using System.IO;
using System.Collections.Generic;
using System;

public class AssetProcessManager : AssetPostprocessor
{
    //static Live2DModelClothInitInfo _clothSetting = AssetDatabase.LoadAssetAtPath<Live2DModelClothInitInfo>("Assets/Live2DPrefabProcess/Editor/Resources/MyLive2DModelClothInitInfo.asset");

    [MenuItem("Tools/生成Logic协议")]
    static void GenerateLogicProto()
    {
        {
            Debug.Log("生成Logic相关的CS文件");
            string command = "";
#if UNITY_EDITOR_OSX
            command = "./Assets/Plugins/ProtoCompiler/macOS/protoc" 
                + " --csharp_out=./Assets/Scripts/Network" 
                + " ./Assets/Scripts/Network/logic.proto";
#elif UNITY_EDITOR_WIN
            command = Path.Combine(Application.dataPath, "Plugins/ProtoCompiler/Windows/protoc.exe")
                + $" --csharp_out={Path.Combine(Application.dataPath, "Scripts/Network")}" 
                + $" -I={Path.Combine(Application.dataPath, "Scripts/Network")} logic.proto";
#endif
            ShellHelper.ShellRequest req = ShellHelper.ProcessCommand(command, "");
            req.onLog += delegate (int arg1, string arg2) {
                Debug.Log(arg2);
            };
            req.onDone += delegate () {
                AssetDatabase.Refresh();
            };
        }
    }

    [MenuItem("Tools/生成Auth协议")]
    static void GenerateAuthProto()
    {
        {
            Debug.Log("生成Auth相关的CS文件");
            string command = "";
#if UNITY_EDITOR_OSX
            command = "./Assets/Plugins/ProtoCompiler/macOS/protoc" 
                + " --csharp_out=./Assets/Scripts/Network" 
                + " ./Assets/Scripts/Network/auth.proto";
#elif UNITY_EDITOR_WIN
            command = Path.Combine(Application.dataPath, "Plugins/ProtoCompiler/Windows/protoc.exe")
                + $" --csharp_out={Path.Combine(Application.dataPath, "Scripts/Network")}"
                + $" -I={Path.Combine(Application.dataPath, "Scripts/Network")} auth.proto";
#endif
            ShellHelper.ShellRequest req = ShellHelper.ProcessCommand(command, "");
            req.onLog += delegate (int arg1, string arg2) {
                Debug.Log(arg2);
            };
            req.onDone += delegate () {
                AssetDatabase.Refresh();
            };
        }
    }

    void OnPreprocessAudio()
    {
        var audioImporter = assetImporter as AudioImporter;

        AudioImporterSampleSettings sampleSetting = new AudioImporterSampleSettings();
        sampleSetting.loadType = AudioClipLoadType.Streaming;
        sampleSetting.compressionFormat = AudioCompressionFormat.Vorbis;
        sampleSetting.quality = 1;
        sampleSetting.sampleRateOverride = 44100;

        audioImporter.SetOverrideSampleSettings("Android", sampleSetting);
        audioImporter.SetOverrideSampleSettings("iOS", sampleSetting);
    }


    List<string> SpecialL2DTextureList = new List<string>()
    {"10062", "10066", "10211", "10212", "10215", "10082", "10242", "10246", "20021", "20025", "20181", "20185", "902503",
        "20221", "20225", "20191", "20195", "20201", "20205", "20111", "20121", "20131", "20101", "20105", "20011", "20015",
        "20141", "20145", "20051", "20055", "20061", "20065", "20171", "20175", "30231", "903703", "903701", "903702", "903704", "903705", "903706",
        "904703", "904704", "904705", "904706", "904707", "904708", "904503", "40111", "40115", "904502", "904701", "904702",
        "905701", "905702", "905703", "50031", "50035", "50021", "50025", "50091", "50101", "50081", "50231", "50235", "905506", "50201",
        "906507", "906505", "906504", "906505", "906503", "906508", "60042", "60041", "60045", "60046", "60052", "60056", "60081", "60082", "60091", "60092", "60101", "60102", "906501", "906502",
        "80011", "80012", "80016", "98501", "80031", "80032", "80035", "80036", "80042", "80046", "80022", "80026",
    };

    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;

        bool isLive2DAsset = false;
        if (assetPath.IndexOf("ClothesTexture") != -1
        || assetPath.IndexOf(".2048") != -1
        || assetPath.IndexOf("CutInTexture") != -1)
        {
            isLive2DAsset = true;
        }

        TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureImporterSettings);

        MakeCommonSetting();

        // SetEffectTexture();

#if UNITY_ANDROID
        MakeAndroidPlatformSetting();
#endif

        //#if UNITY_IOS
        MakeiOSPlatformSetting();
        //#endif

        void MakeCommonSetting()
        {
            bool needCustomSettings = false;
            if (
                isLive2DAsset
                || isIconPath())
            {
                textureImporterSettings.ApplyTextureType(TextureImporterType.Sprite);
                needCustomSettings = true;
            }

            if (needCustomSettings)
            {
                importer.SetTextureSettings(textureImporterSettings);
            }
        }

        //因为有大渐变所以不能使用ETC2_RGBA8Crunched, 会有渐变阶梯,  需要使用ASTC8*8

        void MakeAndroidPlatformSetting()
        {

            // UI纹理集和图标 安卓用ASTC
            if (!isLive2DAsset && isIconPath())
            {
                TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_8x8;
                }
                else
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGB_8x8;
                }
                androidTextureImporterPlatformSettings.name = "Android";
                androidTextureImporterPlatformSettings.compressionQuality = 50;
                androidTextureImporterPlatformSettings.overridden = true;
                importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
            else if (!isLive2DAsset && assetPath.IndexOf("Atlas") != -1)// UI4096
            {
                TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_5x5;
                }
                else
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGB_5x5;
                }
                androidTextureImporterPlatformSettings.name = "Android";
                androidTextureImporterPlatformSettings.compressionQuality = 50;
                androidTextureImporterPlatformSettings.overridden = true;
                androidTextureImporterPlatformSettings.maxTextureSize = 4096;
                importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
            else if (isLive2DAsset)
            {
                //TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();

                //// 默认压缩率8x8
                //androidTextureImporterPlatformSettings.format = TextureImporterFormat.ETC2_RGBA8Crunched;

                //foreach (var cloth in _clothSetting.modelInitCloths)
                //{
                //    foreach (var clothInfo in cloth.DressInfos)
                //    {
                //        if (clothInfo.PartCategory == "Common")
                //        {
                //            // 是部位贴图，压缩率应该低
                //            if (assetPath.IndexOf(clothInfo.ImageName) != -1)
                //            {
                //                androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_4x4;
                //            }
                //        }
                //    }
                //}

                //foreach (var texturename in SpecialL2DTextureList)
                //{
                //    if (assetPath.IndexOf(texturename) != -1)
                //    {
                //        androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_8x8;
                //    }
                //}
                ////Debug.Log(assetPath);

                //androidTextureImporterPlatformSettings.name = "Android";
                //androidTextureImporterPlatformSettings.compressionQuality = 100;
                //androidTextureImporterPlatformSettings.overridden = true;

                //importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
        }

        void MakeiOSPlatformSetting()
        {
            // 图标类 8x8
            if (!isLive2DAsset && isIconPath())
            {
                TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_8x8;
                }
                else
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGB_8x8;
                }
                androidTextureImporterPlatformSettings.name = "iOS";
                androidTextureImporterPlatformSettings.compressionQuality = 50;
                androidTextureImporterPlatformSettings.overridden = true;
                importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
            else if (!isLive2DAsset && assetPath.IndexOf("Atlas") != -1)// UI4096
            {
                TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_5x5;
                }
                else
                {
                    androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGB_5x5;
                }
                androidTextureImporterPlatformSettings.name = "iOS";
                androidTextureImporterPlatformSettings.compressionQuality = 50;
                androidTextureImporterPlatformSettings.overridden = true;
                androidTextureImporterPlatformSettings.maxTextureSize = 4096;
                importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
            else if (isLive2DAsset)
            {
                //TextureImporterPlatformSettings androidTextureImporterPlatformSettings = new TextureImporterPlatformSettings();

                //// 默认压缩率8x8
                //androidTextureImporterPlatformSettings.format = TextureImporterFormat.ETC2_RGBA8Crunched;

                //foreach (var cloth in _clothSetting.modelInitCloths)
                //{
                //    foreach (var clothInfo in cloth.DressInfos)
                //    {
                //        if (clothInfo.PartCategory == "Common")
                //        {
                //            // 是部位贴图，压缩率应该低
                //            if (assetPath.IndexOf(clothInfo.ImageName) != -1)
                //            {
                //                androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_4x4;
                //            }
                //        }
                //    }
                //}

                //foreach (var texturename in SpecialL2DTextureList)
                //{
                //    if (assetPath.IndexOf(texturename) != -1)
                //    {
                //        androidTextureImporterPlatformSettings.format = TextureImporterFormat.ASTC_RGBA_8x8;
                //    }
                //}

                //androidTextureImporterPlatformSettings.name = "iOS";
                //androidTextureImporterPlatformSettings.compressionQuality = 100;
                //androidTextureImporterPlatformSettings.overridden = true;

                //importer.SetPlatformTextureSettings(androidTextureImporterPlatformSettings);
            }
        }
    }
    void SetEffectTexture()
    {
        if (assetPath.IndexOf("Art/Texture") == -1)
            return;

        TextureImporter importer = assetImporter as TextureImporter;

        importer.mipmapEnabled = false;
        TextureImporterPlatformSettings importerSettings = new TextureImporterPlatformSettings()
        {
            name = "Android",
            compressionQuality = 50,
            overridden = true,
            format = importer.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.ASTC_RGBA_4x4 : TextureImporterFormat.ASTC_RGB_4x4,
        };
        importer.SetPlatformTextureSettings(importerSettings);

        importerSettings.name = "iOS";
        importer.SetPlatformTextureSettings(importerSettings);
    }

    bool isIconPath()
    {
        //    return (assetPath.IndexOf(SpriteLoader.BIGICONPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ICONPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.SKILLICONPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ROLE_AVATAR_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ENEMY_SPRITE_BIG_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ARTICLE_ACCOUNT_ICON) != -1
        //            || assetPath.IndexOf(SpriteLoader.ARTICLE_ICON_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ENEMY_CUT_IN_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.REWARD_DISPLAY_ICON_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.CHARACTER_200X200_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.CHARACTER_HALF_LENGTH) != -1
        //            || assetPath.IndexOf(SpriteLoader.FUNCTION_DISPLAY_ICON_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.ENEMY_SPRITE_SMALL_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.MULTISUITICONPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.SUITBIGPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.SUITSMALLPATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.BUFF_ICON_PATH) != -1
        //            || assetPath.IndexOf(SpriteLoader.BADGE_PATH) != -1);
        return false;
    }
    #region ASSET_CHECK
    [MenuItem("Assets/AssetCheck/CheckMode")]
    //设置已有的图片和SpriteAtlas的导入格式为正确格式，主要是设置压缩格式为 ASTC 4x4 block
    static void CheckTextureMode()
    {
        GetFilterResources(new string[] { "Assets/Resources/Atlas" }, (importer) =>
        {
            if (importer is TextureImporter)
            {
                TextureImporter textureImporter = importer as TextureImporter;
            }
#if UNITY_ANDROID
            if (importer.assetPath.Contains(".spriteatlas"))
            {
                Debug.Log("Check sprite altas is: " + importer.assetPath);
                bool isAndroid = false;
                string[] spriteatlas_str = File.ReadAllLines(importer.assetPath);
                for (int i = 0; i < spriteatlas_str.Length; ++i)
                {
                    string str = spriteatlas_str[i];

                    //设置texture format，注意要有严格的空格（缩进）要求，要不然解析会出错
                    //全新未设置的情况
                    if (str.Contains("platformSettings: []"))
                    {
                        spriteatlas_str[i] = "    platformSettings:" + "\n" +
                                             "    - serializedVersion: 2" + "\n" +
                                             "      m_BuildTarget: Android" + "\n" +
                                             "      m_MaxTextureSize: 2048" + "\n" +
                                             "      m_ResizeAlgorithm: 0" + "\n" +
                                             "      m_TextureFormat: 54" + "\n" +
                                             "      m_TextureCompression: 1" + "\n" +
                                             "      m_CompressionQuality: 50" + "\n" +
                                             "      m_CrunchedCompression: 0" + "\n" +
                                             "      m_AllowsAlphaSplitting: 0" + "\n" +
                                             "      m_Overridden: 1" + "\n" +
                                             "      m_AndroidETC2FallbackOverride: 0";
                    }
                    //判断这一段的平台是否为android
                    if (str.Contains("m_BuildTarget"))
                    {
                        if (str.Contains("m_BuildTarget: Android"))
                        {
                            isAndroid = true;
                        }
                        else
                        {
                            isAndroid = false;
                        }
                    }
                    //已经设置过了，但修改format为 ASTC 4X4 block
                    if (str.Contains("m_TextureFormat") && isAndroid)
                    {
                        spriteatlas_str[i] = "      m_TextureFormat: 54";
                    }
                    //打开勾选按钮
                    if (str.Contains("m_Overridden:") && isAndroid)
                    {
                        spriteatlas_str[i] = "      m_Overridden: 1";
                    }
                }

                //检查是否android,ios的设置都有了
                bool hasAndroid = false;
                bool hasIOS = false;
                for (int i = 0; i < spriteatlas_str.Length; ++i)
                {
                    string str = spriteatlas_str[i];

                    if (!hasAndroid && str.Contains("m_BuildTarget: Android"))
                    {
                        hasAndroid = true;
                    }
                    if (!hasIOS && str.Contains("m_BuildTarget: iPhone"))
                    {
                        hasIOS = true;
                    }
                }

                if (!hasAndroid) Debug.LogError($"SpriteAtlas : <{importer.assetPath}> is not set override for android ");
                if (!hasIOS) Debug.LogError($"SpriteAtlas : <{importer.assetPath}> is not set override for iphone ");

                File.WriteAllLines(importer.assetPath, spriteatlas_str);
            }
#endif
        });

        AssetDatabase.Refresh();
        Debug.Log("Asset check is completed.");
    }

    /// <summary>
    /// 获取Resources下的经过过滤的资源
    /// </summary>
    /// <returns></returns>
    static List<string> GetFilterResources(string[] dirPaths = null, Action<AssetImporter> callBack = null)
    {
        string[] allFiles = AssetDatabase.GetAllAssetPaths();
        List<string> files = dirPaths == null ? SelectFilesByDirectoryPath(allFiles, resDirPaths) : SelectFilesByDirectoryPath(allFiles, dirPaths);
        files = SelectFilesByFileFilter(files.ToArray(), NotBuildFileExtensionFilter);

        if (callBack != null)
        {
            foreach (string file in files)
            {
                callBack.Invoke(AssetImporter.GetAtPath(file));
            }
        }

        return files;
    }

    private static string[] resDirPaths = new string[] { "Assets/Resources" };
    private static string[] NotBuildFileExtensionFilter = new string[] { ".meta", ".cs", ".txt", "" };
    private static string[] NotBuildFileDirectoryFilter = new string[] { "Editor" };

    /// <summary>
    /// 获取在某些目录下的文件
    /// </summary>
    /// <param name="filePaths"></param>
    /// <param name="dirFilters"></param>
    /// <returns></returns>
    static List<string> SelectFilesByDirectoryPath(string[] filePaths, string[] dirFilters)
    {
        List<string> returnPaths = new List<string>();
        for (int i = 0; i < filePaths.Length; i++)
        {
            for (int j = 0; j < dirFilters.Length; j++)
            {
                if (filePaths[i].Contains(dirFilters[j]))
                {
                    bool isOk = true;
                    for (int k = 0; k < NotBuildFileDirectoryFilter.Length; k++)
                    {
                        if (filePaths[i].Contains(NotBuildFileDirectoryFilter[k]))
                        {
                            isOk = false;
                            break;
                        }
                    }
                    if (isOk)
                    {
                        returnPaths.Add(filePaths[i]);
                    }
                }
            }

        }
        return returnPaths;
    }

    /// <summary>
    /// 通过文件扩展名筛选文件
    /// </summary>
    /// <param name="files"></param>
    /// <param name="filters"></param>
    /// <returns></returns>    
    static List<string> SelectFilesByFileFilter(string[] files, string[] filters)
    {
        List<string> filePaths = new List<string>();
        bool isTargetFileExtension;
        for (int i = 0; i < files.Length; i++)
        {
            isTargetFileExtension = true;
            for (int k = 0; k < filters.Length; k++)
            {
                string extension = Path.GetExtension(files[i]);
                if (extension == filters[k])
                {
                    isTargetFileExtension = false;
                }
            }
            if (isTargetFileExtension)
            {
                filePaths.Add(files[i]);
            }
        }
        return filePaths;
    }
    #endregion
}