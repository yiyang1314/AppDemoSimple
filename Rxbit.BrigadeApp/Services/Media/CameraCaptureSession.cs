using Rxbit.Brigade.Models.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rxbit.BrigadeApp.Services.Media
{
    public sealed class CameraDeviceInfo
    {
        public string Name { get; set; }
        public string SymbolicLink { get; set; }
        public override string ToString() => Name;
    }

    /// <summary>
    /// Media Foundation 摄像头采集（离线，无需第三方包）：枚举设备、逐帧读取 RGB32 画面。
    /// </summary>
    public sealed class CameraCaptureSession : IDisposable
    {
        private const int MF_VERSION = 0x00020070;
        private const uint MF_SOURCE_READER_FIRST_VIDEO_STREAM = 0xFFFFFFFC;
        private const uint MF_SOURCE_READER_ALL_STREAMS = 0xFFFFFFFE;
        private const uint MF_SOURCE_READERF_ERROR = 0x00000002;
        private const uint MF_SOURCE_READERF_ENDOFSTREAM = 0x00000004;

        private static Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE = new Guid("c60ac5fe-252a-478f-a0ef-bc8fa5f7cad3");
        private static Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID = new Guid("8ac3587a-4ae7-42d8-99e0-0a6013eef90f");
        private static Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK = new Guid("58f0aad8-22bf-4f8a-bb3d-d2c4978c6e2f");
        private static Guid MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME = new Guid("60d0e559-52f8-4fa2-bbce-acdb34a8ec01");
        private static Guid MF_MT_MAJOR_TYPE = new Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
        private static Guid MF_MT_SUBTYPE = new Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
        private static Guid MF_MT_FRAME_SIZE = new Guid("1652c33d-d6b2-4012-b834-72030849a37d");
        private static Guid MFMediaType_Video = new Guid("73646976-0000-0010-8000-00AA00389B71");
        private static Guid MFVideoFormat_RGB32 = new Guid("00000016-0000-0010-8000-00AA00389B71");
        private static Guid MFVideoFormat_MJPG = new Guid("47504A4D-0000-0010-8000-00AA00389B71");
        private static Guid MFVideoFormat_NV12 = new Guid("3231564E-0000-0010-8000-00AA00389B71");
        private static Guid MFVideoFormat_YUY2 = new Guid("32595559-0000-0010-8000-00AA00389B71");
        private static Guid MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING = new Guid("fb394f3d-ccf1-42ee-bbb3-f9b845d58a59");

        private static bool _mfStarted;
        private static readonly object StartupLock = new object();

        private IMFSourceReader _reader;
        private bool _disposed;
        private Guid _subtype = Guid.Empty;
        private bool _loggedReadError;
        private bool _loggedSampleNull;
        private bool _loggedFirstFrame;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public static void EnsureStarted()
        {
            lock (StartupLock)
            {
                if (_mfStarted)
                {
                    return;
                }
                int hr = MFStartup(MF_VERSION, 0);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                _mfStarted = true;
            }
        }

        /// <summary>
        /// 枚举视频采集设备。
        /// </summary>
        public static List<CameraDeviceInfo> EnumerateDevices()
        {
            var devices = new List<CameraDeviceInfo>();
            try
            {
                EnsureStarted();

                IMFAttributes attributes = null;
                IntPtr arrayPtr = IntPtr.Zero;
                try
                {
                    int hr = MFCreateAttributes(out attributes, 1);
                    ThrowIfFailed(hr);
                    hr = attributes.SetGUID(ref MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, ref MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
                    ThrowIfFailed(hr);

                    int count;
                    hr = MFEnumDeviceSources(attributes, out arrayPtr, out count);
                    ThrowIfFailed(hr);
                    LogHelper.Info($"CameraCaptureSession: MFEnumDeviceSources count={count}");

                    for (int i = 0; i < count; i++)
                    {
                        IntPtr activatePtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                        IMFActivate activate = null;
                        try
                        {
                            activate = (IMFActivate)Marshal.GetTypedObjectForIUnknown(activatePtr, typeof(IMFActivate));
                            string name = GetStringAttribute(activate, MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME);
                            string link = GetStringAttribute(activate, MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK);
                            if (!string.IsNullOrWhiteSpace(link))
                            {
                                devices.Add(new CameraDeviceInfo
                                {
                                    Name = string.IsNullOrWhiteSpace(name) ? $"Camera {i + 1}" : name,
                                    SymbolicLink = link
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            // 单个设备读取失败不影响其它设备
                            LogHelper.Error($"CameraCaptureSession: 读取设备 {i} 失败: {ex.Message}", ex);
                        }
                        finally
                        {
                            if (activate != null)
                            {
                                Marshal.ReleaseComObject(activate);
                                Marshal.Release(activatePtr);
                            }
                        }
                    }
                }
                finally
                {
                    if (arrayPtr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(arrayPtr);
                    }
                    if (attributes != null)
                    {
                        Marshal.ReleaseComObject(attributes);
                    }
                }
            }
            catch (Exception ex)
            {
                // 无摄像头或 Media Foundation 不可用时返回空列表
                LogHelper.Error($"CameraCaptureSession: 枚举摄像头失败: {ex.Message}", ex);
            }
            return devices;
        }

        private static string GetStringAttribute(IMFAttributes attributes, Guid key)
        {
            IntPtr valuePtr = IntPtr.Zero;
            uint length;
            int hr = attributes.GetAllocatedString(ref key, out valuePtr, out length);
            if (hr < 0 || valuePtr == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(valuePtr);
            }
        }

        public void Open(string symbolicLink, int preferredWidth, int preferredHeight)
        {
            Close();

            _loggedReadError = false;
            _loggedSampleNull = false;
            _loggedFirstFrame = false;

            EnsureStarted();

            IMFAttributes attributes = null;
            object source = null;
            IMFAttributes readerAttributes = null;
            IMFMediaType mediaType = null;
            try
            {
                int hr = MFCreateAttributes(out attributes, 2);
                ThrowIfFailed(hr);
                hr = attributes.SetGUID(ref MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, ref MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
                ThrowIfFailed(hr);
                hr = attributes.SetString(ref MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, symbolicLink);
                ThrowIfFailed(hr);

                hr = MFCreateDeviceSource(attributes, out source);
                ThrowIfFailed(hr);

                hr = MFCreateAttributes(out readerAttributes, 1);
                ThrowIfFailed(hr);
                hr = readerAttributes.SetUINT32(ref MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, 1);
                ThrowIfFailed(hr);

                hr = MFCreateSourceReaderFromMediaSource(source, readerAttributes, out _reader);
                ThrowIfFailed(hr);

                hr = _reader.SetStreamSelection(MF_SOURCE_READER_ALL_STREAMS, false);
                ThrowIfFailed(hr);
                hr = _reader.SetStreamSelection(MF_SOURCE_READER_FIRST_VIDEO_STREAM, true);
                ThrowIfFailed(hr);

                hr = MFCreateMediaType(out mediaType);
                ThrowIfFailed(hr);
                hr = mediaType.SetGUID(ref MF_MT_MAJOR_TYPE, ref MFMediaType_Video);
                ThrowIfFailed(hr);
                hr = mediaType.SetGUID(ref MF_MT_SUBTYPE, ref MFVideoFormat_RGB32);
                ThrowIfFailed(hr);

                hr = _reader.SetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, IntPtr.Zero, mediaType);
                if (hr >= 0)
                {
                    _subtype = MFVideoFormat_RGB32;
                }
                else
                {
                    // 部分 USB 摄像头不支持 RGB32 输出，退回摄像头原生格式（MJPG/NV12/YUY2）
                    LogHelper.Info($"CameraCaptureSession: RGB32 不受支持(0x{hr:X8})，改用摄像头原生格式。");
                    SelectNativeMediaType();
                }

                ReadCurrentFormat();
            }
            finally
            {
                if (mediaType != null) Marshal.ReleaseComObject(mediaType);
                if (readerAttributes != null) Marshal.ReleaseComObject(readerAttributes);
                if (source != null) Marshal.ReleaseComObject(source);
                if (attributes != null) Marshal.ReleaseComObject(attributes);
            }
        }

        /// <summary>
        /// RGB32 不受支持时，从摄像头原生媒体类型中选择一种可处理的格式。
        /// </summary>
        private void SelectNativeMediaType()
        {
            IMFMediaType bestType = null;
            Guid bestSubtype = Guid.Empty;
            int bestWidth = 0;
            int bestHeight = 0;
            int bestScore = 0;

            for (int index = 0; index < 64; index++)
            {
                IMFMediaType type = null;
                int hr = _reader.GetNativeMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, (uint)index, out type);
                if (hr < 0 || type == null) break;

                Guid subtype = Guid.Empty;
                ulong packed = 0;
                if (type.GetGUID(ref MF_MT_SUBTYPE, out subtype) < 0
                    || type.GetUINT64(ref MF_MT_FRAME_SIZE, out packed) < 0
                    || packed == 0)
                {
                    Marshal.ReleaseComObject(type);
                    continue;
                }

                int width = (int)(packed >> 32);
                int height = (int)(packed & 0xFFFFFFFF);
                LogHelper.Info($"CameraCaptureSession: 原生类型[{index}] {subtype} {width}x{height}");

                if (!IsSupportedSubtype(subtype) || width <= 0 || width > 1920)
                {
                    Marshal.ReleaseComObject(type);
                    continue;
                }

                // 优先 MJPG（无需转换、最兼容），其次 YUY2/NV12
                int score = subtype == MFVideoFormat_MJPG ? 4
                          : subtype == MFVideoFormat_YUY2 ? 3
                          : subtype == MFVideoFormat_NV12 ? 2 : 1;
                if (score > bestScore)
                {
                    if (bestType != null) Marshal.ReleaseComObject(bestType);
                    bestType = type;
                    bestSubtype = subtype;
                    bestWidth = width;
                    bestHeight = height;
                    bestScore = score;
                }
                else
                {
                    Marshal.ReleaseComObject(type);
                }
            }

            if (bestType == null)
            {
                throw new InvalidOperationException("摄像头不支持可用的输出格式（需要 RGB32 / MJPG / NV12 / YUY2）。");
            }

            try
            {
                int hr = _reader.SetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, IntPtr.Zero, bestType);
                if (hr < 0)
                {
                    throw new InvalidOperationException($"设置摄像头格式失败（{bestSubtype} {bestWidth}x{bestHeight}），HRESULT=0x{hr:X8}。");
                }

                _subtype = bestSubtype;
                LogHelper.Info($"CameraCaptureSession: 选择原生格式={bestSubtype} {bestWidth}x{bestHeight}");
            }
            finally
            {
                Marshal.ReleaseComObject(bestType);
            }
        }

        private void ReadCurrentFormat()
        {
            IMFMediaType currentType = null;
            try
            {
                int hr = _reader.GetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, out currentType);
                ThrowIfFailed(hr);

                Guid subtype = Guid.Empty;
                if (currentType.GetGUID(ref MF_MT_SUBTYPE, out subtype) >= 0)
                {
                    _subtype = subtype;
                }

                ulong packedSize = 0;
                hr = currentType.GetUINT64(ref MF_MT_FRAME_SIZE, out packedSize);
                ThrowIfFailed(hr);
                Width = (int)(packedSize >> 32);
                Height = (int)(packedSize & 0xFFFFFFFF);
            }
            finally
            {
                if (currentType != null) Marshal.ReleaseComObject(currentType);
            }

            if (Width <= 0 || Height <= 0)
            {
                throw new InvalidOperationException("无法获取摄像头分辨率。");
            }

            LogHelper.Info($"CameraCaptureSession: 当前格式={_subtype} {Width}x{Height}");
        }

        private static bool IsSupportedSubtype(Guid subtype)
        {
            return subtype == MFVideoFormat_RGB32
                || subtype == MFVideoFormat_MJPG
                || subtype == MFVideoFormat_NV12
                || subtype == MFVideoFormat_YUY2;
        }

        /// <summary>
        /// 阻塞读取一帧。没有新帧或设备断开时返回 null。
        /// </summary>
        public BitmapSource ReadFrame()
        {
            if (_reader == null || _disposed)
            {
                return null;
            }

            IMFSample sample = null;
            IMFMediaBuffer buffer = null;
            try
            {
                uint actualStreamIndex;
                uint streamFlags;
                long timestamp;
                int hr = _reader.ReadSample(MF_SOURCE_READER_FIRST_VIDEO_STREAM, 0, out actualStreamIndex, out streamFlags, out timestamp, out sample);
                if (hr < 0)
                {
                    if (!_loggedReadError)
                    {
                        _loggedReadError = true;
                        LogHelper.Error($"CameraCaptureSession: ReadSample 失败 hr=0x{hr:X8}");
                    }
                    return null;
                }
                if ((streamFlags & (MF_SOURCE_READERF_ERROR | MF_SOURCE_READERF_ENDOFSTREAM)) != 0)
                {
                    if (!_loggedReadError)
                    {
                        _loggedReadError = true;
                        LogHelper.Error($"CameraCaptureSession: ReadSample flags=0x{streamFlags:X8}（流结束或出错）");
                    }
                    return null;
                }
                if (sample == null)
                {
                    if (!_loggedSampleNull)
                    {
                        _loggedSampleNull = true;
                        LogHelper.Info($"CameraCaptureSession: ReadSample 返回空帧，flags=0x{streamFlags:X8} stream={actualStreamIndex}");
                    }
                    return null;
                }

                hr = sample.ConvertToContiguousBuffer(out buffer);
                if (hr < 0 || buffer == null)
                {
                    if (!_loggedReadError)
                    {
                        _loggedReadError = true;
                        LogHelper.Error($"CameraCaptureSession: ConvertToContiguousBuffer 失败 hr=0x{hr:X8}");
                    }
                    return null;
                }

                IntPtr dataPtr;
                uint maxLength;
                uint currentLength;
                hr = buffer.Lock(out dataPtr, out maxLength, out currentLength);
                if (hr < 0)
                {
                    if (!_loggedReadError)
                    {
                        _loggedReadError = true;
                        LogHelper.Error($"CameraCaptureSession: IMFMediaBuffer.Lock 失败 hr=0x{hr:X8}");
                    }
                    return null;
                }

                try
                {
                    int size = (int)Math.Min(currentLength, (uint)(Width * Height * 4 + Width * Height));
                    var bytes = new byte[size];
                    Marshal.Copy(dataPtr, bytes, 0, size);
                    var bitmap = CreateBitmap(bytes, size);
                    if (bitmap == null && !_loggedReadError)
                    {
                        _loggedReadError = true;
                        LogHelper.Error($"CameraCaptureSession: 帧数据转换失败，buffer={size} 格式={_subtype} {Width}x{Height}");
                    }
                    if (bitmap != null && !_loggedFirstFrame)
                    {
                        _loggedFirstFrame = true;
                        LogHelper.Info($"CameraCaptureSession: 首帧 {Width}x{Height}，格式={_subtype}");
                    }
                    return bitmap;
                }
                finally
                {
                    buffer.Unlock();
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (buffer != null) Marshal.ReleaseComObject(buffer);
                if (sample != null) Marshal.ReleaseComObject(sample);
            }
        }

        private BitmapSource CreateBitmap(byte[] bytes, int size)
        {
            if (_subtype == MFVideoFormat_MJPG)
            {
                using (var stream = new MemoryStream(bytes, 0, size))
                {
                    var decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    var frame = decoder.Frames[0];
                    frame.Freeze();
                    return frame;
                }
            }

            var bgra = new byte[Width * Height * 4];
            if (_subtype == MFVideoFormat_NV12)
            {
                int ySize = Width * Height;
                if (size < ySize + ySize / 2) return null;

                for (int y = 0; y < Height; y++)
                {
                    int yRow = y * Width;
                    int uvRow = ySize + (y >> 1) * Width;
                    int outRow = yRow * 4;
                    for (int x = 0; x < Width; x++)
                    {
                        int uvIndex = uvRow + (x & ~1);
                        int u = bytes[uvIndex] - 128;
                        int v = bytes[uvIndex + 1] - 128;
                        WriteYuvPixel(bgra, outRow + x * 4, bytes[yRow + x], u, v);
                    }
                }
            }
            else if (_subtype == MFVideoFormat_YUY2)
            {
                if (size < Width * Height * 2) return null;

                int srcRow = Width * 2;
                for (int y = 0; y < Height; y++)
                {
                    int inRow = y * srcRow;
                    int outRow = y * Width * 4;
                    for (int x = 0; x + 1 < Width; x += 2)
                    {
                        int i = inRow + x * 2;
                        int u = bytes[i + 1] - 128;
                        int v = bytes[i + 3] - 128;
                        WriteYuvPixel(bgra, outRow + x * 4, bytes[i], u, v);
                        WriteYuvPixel(bgra, outRow + (x + 1) * 4, bytes[i + 2], u, v);
                    }
                }
            }
            else
            {
                int expected = Width * Height * 4;
                if (size < expected) return null;
                Array.Copy(bytes, bgra, expected);
                for (int i = 3; i < expected; i += 4)
                {
                    bgra[i] = 255;
                }
            }

            var bitmap = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgra32, null, bgra, Width * 4);
            bitmap.Freeze();
            return bitmap;
        }

        private static void WriteYuvPixel(byte[] target, int offset, int y, int u, int v)
        {
            int c = y - 16;
            int r = (298 * c + 409 * v + 128) >> 8;
            int g = (298 * c - 100 * u - 208 * v + 128) >> 8;
            int b = (298 * c + 516 * u + 128) >> 8;
            target[offset] = (byte)(b < 0 ? 0 : b > 255 ? 255 : b);
            target[offset + 1] = (byte)(g < 0 ? 0 : g > 255 ? 255 : g);
            target[offset + 2] = (byte)(r < 0 ? 0 : r > 255 ? 255 : r);
            target[offset + 3] = 255;
        }

        public void Close()
        {
            if (_reader != null)
            {
                try
                {
                    _reader.SetStreamSelection(MF_SOURCE_READER_ALL_STREAMS, false);
                }
                catch
                {
                    // 忽略释放阶段异常
                }
                Marshal.ReleaseComObject(_reader);
                _reader = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close();
        }

        private static void ThrowIfFailed(int hr)
        {
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        #region Media Foundation interop

        [ComImport]
        [Guid("2cd2d921-c447-44a7-a13c-4adabfc247e3")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFAttributes
        {
            [PreserveSig] int GetItem(ref Guid guidKey, IntPtr pValue);
            [PreserveSig] int GetItemType(ref Guid guidKey, out int pType);
            [PreserveSig] int CompareItem(ref Guid guidKey, IntPtr value, out bool pbResult);
            [PreserveSig] int Compare(IMFAttributes pTheirs, int matchType, out bool pbResult);
            [PreserveSig] int GetUINT32(ref Guid guidKey, out uint pValue);
            [PreserveSig] int GetUINT64(ref Guid guidKey, out ulong pValue);
            [PreserveSig] int GetDouble(ref Guid guidKey, out double pfValue);
            [PreserveSig] int GetGUID(ref Guid guidKey, out Guid pGuidValue);
            [PreserveSig] int GetStringLength(ref Guid guidKey, out uint pcchLength);
            [PreserveSig] int GetString(ref Guid guidKey, IntPtr pwszValue, uint cchBufSize, IntPtr pcchLength);
            [PreserveSig] int GetAllocatedString(ref Guid guidKey, out IntPtr ppwszValue, out uint pcchLength);
            [PreserveSig] int GetBlobSize(ref Guid guidKey, out uint pcbBlobSize);
            [PreserveSig] int GetBlob(ref Guid guidKey, IntPtr pbBuf, uint cbBufSize, IntPtr pcbBlobSize);
            [PreserveSig] int GetAllocatedBlob(ref Guid guidKey, out IntPtr ppBuf, out uint pcbSize);
            [PreserveSig] int GetUnknown(ref Guid guidKey, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int SetItem(ref Guid guidKey, IntPtr value);
            [PreserveSig] int DeleteItem(ref Guid guidKey);
            [PreserveSig] int DeleteAllItems();
            [PreserveSig] int SetUINT32(ref Guid guidKey, uint unValue);
            [PreserveSig] int SetUINT64(ref Guid guidKey, ulong unValue);
            [PreserveSig] int SetDouble(ref Guid guidKey, double fValue);
            [PreserveSig] int SetGUID(ref Guid guidKey, ref Guid guidValue);
            [PreserveSig] int SetString(ref Guid guidKey, [MarshalAs(UnmanagedType.LPWStr)] string wszValue);
            [PreserveSig] int SetBlob(ref Guid guidKey, byte[] pbBuf, uint cbBufSize);
            [PreserveSig] int SetUnknown(ref Guid guidKey, [MarshalAs(UnmanagedType.IUnknown)] object pUnknown);
            [PreserveSig] int LockStore();
            [PreserveSig] int UnlockStore();
            [PreserveSig] int GetCount(out uint pcItems);
            [PreserveSig] int GetItemByIndex(uint unIndex, out Guid pguidKey, IntPtr pValue);
            [PreserveSig] int CopyAllItems(IMFAttributes pDest);
        }

        [ComImport]
        [Guid("7fee9e9a-4a89-47a6-899c-b6a53a70fb67")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFActivate : IMFAttributes
        {
            [PreserveSig] int ActivateObject(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
            [PreserveSig] int DetachObject();
            [PreserveSig] int ShutdownObject();
        }

        [ComImport]
        [Guid("44ae0fa8-ea31-4109-8d2e-4cae4997c555")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFMediaType : IMFAttributes
        {
        }

        [ComImport]
        [Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFSample : IMFAttributes
        {
            [PreserveSig] int GetSampleFlags(out uint pdwSampleFlags);
            [PreserveSig] int SetSampleFlags(uint dwSampleFlags);
            [PreserveSig] int GetSampleTime(out long pllSampleTime);
            [PreserveSig] int SetSampleTime(long llSampleTime);
            [PreserveSig] int GetSampleDuration(out long pllSampleDuration);
            [PreserveSig] int SetSampleDuration(long llSampleDuration);
            [PreserveSig] int GetBufferCount(out uint pdwBufferCount);
            [PreserveSig] int GetBufferByIndex(uint dwIndex, out IMFMediaBuffer ppBuffer);
            [PreserveSig] int ConvertToContiguousBuffer(out IMFMediaBuffer ppBuffer);
            [PreserveSig] int AddBuffer(IMFMediaBuffer pBuffer);
            [PreserveSig] int RemoveBufferByIndex(uint dwIndex);
            [PreserveSig] int RemoveAllBuffers();
            [PreserveSig] int GetTotalLength(out uint pdwTotalLength);
            [PreserveSig] int CopyToBuffer(IMFMediaBuffer pBuffer);
        }

        [ComImport]
        [Guid("045fa593-8799-42b8-bc8d-8968c6453507")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFMediaBuffer
        {
            [PreserveSig] int Lock(out IntPtr ppbBuffer, out uint pcbMaxLength, out uint pcbCurrentLength);
            [PreserveSig] int Unlock();
            [PreserveSig] int GetCurrentLength(out uint pcbCurrentLength);
            [PreserveSig] int SetCurrentLength(uint cbCurrentLength);
            [PreserveSig] int GetMaxLength(out uint pcbMaxLength);
        }

        [ComImport]
        [Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMFSourceReader
        {
            [PreserveSig] int GetStreamSelection(uint dwStreamIndex, out bool pfSelected);
            [PreserveSig] int SetStreamSelection(uint dwStreamIndex, bool fSelected);
            [PreserveSig] int GetNativeMediaType(uint dwStreamIndex, uint dwMediaTypeIndex, out IMFMediaType ppMediaType);
            [PreserveSig] int GetCurrentMediaType(uint dwStreamIndex, out IMFMediaType ppMediaType);
            [PreserveSig] int SetCurrentMediaType(uint dwStreamIndex, IntPtr pdwReserved, IMFMediaType pMediaType);
            [PreserveSig] int SetCurrentPosition(ref Guid guidTimeFormat, IntPtr pvarPosition);
            [PreserveSig] int ReadSample(uint dwStreamIndex, uint dwFlags, out uint pdwActualStreamIndex, out uint pdwStreamFlags, out long pllTimestamp, out IMFSample ppSample);
            [PreserveSig] int Flush(uint dwStreamIndex);
            [PreserveSig] int GetServiceForStream(uint dwStreamIndex, ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
            [PreserveSig] int GetPresentationAttribute(uint dwStreamIndex, ref Guid guidAttribute, IntPtr pvarAttribute);
        }

        [DllImport("mfplat.dll", ExactSpelling = true)]
        private static extern int MFStartup(int version, int dwFlags);

        [DllImport("mfplat.dll", ExactSpelling = true)]
        private static extern int MFShutdown();

        [DllImport("mfplat.dll", ExactSpelling = true)]
        private static extern int MFCreateAttributes(out IMFAttributes ppMFAttributes, int cInitialSize);

        [DllImport("mfplat.dll", ExactSpelling = true)]
        private static extern int MFCreateMediaType(out IMFMediaType ppMFType);

        [DllImport("mf.dll", ExactSpelling = true, EntryPoint = "MFEnumDeviceSources")]
        private static extern int MFEnumDeviceSources(IMFAttributes pAttributes, out IntPtr pppDevices, out int pcDevices);

        [DllImport("mf.dll", ExactSpelling = true, EntryPoint = "MFCreateDeviceSource")]
        private static extern int MFCreateDeviceSource(IMFAttributes pAttributes, [MarshalAs(UnmanagedType.IUnknown)] out object ppSource);

        [DllImport("mfreadwrite.dll", ExactSpelling = true, EntryPoint = "MFCreateSourceReaderFromMediaSource")]
        private static extern int MFCreateSourceReaderFromMediaSource([MarshalAs(UnmanagedType.IUnknown)] object pMediaSource, IMFAttributes pAttributes, out IMFSourceReader ppSourceReader);

        #endregion
    }
}
