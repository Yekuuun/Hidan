namespace Deamon.Ebpf.Config;

internal static class EbpfMaps
{
    public static string RingbufferMapName = "event_output";
    public static List<string> CommonMaps  = ["hide_from_cache_bin", "hide_from_cache_dir", "hide_from_cache_file", "getdents64_cache"];
}