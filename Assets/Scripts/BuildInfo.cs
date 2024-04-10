using UnityEngine;

public static class BuildInfo {
    public static readonly string VANILLA_BUILD_TIME = "4/9/2024 3:18:18 PM";
    public static readonly string VANILLA_VERSION = "1.8.0.0";

    public static readonly string VCMI_BUILD_CODE = "dev-0";
    public static readonly string VCMI_VERSION = Application.version;

    public static string GetVcmiVersionString() {
        return (VCMI_BUILD_CODE.Contains("dev") ? VCMI_BUILD_CODE : "") + "\nv" + VCMI_VERSION;
    }

    public static string GetDevVersionString() {
        return $"vcmi {VCMI_VERSION} ({VCMI_BUILD_CODE}) based on {VANILLA_VERSION} ({VANILLA_BUILD_TIME})";
    }
}
