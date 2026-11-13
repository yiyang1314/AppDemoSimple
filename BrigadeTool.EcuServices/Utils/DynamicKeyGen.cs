using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using static Rxbit.Brigade.EcuServices.Utils.DynamicKeyGen;

namespace Rxbit.Brigade.EcuServices.Utils
{
    /// <summary>
    /// ZLGKey 动态库封装器
    /// </summary>
    public class ZlgKeyDllWrapper
    {
        #region Win32 API 动态加载DLL
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        #endregion

        #region ZLGKey 委托定义【严格匹配C原型】
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ZLGKeyDelegate(
            byte[] seedArray,
            ushort seedArraySize,
            uint securityLevel,
            [MarshalAs(UnmanagedType.LPStr)] string variant,
            byte[] keyArray,
            ref ushort keyArraySize
        );
        #endregion

        private IntPtr _dllHandle = IntPtr.Zero;
        private ZLGKeyDelegate _zlgKeyFunc;

        /// <summary>
        /// DLL是否加载成功
        /// </summary>
        public bool IsLoaded => _dllHandle != IntPtr.Zero;

        /// <summary>
        /// 加载指定路径SecurityLevel13.dll
        /// </summary>
        /// <param name="dllFullPath">dll完整路径</param>
        /// <returns>true加载成功</returns>
        public bool LoadDll(string dllFullPath)
        {
            if (!File.Exists(dllFullPath))
            {
                Console.WriteLine("DLL文件不存在");
                return false;
            }
            UnloadDll();

            // 加载32位DLL，项目必须设置平台目标 x86
            _dllHandle = LoadLibrary(dllFullPath);
            if (_dllHandle == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                Console.WriteLine($"LoadLibrary失败，错误码:{err}");
                return false;
            }

            // 获取ZLGKey函数地址
            IntPtr funcPtr = GetProcAddress(_dllHandle, "ZLGKey");
            if (funcPtr == IntPtr.Zero)
            {
                Console.WriteLine("找不到导出函数 ZLGKey");
                UnloadDll();
                return false;
            }

            _zlgKeyFunc = Marshal.GetDelegateForFunctionPointer<ZLGKeyDelegate>(funcPtr);
            Console.WriteLine("ZLGKey函数加载成功");
            return true;
        }

        /// <summary>
        /// 释放DLL，解除文件占用
        /// </summary>
        public void UnloadDll()
        {
            if (_dllHandle != IntPtr.Zero)
            {
                FreeLibrary(_dllHandle);
                _dllHandle = IntPtr.Zero;
            }
            _zlgKeyFunc = null;
        }

        /// <summary>
        /// 调用DLL：Seed -> Key
        /// </summary>
        /// <param name="seed">ECU返回种子</param>
        /// <param name="securityLevel">安全等级</param>
        /// <param name="variant">变体参数，传null</param>
        /// <param name="key">输出密钥</param>
        /// <returns>DLL返回码，0=成功</returns>
        public int UnlockSeek(string dllFullPath,byte[] seed, uint securityLevel, string variant, out byte[] key)
        {
            LoadDll(dllFullPath);
            key = Array.Empty<byte>();
            if (!IsLoaded)
                throw new InvalidOperationException("请先加载DLL！");

            // 密钥缓冲区，按需调整最大长度（一般16/32字节足够）
            const ushort MaxKeyBufferLen = 32;
            byte[] keyBuffer = new byte[MaxKeyBufferLen];
            ushort keyLen = MaxKeyBufferLen;

            int retCode = _zlgKeyFunc(seed, (ushort)seed.Length, securityLevel, variant, keyBuffer, ref keyLen);

            if (retCode == 0 && keyLen > 0)
            {
                key = new byte[keyLen];
                Array.Copy(keyBuffer, key, keyLen);
            }
            return retCode;
        }
    }


    public class DynamicKeyGen
    {
            #region Win32 API
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FreeLibrary(IntPtr hModule);
            #endregion

            #region 委托定义
            /// <summary>
            /// GenerateKeyEx 委托
            /// </summary>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int GenerateKeyExDelegate(
                byte[] seedArray,
                ushort seedArraySize,
                uint securityLevel,
                [MarshalAs(UnmanagedType.LPStr)] string variant,
                byte[] ioKeyArray,
                uint iKeyArraySize,
                ref uint oSize
            );

            /// <summary>
            /// ZLGKey 委托
            /// </summary>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int ZLGKeyDelegate(
                byte[] seedArray,
                ushort seedArraySize,
                uint securityLevel,
                [MarshalAs(UnmanagedType.LPStr)] string variant,
                byte[] keyArray,
                ref ushort keyArraySize
            );
            #endregion

            private IntPtr _dllHandle = IntPtr.Zero;
            private GenerateKeyExDelegate _generateKeyExFunc;
            private ZLGKeyDelegate _zlgKeyFunc;

            public bool IsLoaded => _dllHandle != IntPtr.Zero;
            public bool HasGenerateKeyEx => _generateKeyExFunc != null;
            public bool HasZLGKey => _zlgKeyFunc != null;

            public static bool UnlockSeek(string dllPath, byte[] seedArray, uint securityLevel, out byte[] dada)
            {
                // 1. 自定义 DLL 路径和名称（可从配置文件、用户输入等动态获取）
                // string dllPath = "D:\\tang\\vrp-workItems\\压力台架\\KY_DLL_Creat\\Release\\KY_x86_19.dll"; // 替换为实际路径
                dada = new byte[4];
                var keyGen = new DynamicKeyGen();
                try
                {
                    if (!keyGen.LoadDll(dllPath))
                    {
                        return false;
                    }
                    int ret = -1;
                    if (keyGen.HasGenerateKeyEx)
                    {
                        ret = keyGen.GenerateKeyEx(seedArray, securityLevel, null, out dada);
                    }
                    else if (keyGen.HasZLGKey)
                    {
                        ret = keyGen.ZLGKey(seedArray, securityLevel, null, out dada);
                    }
                    // 程序退出务必释放
                    keyGen.UnloadDll();
                    return ret == 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnlockSeek错误：{ex.Message}");
                }
                finally
                {
                    // 释放资源
                    keyGen.UnloadDll();
                }
                return false;
            }

            /// <summary>
            /// 加载DLL，自动识别两个函数
            /// </summary>
            public bool LoadDll(string dllFullPath)
            {
                if (!File.Exists(dllFullPath))
                {
                    Console.WriteLine("DLL不存在：" + dllFullPath);
                    return false;
                }
                UnloadDll();

                _dllHandle = LoadLibrary(dllFullPath);
                if (_dllHandle == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    Console.WriteLine($"LoadLibrary失败，错误码：{err}");
                    return false;
                }

                // 尝试加载 GenerateKeyEx
                IntPtr pGenEx = GetProcAddress(_dllHandle, "GenerateKeyEx");
                if (pGenEx != IntPtr.Zero)
                {
                    _generateKeyExFunc = Marshal.GetDelegateForFunctionPointer<GenerateKeyExDelegate>(pGenEx);
                    Console.WriteLine("成功加载函数：GenerateKeyEx");
                    return true;
                }
                else
                {
                    Console.WriteLine("DLL未找到 GenerateKeyEx");
                }

                // 尝试加载 ZLGKey
                IntPtr pZlgKey = GetProcAddress(_dllHandle, "ZLGKey");
                if (pZlgKey == IntPtr.Zero)
                {
                    Console.WriteLine("DLL未找到 ZLGKey");
                    return false;
                }
                _zlgKeyFunc = Marshal.GetDelegateForFunctionPointer<ZLGKeyDelegate>(pZlgKey);
                if (_zlgKeyFunc == null)
                {
                    UnloadDll();
                    Console.WriteLine("DLL内无可用密钥函数！");
                    return false;
                }
                Console.WriteLine("成功加载函数：ZLGKey");
                return true;
            }

            /// <summary>
            /// 释放DLL
            /// </summary>
            public void UnloadDll()
            {
                if (_dllHandle != IntPtr.Zero)
                {
                    FreeLibrary(_dllHandle);
                    _dllHandle = IntPtr.Zero;
                }
                _generateKeyExFunc = null;
                _zlgKeyFunc = null;
            }

            #region 对外调用方法
            /// <summary>
            /// 调用 GenerateKeyEx
            /// 返回0成功
            /// </summary>
            public int GenerateKeyEx(byte[] seed, uint securityLevel, string variant, out byte[] key)
            {
                key = Array.Empty<byte>();
                if (!IsLoaded || _generateKeyExFunc == null)
                    throw new NotSupportedException("未加载GenerateKeyEx函数");

                const uint MaxBuf = 64;
                byte[] keyBuf = new byte[MaxBuf];
                uint outSize = 0;

                int ret = _generateKeyExFunc(
                    seed,
                    (ushort)seed.Length,
                    securityLevel,
                    variant,
                    keyBuf,
                    MaxBuf,
                    ref outSize
                );

                if (ret == 0 && outSize > 0)
                {
                    key = new byte[outSize];
                    Array.Copy(keyBuf, key, outSize);
                }
                return ret;
            }

            /// <summary>
            /// 调用 ZLGKey
            /// 返回0成功
            /// </summary>
            public int ZLGKey(byte[] seed, uint securityLevel, string variant, out byte[] key)
            {
                key = Array.Empty<byte>();
                if (!IsLoaded || _zlgKeyFunc == null)
                    throw new NotSupportedException("未加载ZLGKey函数");

                const ushort MaxBuf = 64;
                byte[] keyBuf = new byte[MaxBuf];
                ushort keyLen = MaxBuf;

                int ret = _zlgKeyFunc(seed, (ushort)seed.Length, securityLevel, variant, keyBuf, ref keyLen);

                if (ret == 0 && keyLen > 0)
                {
                    key = new byte[keyLen];
                    Array.Copy(keyBuf, key, keyLen);
                }
                return ret;
            }
            #endregion
        }
    
}
