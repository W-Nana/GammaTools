using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using IniParser;
//using IniParser.Model;

namespace GammaTools
{
    public class Config
    {
        public static Dictionary<string, Dictionary<string, string>> CheckAndCreateConfigFile(){
            // 获取程序所在的目录
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 配置文件路径
            string configFilePath = Path.Combine(appDirectory, "config.ini");

            // 检查文件是否存在
            if (!File.Exists(configFilePath))
            {
                // 文件不存在，创建一个默认的配置文件
                string defaultConfig = "[Profile1]\ngamma=0\nkey1=0x11\nkey2=0x12\nkey3=0x50"; // 默认配置内容

                // 创建并写入配置文件
                File.WriteAllText(configFilePath, defaultConfig);

                Console.WriteLine("配置文件不存在，已生成默认配置文件。");
            }
            else
            {
                Console.WriteLine("配置文件已存在。");
            }
            return ReadIniFile(configFilePath);
            
        }

        static Dictionary<string, Dictionary<string, string>> ReadIniFile(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = string.Empty;

            foreach (var line in File.ReadLines(filePath))
            {
                var trimmedLine = line.Trim();

                // 跳过空行或注释行
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                    continue;

                // 检查是否是节
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.TrimStart('[').TrimEnd(']');
                    if (!result.ContainsKey(currentSection))
                        result[currentSection] = new Dictionary<string, string>();
                }
                else
                {
                    // 读取键值对
                    var parts = trimmedLine.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        result[currentSection][parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            return result;
        }
    }
}