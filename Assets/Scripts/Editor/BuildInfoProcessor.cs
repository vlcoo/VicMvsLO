// https://forum.unity.com/threads/build-date-or-version-from-code.59134/

using System;
using System.IO;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildInfoProcessor : IPreprocessBuildWithReport {
    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report) {
        // WARNING: this date is the build date of vanilla, not of vcmi. so we don't recalculate it. 
        // StringBuilder sb = new();
        // sb.Append("public static class BuildInfo");
        // sb.Append("{");
        // sb.Append("public static string SOURCE_BUILD_TIME = \"");
        // sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        // sb.Append("\";");
        // sb.Append("}");
        //
        // using StreamWriter file = new(@"Assets/Scripts/BuildInfo.cs");
        // file.WriteLine(sb.ToString());
    }
}

