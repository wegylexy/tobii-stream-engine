using System.Numerics;
using System.Runtime.InteropServices;

namespace FlyByWireless;

public sealed record class TobiiDeviceInfo
(
    string SerialNumber,
    string Model,
    string Generation,
    string FirmwareVersion,
    string? IntegrationId,
    string? HWCalibrationVersion,
    string? HWCalibrationDate,
    string? LotId,
    string? IntegrationType,
    string? RuntimeBuildVersion
)
{
    internal const int Size2 = 384, Size3 = 2048;

    internal static unsafe TobiiDeviceInfo From2(ReadOnlySpan<sbyte> deviceInfo)
    {
        fixed (sbyte* p = deviceInfo)
        {
            return new
            (
                new(p),
                new(p + 0x80),
                new(p + 0xC0),
                new(p + 0x100),
                null,
                null,
                null,
                null,
                null,
                null
            );
        }
    }

    internal static unsafe TobiiDeviceInfo From3(ReadOnlySpan<sbyte> deviceInfo)
    {
        fixed (sbyte* p = deviceInfo)
        {
            return new
            (
                new(p),
                new(p + 0x100),
                new(p + 0x200),
                new(p + 0x300),
                new(p + 0x400),
                new(p + 0x480),
                new(p + 0x500),
                new(p + 0x580),
                new(p + 0x600),
                new(p + 0x700)
            );
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TobiiTrackBox
{
    public readonly Vector3
        FrontUpperRight, FrontUpperLeft, FrontLowerLeft, FrontLowerRight,
        BackUpperRight, BackUpperLeft, BackLowerLeft, BackLowerRight;
}

public enum TobiiCapability
{
    /// <summary>
    /// Query if the display area of the display can be changed by setting <see cref="TobiiDevice.DisplayArea"/>.
    /// </summary>
    DisplayAreaWritable,
    /// <summary>
    /// Query if the device supports performing 2D calibration by calling <see cref="TobiiDevice.CalibrationCollectData(Vector2)"/>.
    /// </summary>
    Calibration2D,
    /// <summary>
    /// Query if the device supports performing 3D calibration by calling <see cref="TobiiDevice.CalibrationCollectData(Vector3)"/>.
    /// </summary>
    Calibration3D,
    /// <summary>
    /// Query if the device supports persistent storage, needed to use license key.
    /// </summary>
    PersistentStorage,
    /// <summary>
    /// Query if the device supports per-eye calibration, needed to use the per-eye calibration api.
    /// </summary>
    CalibrationPerEye,
    /// <summary>
    /// Query if the device supports combined gaze point in the wearable data stream.
    /// </summary>
    CompoundStreamWearable3DGazeCombined,
    /// <summary>
    /// Query if the device supports face type setting, needed to use <see cref="TobiiDevice.FaceType"/> and <see cref="TobiiDevice.FaceTypes"/>.
    /// </summary>
    FaceType,
    /// <summary>
    /// Query if the device supports the x- and y-coordinates of the user position guide stream.
    /// </summary>
    CompoundStreamUserPositionGuideXY,
    /// <summary>
    /// Query if the device supports the z-coordinate of the user position guide stream.
    /// </summary>
    CompoundStreamUserPositionGuideZ,
    /// <summary>
    /// Query if the device supports the wearable limited image stream.
    /// </summary>
    CompoundStreamWearableLimitedImage,
    /// <summary>
    /// Query if the device supports pupil diamater in the wearable data stream.
    /// </summary>
    CompoundStreamWearablePupilDiameter,
    /// <summary>
    /// Query if the device supports pupil position in the wearable data stream.
    /// </summary>
    CompoundStreamWearablePupilPosition,
    /// <summary>
    /// Query if the device supports eye openness signal in the wearable data stream.
    /// </summary>
    CompoundStreamWearableEyeOpenness,
    /// <summary>
    /// Query if the device supports per eye 3D gaze in the wearable data stream.
    /// </summary>
    CompoundStreamWearable3DGazePerEye,
    /// <summary>
    /// Query if the device supports x- and y- coordinates of user position guide signal in the wearable data stream.
    /// </summary>
    CompoundStreamWearableUserPositionGuideXY,
    /// <summary>
    /// <para>See alternative capabilities <seealso cref="CompoundStreamWearableImproveUserPositionHmd"/> and <seealso cref="CompoundStreamWearableIncreaseEyeRelief"/></para>
    /// Query if the device supports tracking improvements in the wearable data stream.
    /// </summary>
    [Obsolete("See alternative capabilities CompoundStreamWearableImproveUserPositionHmd and CompoundStreamWearableIncreateEyeRelief")]
    CompoundStreamWearableTrackingImprovements,
    /// <summary>
    /// Query if the device supports convergence distance in the wearable data stream.
    /// </summary>
    CompoundStreamWearableConvergenceDistance,
    /// <summary>
    /// Query if the device supports the improve user position hmd signal in the wearable data stream.
    /// </summary>
    CompoundStreamWearableImproveUserPositionHmd,
    /// <summary>
    /// Query if the device supports the increase eye relief signal in the wearable data stream.
    /// </summary>
    CompoundStreamWearableIncreaseEyeRelief
}

public enum TobiiStream
{
    GazePoint,
    GazeOrigin,
    EyePositionNormalized,
    UserPresence,
    HeadPose,
    Wearable,
    GazeData,
    DigitalSyncport,
    DiagnosticsImage,
    Custom
}

public enum TobiiValidity { Invalid, Valid }

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TobiiGazePoint
{
    public readonly long Timestamp;
    public readonly TobiiValidity Validity;
    public readonly Vector2 Position;
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TobiiStereoPosition
{
    public readonly long Timestamp;
    public readonly TobiiValidity LeftValidity;
    public readonly Vector3 Left;
    public readonly TobiiValidity RightValidity;
    public readonly Vector3 Right;
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TobiiHeadPose
{
    public readonly long Timestamp;
    public readonly TobiiValidity PositionValidity;
    public readonly Vector3 Position;
    public readonly TobiiValidity RotationValidity;
    public readonly Vector3 Rotation;
}

public enum TobiiUserPresenceStatus { Unknown, Away, Present }

public record class TobiiUserPresence
(
    TobiiUserPresenceStatus Status,
    long Timestamp
);

public enum TobiiNotificationType
{
    CalibrationState,
    ExclusiveModeState,
    TrackBox,
    DisplayArea,
    Framerate,
    PowerSaveState,
    DevicePausedState,
    CalibrationEnabledEye,
    CalibrationId,
    CombinedGazeFactor,
    Faults,
    Warnings,
    FaceType
}
public enum TobiiNotificationValueType
{
    None,
    Float,
    State,
    DisplayArea,
    Unit,
    EnabledEye,
    String
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TobiiDisplayArea
{
    public readonly Vector3 TopLeft, TopRight, BottomLeft;
}

public enum TobiiEnabledEye { Left, Right, Both }

[StructLayout(LayoutKind.Explicit, Size = 520)]
public readonly struct TobiiNotification
{

    [FieldOffset(0)]
    public readonly TobiiNotificationType Type;

    [FieldOffset(4)]
    public readonly TobiiNotificationValueType ValueType;

    [FieldOffset(8)]
    public readonly float Float;

    [FieldOffset(8)]
    public readonly TobiiDisplayArea DisplayArea;

    [FieldOffset(8)]
    public readonly uint UInt;

    [FieldOffset(8)]
    public readonly TobiiEnabledEye EnabledEye;

    [FieldOffset(8)]
    private readonly sbyte _string;
    public string String
    {
        get
        {
            unsafe
            {
                fixed (sbyte* p = &_string)
                {
                    return new(p);
                }
            }
        }
    }
}

internal class SafeDeviceHandle : SafeHandle
{
    public override bool IsInvalid => handle == default;

    public SafeDeviceHandle(SafeApiHandle api, string url) : base(default, true)
    {
        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_create")]
        static extern int Create4(SafeApiHandle api, nint url, int fieldOfUse, out nint device);

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_create")]
        static extern int Create2(SafeApiHandle api, nint url, out nint device);

        var u = Marshal.StringToCoTaskMemUTF8(url);
        try
        {
            TobiiApiException.Throw(TobiiApi.Version.Major switch
            {
                < 4 => Create2(api, u, out handle),
                _ => Create4(api, u, 1, out handle)
            });
        }
        finally
        {
            Marshal.ZeroFreeCoTaskMemUTF8(u);
        }
    }

    protected override bool ReleaseHandle()
    {
        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_destroy")]
        static extern int Destroy(nint device);

        if (Destroy(handle) == default)
        {
            handle = default;
            return true;
        }
        return false;
    }
}

public sealed class TobiiDevice : IDisposable
{
    internal class SafeGazePointHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeGazePointHandle(TobiiDevice device) : base((TobiiGazePoint e) => device._GazePoint?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static unsafe void Callback(TobiiGazePoint* gazePoint, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiGazePoint> callback)
                {
                    callback?.Invoke(*gazePoint);
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_gaze_point_subscribe")]
                static extern int Subscribe(SafeDeviceHandle device, delegate* unmanaged<TobiiGazePoint*, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(Subscribe(device._device, &Callback, this));
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_gaze_point_unsubscribe")]
            static extern int Unsubscribe(SafeDeviceHandle device);

            return Unsubscribe(_device._device) == default && base.ReleaseHandle();
        }
    }

    internal class SafeGazeOriginHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeGazeOriginHandle(TobiiDevice device) : base((TobiiStereoPosition e) => device._GazeOrigin?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static unsafe void Callback(TobiiStereoPosition* gazeOrigin, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiStereoPosition> callback)
                {
                    callback?.Invoke(*gazeOrigin);
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_gaze_origin_subscribe")]
                static extern int Subscribe(SafeDeviceHandle device, delegate* unmanaged<TobiiStereoPosition*, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(Subscribe(device._device, &Callback, this));
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_gaze_origin_unsubscribe")]
            static extern int Unsubscribe(SafeDeviceHandle device);

            return Unsubscribe(_device._device) == default && base.ReleaseHandle();
        }
    }

    internal class SafeUserPresenceHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeUserPresenceHandle(TobiiDevice device) : base((TobiiUserPresence e) => device._UserPresence?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static void Callback(TobiiUserPresenceStatus userPresence, long timestamp, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiUserPresence> callback)
                {
                    callback?.Invoke(new(userPresence, timestamp));
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_user_presence_subscribe")]
                static extern int Subscribe(SafeDeviceHandle device, delegate* unmanaged<TobiiUserPresenceStatus, long, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(Subscribe(device._device, &Callback, this));
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_user_presence_unsubscribe")]
            static extern int Unsubscribe(SafeDeviceHandle device);

            return Unsubscribe(_device._device) == default && base.ReleaseHandle();
        }
    }

    internal class SafeHeadPoseHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeHeadPoseHandle(TobiiDevice device) : base((TobiiHeadPose e) => device._HeadPose?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static unsafe void Callback(TobiiHeadPose* headPose, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiHeadPose> callback)
                {
                    callback?.Invoke(*headPose);
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_head_pose_subscribe")]
                static extern int Subscribe(SafeDeviceHandle device, delegate* unmanaged<TobiiHeadPose*, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(Subscribe(device._device, &Callback, this));
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_head_pose_unsubscribe")]
            static extern int Unsubscribe(SafeDeviceHandle device);

            return Unsubscribe(_device._device) == default && base.ReleaseHandle();
        }
    }

    internal class SafeNotificationHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeNotificationHandle(TobiiDevice device) : base((TobiiNotification e) => device._Notification?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static unsafe void Callback(TobiiNotification* notification, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiNotification> callback)
                {
                    callback?.Invoke(*notification);
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_notifications_subscribe")]
                static extern int Subscribe(SafeDeviceHandle device, delegate* unmanaged<TobiiNotification*, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(Subscribe(device._device, &Callback, this));
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_notifications_unsubscribe")]
            static extern int Unsubscribe(SafeDeviceHandle device);

            return Unsubscribe(_device._device) == default && base.ReleaseHandle();
        }
    }

    internal class SafeUserPositionGuideHandle : SafeGCHandle
    {
        private readonly TobiiDevice _device;

        public SafeUserPositionGuideHandle(TobiiDevice device) : base((TobiiStereoPosition e) => device._UserPositionGuide?.Invoke(device, e))
        {
            _device = device;

            [UnmanagedCallersOnly]
            static unsafe void Callback(TobiiStereoPosition* userPositionGuide, nint context)
            {
                if (GCHandle.FromIntPtr(context).Target is Action<TobiiStereoPosition> callback)
                {
                    callback?.Invoke(*userPositionGuide);
                }
            }

            unsafe
            {
                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_eye_position_normalized_subscribe")]
                static extern int Subscribe2(SafeDeviceHandle device, delegate* unmanaged<TobiiStereoPosition*, nint, void> callback, SafeGCHandle context);

                [DllImport(TobiiApi.DllName, EntryPoint = "tobii_user_position_guide_subscribe")]
                static extern int Subscribe3(SafeDeviceHandle device, delegate* unmanaged<TobiiStereoPosition*, nint, void> callback, SafeGCHandle context);

                TobiiApiException.Throw(TobiiApi.Version.Major switch
                {
                    < 3 => Subscribe2(device._device, &Callback, this),
                    _ => Subscribe3(device._device, &Callback, this)
                });
            }
        }

        protected override bool ReleaseHandle()
        {
            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_eye_position_normalized_unsubscribe")]
            static extern int Unsubscribe2(SafeDeviceHandle device);

            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_user_position_guide_unsubscribe")]
            static extern int Unsubscribe3(SafeDeviceHandle device);

            return TobiiApi.Version.Major switch
            {
                < 3 => Unsubscribe2(_device._device),
                _ => Unsubscribe3(_device._device)
            } == default && base.ReleaseHandle();
        }
    }

    internal readonly SafeDeviceHandle _device;

    /// <summary>
    /// Retrieves detailed information about the device, such as name and serial number.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public TobiiDeviceInfo Info
    {
        get
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }

            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_device_info")]
            static extern int Get(SafeDeviceHandle device, in sbyte info);

            var m = TobiiApi.Version.Major;
            ReadOnlySpan<sbyte> info = stackalloc sbyte[m switch
            {
                < 3 => TobiiDeviceInfo.Size2,
                _ => TobiiDeviceInfo.Size3
            }];
            TobiiApiException.Throw(Get(_device, in info.GetPinnableReference()));
            return m switch
            {
                < 3 => TobiiDeviceInfo.From2(info),
                _ => TobiiDeviceInfo.From3(info)
            };
        }
    }

    /// <summary>
    /// Retrieves 3d coordinates of the track box frustum, given in millimeters from the device center.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="NotSupportedException">The function failed because the operation is not supported by the connected tracker.</exception>
    public TobiiTrackBox TrackBox
    {
        get
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }

            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_track_box")]
            static extern int Get(SafeDeviceHandle device, out TobiiTrackBox trackBox);

            TobiiApiException.Throw(Get(_device, out var b));
            return b;
        }
    }

    #region State getters
    /// <summary>
    /// Gets whether the power save feature is active on the device. This does not necessarily mean power saving measures have been engaged.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public bool PowerSaveActive => GetStateBool(State.PowerSaveActive);

    /// <summary>
    /// Gets whether the remote wake feature is active on the device.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>

    public bool RemoteWakeActive => GetStateBool(State.RemoteWakeActive);

    /// <summary>
    /// Gets whether the device is paused. A paused device will keep the connection open but will not send any data while paused. This can indicate that the user temporarily wants to disable the device.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public bool Paused => GetStateBool(State.DevicePaused);

    /// <summary>
    /// Gets whether the device is in an exclusive mode. Similar to <see cref="Paused"/> but the device is sending data to a client with exclusive access. This state is only true for short durations and does not normally need to be handled in a normal application.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public bool ExclusiveMode => GetStateBool(State.ExclusiveMode);

    /// <summary>
    /// Gets the unique value identifying the calibration blob. 0 value indicates default calibration/no calibration done.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public uint CalibrationId => GetStateUint(State.CalibrationId);

    /// <summary>
    /// Retrieves a comma separated list of critical errors, if no errors exists the string “ok” is returned. If a critical error has occured the device will be unable to track or accept subscriptions.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public string Fault => GetStateString(State.Fault);

    /// <summary>
    /// Retrieves a comma separated list of warnings, if no warnings exists the string “ok” is returned. If a warning has occured the device should still be able to track and accept subscriptions.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="NotSupportedException">The device firmware has no support for retrieving the value of this state.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public string Warning => GetStateString(State.Warning);
    #endregion

    #region Stream events
    private SafeGazePointHandle? _safeGazePoint;
    internal EventHandler<TobiiGazePoint>? _GazePoint;
    /// <summary>
    /// The position on the screen that the user is currently looking at.
    /// </summary>
    public event EventHandler<TobiiGazePoint> GazePoint
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeGazePoint == null)
            {
                _safeGazePoint = new(this);
            }
            _GazePoint += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _GazePoint -= value;
            if (_GazePoint == null)
            {
                using (_safeGazePoint) { }
                _safeGazePoint = null;
            }
        }
    }

    private SafeGazeOriginHandle? _safeGazeOrigin;
    internal EventHandler<TobiiStereoPosition>? _GazeOrigin;
    /// <summary>
    /// A point on the users eye, reported in millimeters from the center of the display.
    /// </summary>
    public event EventHandler<TobiiStereoPosition> GazeOrigin
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeGazeOrigin == null)
            {
                _safeGazeOrigin = new(this);
            }
            _GazeOrigin += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _GazeOrigin -= value;
            if (_GazeOrigin == null)
            {
                using (_safeGazeOrigin) { }
                _safeGazeOrigin = null;
            }
        }
    }

    private SafeGazeOriginHandle? _safeUserPresence;
    internal EventHandler<TobiiUserPresence>? _UserPresence;
    /// <summary>
    /// Whether there is a person in front of the device.
    /// </summary>
    public event EventHandler<TobiiUserPresence> UserPresence
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeUserPresence == null)
            {
                _safeUserPresence = new(this);
            }
            _UserPresence += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _UserPresence -= value;
            if (_UserPresence == null)
            {
                using (_safeUserPresence) { }
                _safeUserPresence = null;
            }
        }
    }

    private SafeHeadPoseHandle? _safeHeadPose;
    internal EventHandler<TobiiHeadPose>? _HeadPose;
    /// <summary>
    /// The position and rotation of the user's head.
    /// </summary>
    public event EventHandler<TobiiHeadPose> HeadPose
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeHeadPose == null)
            {
                _safeHeadPose = new(this);
            }
            _HeadPose += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _HeadPose -= value;
            if (_HeadPose == null)
            {
                using (_safeHeadPose) { }
                _safeHeadPose = null;
            }
        }
    }

    private SafeNotificationHandle? _safeNotification;
    internal EventHandler<TobiiNotification>? _Notification;
    /// <summary>
    /// State changes for a device.
    /// </summary>
    public event EventHandler<TobiiNotification> Notification
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeNotification == null)
            {
                _safeNotification = new(this);
            }
            _Notification += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _Notification -= value;
            if (_Notification == null)
            {
                using (_safeHeadPose) { }
                _safeNotification = null;
            }
        }
    }

    private SafeUserPositionGuideHandle? _safeUserPositionGuide;
    internal EventHandler<TobiiStereoPosition>? _UserPositionGuide;
    /// <summary>
    /// The user position guide stream, which is used to help a user position their eyes in the track box correctly.
    /// </summary>
    public event EventHandler<TobiiStereoPosition> UserPositionGuide
    {
        add
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            if (_safeUserPositionGuide == null)
            {
                _safeUserPositionGuide = new(this);
            }
            _UserPositionGuide += value;
        }
        remove
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
            _UserPositionGuide -= value;
            if (_UserPositionGuide == null)
            {
                using (_safeUserPositionGuide) { }
                _safeUserPositionGuide = null;
            }
        }
    }
    #endregion

    // TODO: wearable events

    #region Config getters
    /// <summary>
    /// Queries the enabled eye property of the device.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="NotSupportedException">Getting the enabled eye property is not supported by the device.</exception>
    /// <exception cref="UnauthorizedAccessException">The provided license was not a valid license, or has been blacklisted.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    public TobiiEnabledEye EnabledEye
    {
        get
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }

            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_enabled_eye")]
            static extern int Get(SafeDeviceHandle device, out TobiiEnabledEye enabledEye);

            TobiiApiException.Throw(Get(_device, out var e));
            return e;
        }
    }

    /// <summary>
    /// Retrieves the current display area from the device.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="UnauthorizedAccessException">The provided license was not a valid license, or has been blacklisted.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    public TobiiDisplayArea DisplayArea
    {
        get
        {
            if (_device.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }

            [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_display_area")]
            static extern int Get(SafeDeviceHandle device, out TobiiDisplayArea displayArea);

            TobiiApiException.Throw(Get(_device, out var e));
            return e;
        }
    }

    // TODO: get device name

    // TODO: enumerate output frequencies

    // TODO: get output frequency
    #endregion

    internal TobiiDevice(SafeDeviceHandle device) => _device = device;

    /// <summary>
    /// Destroys a <see cref="TobiiDevice"/> instance.
    /// </summary>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public void Dispose()
    {
        using (_safeGazePoint) { }
        using (_safeGazeOrigin) { }
        using (_safeUserPresence) { }
        using (_safeHeadPose) { }
        using (_safeNotification) { }
        using (_safeUserPositionGuide) { }
        _device.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Receives data packages from the device, and sends the data through any registered callbacks.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="OutOfMemoryException">The internal call to malloc returned NULL, so api creation failed.</exception>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public void ProcessCallbacks()
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_process_callbacks")]
        static extern int Process(SafeDeviceHandle device);

        TobiiApiException.Throw(Process(_device));
    }

    /// <summary>
    /// Removes all unprocessed entries from the callback queues.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public void ClearCallbackBuffers()
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_clear_callback_buffers")]
        static extern int Clear(SafeDeviceHandle device);

        TobiiApiException.Throw(Clear(_device));
    }

    /// <summary>
    /// Establish a new connection after a disconnect.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// TODO: add TOBII_ERROR_FIRMWARE_UPGRADE_IN_PROGRESS
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    public void Reconnect()
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_device_reconnect")]
        static extern int Connect(SafeDeviceHandle device);

        TobiiApiException.Throw(Connect(_device));
    }

    /// <summary>
    /// Synchronizes the system clock with the device’s hardware clock.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="OperationCanceledException">Timesync operation could not be performed at this time. Please wait a while and try again.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// <exception cref="NotSupportedException">The function failed because the operation is not supported by the connected tracker.</exception>
    public void UpdateTimeSync()
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_update_timesync")]
        static extern int Update(SafeDeviceHandle device);

        TobiiApiException.Throw(Update(_device));
    }

    #region Get states
    private enum State
    {
        PowerSaveActive,
        RemoteWakeActive,
        DevicePaused,
        ExclusiveMode,
        Fault,
        Warning,
        CalibrationId,
        CalibrationActive
    }

    private bool GetStateBool(State state)
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_state_bool")]
        static extern int Get(SafeDeviceHandle device, State state, out int value);

        TobiiApiException.Throw(Get(_device, state, out var b));
        return b != default;
    }

    private uint GetStateUint(State state)
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_state_uint32")]
        static extern int Get(SafeDeviceHandle device, State state, out uint value);

        TobiiApiException.Throw(Get(_device, state, out var u));
        return u;
    }

    private string GetStateString(State state)
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_get_state_string")]
        static extern int Get(SafeDeviceHandle device, State state, in sbyte value);

        ReadOnlySpan<sbyte> s = stackalloc sbyte[512];
        TobiiApiException.Throw(Get(_device, state, s.GetPinnableReference()));
        unsafe
        {
            fixed (sbyte* p = s)
            {
                return new(p);
            }
        }
    }
    #endregion

    /// <summary>
    /// Ask if a specific feature is supported or not.
    /// </summary>
    /// <param name="capability">One of the capabilities.</param>
    /// <returns>Whether the feature is supported.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="IOException">The connection to the device was lost. Call <see cref="Reconnect"/> to re-establish connection.</exception>
    /// <exception cref="TobiiApiException">Some unexpected internal error occurred. This error should normally not be returned, so if it is, please contact the support.</exception>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="NotSupportedException">Specified capability not supported in the current API.</exception>
    public bool CapabilitySupported(TobiiCapability capability)
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_capability_supported")]
        static extern int Get(SafeDeviceHandle device, TobiiCapability capability, out int supported);

        try
        {
            TobiiApiException.Throw(Get(_device, capability, out var s));
            return s != 0;
        }
        catch (ArgumentException ex)
        {
            throw new NotSupportedException("Specified capability not supported in the API.", ex);
        }
    }

    /// <summary>
    /// Ask if a specific stream is supported or not.
    /// </summary>
    /// <param name="stream">One of the streams.</param>
    /// <returns>Whether the feature is supported.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="InvalidOperationException">The function failed because it was called from within a callback triggered from an API call.</exception>
    /// <exception cref="NotSupportedException">Specified stream not supported in the current API.</exception>
    public bool StreamSupported(TobiiStream stream)
    {
        if (_device.IsInvalid)
        {
            throw new ObjectDisposedException(null);
        }

        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_stream_supported")]
        static extern int Get(SafeDeviceHandle device, TobiiStream stream, out int supported);

        try
        {
            TobiiApiException.Throw(Get(_device, stream, out var s));
            return s != 0;
        }
        catch (ArgumentException ex)
        {
            throw new NotSupportedException("Specified stream not supported in the API.", ex);
        }
    }
}