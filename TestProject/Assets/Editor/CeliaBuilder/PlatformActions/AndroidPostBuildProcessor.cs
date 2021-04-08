using System.IO;

using UnityEditor;
using UnityEditor.Android;

public class AndroidPostBuildProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get
        {
            return 999;
        }
    }

    void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
    {
        generateGradleProperties(path);
    }

    private void generateGradleProperties(string path)
    {
        SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
        string gradlePropertiesFile = path + "/gradle.properties";
        if (File.Exists(gradlePropertiesFile))
        {
            File.Delete(gradlePropertiesFile);
        }
        StreamWriter writer = File.CreateText(gradlePropertiesFile);
        writer.WriteLine("org.gradle.jvmargs = -Xmx8192M");
        writer.WriteLine("android.useAndroidX = true");
        writer.WriteLine("android.enableJetifier = true");
        if (sdkParams.SDKType == celia.game.SDKType.Native)
        {
            writer.WriteLine("android.enableD8.desugaring = false");
            writer.WriteLine("android.enableD8 = false");
        }
        writer.Flush();
        writer.Close();
    }
}