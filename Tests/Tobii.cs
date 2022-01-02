using FlyByWireless;
using Xunit;
using Xunit.Abstractions;

namespace Tests;

public sealed class Tests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly TobiiApi _api;

    public Tests(ITestOutputHelper output)
    {
        _output = output;
        _api = new((level, text) => _output.WriteLine($"[{level}] {text}"));
    }

    [Fact]
    public void Version()
    {
        var v = TobiiApi.Version;
        _output.WriteLine(v.ToString());
        Assert.InRange(v.Major, 2, 4);
    }

    [Fact]
    public void Timestamp()
    {
        var begin = _api.SystemClock;
        Thread.Sleep(TimeSpan.FromTicks(200000));
        var end = _api.SystemClock;
        Assert.InRange(end - begin, 20000, 21000);
    }

    [Fact]
    public void Devices()
    {
        foreach (var u in _api.EnumerateLocalDeviceUrls())
        {
            _output.WriteLine(u);
            using var device = _api.CreateDevice(u);
            device.Reconnect();
            var info = device.Info;
            _output.WriteLine(info.ToString());
            Assert.Throws<NotSupportedException>(() => device.TrackBox);
            _output.WriteLine(device.EnabledEye.ToString());
            _output.WriteLine(device.DisplayArea.ToString());
            device.GazePoint += (s, e) => _output.WriteLine(e.ToString());
            device.ClearCallbackBuffers();
            for (var i = 0; i < 10; ++i)
            {
                _ = TobiiApi.WaitForCallbacks(device);
                device.ProcessCallbacks();
                device.UpdateTimeSync();
            }
        }
    }

    public void Dispose()
    {
        using (_api) { }
        GC.SuppressFinalize(this);
    }
}