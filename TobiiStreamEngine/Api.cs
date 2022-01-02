using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlyByWireless;

public enum TobiiLogLevel { Error, Warn, Info, Debug, Trace }

[Flags]
public enum TobiiDeviceGeneration : uint
{
    G5 = 2,
    IS3 = 4,
    IS4 = 8,
    All = ~0u
}

internal class SafeGCHandle : SafeHandle
{
    public SafeGCHandle(object? value = null) : base(value != null ? (nint)GCHandle.Alloc(value) : default, value != null) { }

    public override bool IsInvalid => handle == default;

    protected override bool ReleaseHandle()
    {
        GCHandle.FromIntPtr(handle).Free();
        handle = default;
        return true;
    }
}

internal class SafeApiHandle : SafeHandle
{
    [StructLayout(LayoutKind.Sequential)]
    private readonly unsafe struct CustomLog
    {
        public readonly nint Context;
        public readonly delegate* unmanaged<nint, TobiiLogLevel, sbyte*, void> Func;

        public CustomLog(SafeHandle context)
        {
            [UnmanagedCallersOnly]
            static void Log(nint context, TobiiLogLevel level, sbyte* text) => ((Action<TobiiLogLevel, string>)GCHandle.FromIntPtr(context).Target!)(level, new(text));

            Context = context.DangerousGetHandle();
            Func = &Log;
        }
    }

    private readonly SafeGCHandle _log;

    public override bool IsInvalid => handle == default;

    public SafeApiHandle(Action<TobiiLogLevel, string>? log = null) : base(default, false)
    {
        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_api_create")]
        static extern int Create(out nint api, nint customAlloc, in CustomLog log);

        _log = new(log);
        TobiiApiException.Throw(_log.IsInvalid ?
            Create(out handle, default, in Unsafe.NullRef<CustomLog>()) :
            Create(out handle, default, new(_log))
        );
    }

    protected override bool ReleaseHandle()
    {
        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_api_destroy")]
        static extern int Destroy(nint device);

        if (Destroy(handle) == default)
        {
            handle = default;
            _log.Close();
            return true;
        }
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _log.Dispose();
    }
}

public sealed class TobiiApi : IDisposable
{
    public const string DllName = "tobii_stream_engine";

    static TobiiApi()
    {
        NativeLibrary.Load(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tobii", "Tobii EyeX", DllName));

        [DllImport(DllName, EntryPoint = "tobii_get_api_version")]
        static extern int Get(in int versions);

        ReadOnlySpan<int> versions = stackalloc int[4];
        TobiiApiException.Throw(Get(versions.GetPinnableReference()));
        Version = new(versions[0], versions[1], versions[2], versions[3]);
    }

    /// <summary>
    /// Puts the calling thread to sleep until there are new callbacks available to process.
    /// </summary>
    /// <param name="devices"><see cref="TobiiDevice"> instances created with the same instance of API.</param>
    /// <returns>Whether any data was received.</returns>
    /// <exception cref="ArgumentException">No valid <see cref="TobiiDevice"/> instance was provided. At least one valid <see cref="TobiiDevice"/> instance must be provided.</exception>
    /// <exception cref="InvalidOperationException">Every instance of <see cref="TobiiDevice"/> passed in must be created with the same instance of <see cref="TobiiApi"/>. If different <see cref="TobiiApi"/> instances were used, this exception will be thrown.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    public static bool WaitForCallbacks(params TobiiDevice[] devices)
    {
        Span<nint> ds = stackalloc nint[devices.Length];
        for (var i = 0; i < devices.Length; ++i)
        {
            ds[i] = devices[i]._device.DangerousGetHandle();
        }

        [DllImport(DllName, EntryPoint = "tobii_wait_for_callbacks")]
        static extern unsafe int Wait2(nint engine, int deviceCount, in nint devices);

        [DllImport(DllName, EntryPoint = "tobii_wait_for_callbacks")]
        static extern unsafe int Wait3(int deviceCount, in nint devices);

        try
        {
            TobiiApiException.Throw(Version.Major switch
            {
                < 3 => Wait2(default, ds.Length, ds.GetPinnableReference()),
                _ => Wait3(ds.Length, ds.GetPinnableReference())
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// The current version of the API.
    /// </summary>
    public static Version Version { get; }

    private readonly SafeApiHandle _api;

    /// <summary>
    /// Returns the current system time, from the same clock used to time-stamp callback data.
    /// </summary>
    public long SystemClock
    {
        get
        {
            if (_api.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }

            [DllImport(DllName, EntryPoint = "tobii_system_clock")]
            static extern int Get(SafeApiHandle api, out long timestamp);

            TobiiApiException.Throw(Get(_api, out var t));
            return t;
        }
    }

    /// <summary>
    /// Initializes the stream engine API, with optionally provided custom logging delegate.
    /// </summary>
    /// <param name="log">Logging delegate.</param>
    /// <exception cref="OutOfMemoryException">The internal call to malloc returned NULL, so api creation failed.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    public TobiiApi(Action<TobiiLogLevel, string>? log = null) => _api = new(log);

    /// <summary>
    /// Destroys an <see cref="TobiiApi"/> instance.
    /// </summary>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public void Dispose()
    {
        _api.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Retrieves the URLs for the stream engine compatible devices, of the specified generation(s), currently connected to the system.
    /// </summary>
    /// <param name="deviceGeneration">Flags specifying which hardware generations are to be included in the enumeration.</param>
    /// <returns>An enumerable collection of device URLs.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    public IEnumerable<string> EnumerateLocalDeviceUrls(TobiiDeviceGeneration deviceGeneration = TobiiDeviceGeneration.All)
    {
        if (_api.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [UnmanagedCallersOnly]
        static unsafe void Receive(sbyte* url, nint context) => ((List<string>)GCHandle.FromIntPtr(context).Target!).Add(new(url)!);

        [DllImport(DllName, EntryPoint = "tobii_enumerate_local_device_urls_ex")]
        static extern unsafe int Enumerate(SafeApiHandle api, delegate* unmanaged<sbyte*, nint, void> receiver, nint context, TobiiDeviceGeneration deviceGeneration);

        List<string> urls = new();
        var h = GCHandle.Alloc(urls);
        try
        {
            unsafe
            {
                TobiiApiException.Throw(Enumerate(_api, &Receive, (nint)h, deviceGeneration));
            }
            return urls;
        }
        finally
        {
            h.Free();
        }
    }

    /// <summary>
    /// Creates a <see cref="TobiiDevice"/> instance to be used for communicating with a specific device.
    /// </summary>
    /// <param name="url">A valid device URL as returned by <see cref="TobiiApi.EnumerateLocalDeviceUrls(TobiiDeviceGeneration)"/>.</param>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="OutOfMemoryException">The internal call to malloc returned NULL, so api creation failed.</exception>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// TODO: add TOBII_ERROR_FIRMWARE_UPGRADE_IN_PROGRESS
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public TobiiDevice CreateDevice(string url)
    {
        if (_api.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        return new(new(_api, url));
    }
}