using System.Runtime.InteropServices;

namespace FlyByWireless;

public class TobiiApiException : ExternalException
{
    internal static void Throw(int error)
    {
        switch (error)
        {
            case 0:
                return;
            case 2: // insufficient license
                throw new UnauthorizedAccessException(GetMessage(error))
                {
                    HResult = error
                };
            case 3: // not supported
                throw new NotSupportedException(GetMessage(error))
                {
                    HResult = error
                };
            case 5: // connection failed
                    // TODO: connection failed driver
                throw new IOException(GetMessage(error))
                {
                    HResult = error
                };
            case 6: // timed out
                throw new TimeoutException(GetMessage(error))
                {
                    HResult = error
                };
            case 7: // allocation failed
                throw new OutOfMemoryException(GetMessage(error))
                {
                    HResult = error
                };
            case 8: // invalid parameter
                throw new ArgumentException(GetMessage(error))
                {
                    HResult = error
                };
            case 9: // calibration already started
            case 10: // calibration not started
            case 11: // already subscribed
            case 12: // not subscribed
            case 14: // conflicting API instances
            case 15: // calibration busy
            case 16: // callback in progress
            case 17: // too many subscribers
                throw new InvalidOperationException(GetMessage(error))
                {
                    HResult = error
                };
            case 13: // operation failed
                throw new OperationCanceledException(GetMessage(error))
                {
                    HResult = error
                };
            // TODO: firmware upgrade in progress
            default:
                throw new TobiiApiException(error);
        }
    }

    private static unsafe string GetMessage(int error)
    {
        [DllImport(TobiiApi.DllName, EntryPoint = "tobii_error_message")]
        static extern sbyte* Get(int error);

        return new(Get(error))!;
    }

    internal TobiiApiException(int error) : base(GetMessage(error), error) { }
}