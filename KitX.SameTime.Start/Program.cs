using System.Diagnostics;

Console.WriteLine("Hello, World!");
Console.WriteLine("KitX.SameTime.Start");

Console.Write("Input target time: ");
string? tarTime = Console.ReadLine();
if (tarTime != null)
{
    DateTime dt = DateTime.Parse(tarTime);
    Console.Write("KitX Dashboard will start at: ");
    Console.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss"));

    while (true)
    {
        if (DateTime.Now >= dt)
        {
            Console.WriteLine("KitX Dashboard is starting...");
            if (OperatingSystem.IsWindows())
                Process.Start(Path.GetFullPath($"KitX Dashboard.exe"));
            if (OperatingSystem.IsLinux())
                Process.Start(Path.GetFullPath($"KitX Dashboard"));
            if (OperatingSystem.IsMacOS())
                Process.Start(Path.GetFullPath($"KitX Dashboard"));
            break;
        }
        Thread.Sleep(10);
    }
    Console.WriteLine($"KitX Dashboard started.");
}
