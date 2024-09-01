using System.IO;

using Newtonsoft.Json;

namespace MovieEditor2.main.Library;

internal static class JsonHandler
{
    /// <summary>
    /// 設定ファイルロード処理
    /// </summary>
    /// <param name="setting">設定値オブジェクト</param>
    /// <param name="jsonPath">設定値ファイルパス</param>
    /// <typeparam name="Type">設定値オブジェクトの型</typeparam>
    public static void LoadSettingFromJson<Type>(ref Type setting, string jsonPath)
    {
        try
        {
            var jsonContent = File.ReadAllText(jsonPath);
            var deserialized = JsonConvert.DeserializeObject<Type>(jsonContent);
            if(deserialized is not null)
            {
                setting = deserialized;
            }
        }
        catch(FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine("jsonファイル新規作成");
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{e}");
        }
    }

    /// <summary>
    /// 設定ファイルセーブ処理
    /// </summary>
    /// <param name="setting">設定値オブジェクト</param>
    /// <param name="jsonPath">設定値ファイルパス</param>
    /// <typeparam name="Type">設定値オブジェクトの型</typeparam>
    public static void SaveSettingToJson<Type>(Type setting, string jsonPath)
    {
        try
        {
            var jsonContent = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonContent);
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{e}");
        }
    }
}
