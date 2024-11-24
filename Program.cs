using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GammaTools;
class Program
{
    // 引入 Windows API 函数
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    static void Main(string[] args)
    {
        // 创建一个监听线程
        Thread listenerThread = new Thread(ListenForKeyCombo);
        listenerThread.Start();

        // 主线程等待监听线程退出
        listenerThread.Join();
    }

    static void ListenForKeyCombo()
    {
        var config = Config.CheckAndCreateConfigFile();
        Console.WriteLine($"已读取{config.Count}个配置:{string.Join(", ", new List<string>(config.Keys))}");
        while (true)
        {
            foreach(var section in config){
                if ((GetAsyncKeyState(Convert.ToInt16(section.Value["key1"], 16)) & 0x8000) != 0 &&
                    (GetAsyncKeyState(Convert.ToInt16(section.Value["key2"], 16)) & 0x8000) != 0 &&
                    (GetAsyncKeyState(Convert.ToInt16(section.Value["key3"], 16)) & 0x8000) != 0)
                {
                    Gamma.SetGammaForMonitorByIndex(int.Parse(section.Value["monitor"])-1, 0-float.Parse(section.Value["gamma"]));
                    //Console.WriteLine($"Monitor:{section.Value["monitor"]},Gamma:{section.Value["gamma"]}");
                    Thread.Sleep(200);
                }
            }
            Thread.Sleep(50); // 减少 CPU 占用
        }
    }
}
