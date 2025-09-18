using MFDMFApp;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

class Program
{
    [STAThread()]
    static int Main(string[] args)
    {
        using var mutex = new Mutex(true, "MFDMFApp_SingleInstanceMutex", out bool createdNew);
        if (!createdNew)
        {
            // Send parameters to running instance
            using var client = new NamedPipeClientStream(".", "MFDMFAppPipe", PipeDirection.Out);
            client.Connect(1000); // Wait up to 1 second
            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine(string.Join(' ', args));
            return 0;
        }

        var mainApp = new MainApp(args);
        mainApp.Run();
        return 0;
    }
}