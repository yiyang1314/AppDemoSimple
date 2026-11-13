using System;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Structs
{
    using TPCANHandle = System.UInt16;
    using TPCANBitrateFD = System.String;
    using TPCANTimestampFD = System.UInt64;
    public struct PCAN
    {
        #region Structures
        /// <summary>
        /// Represents a PCAN message
        /// </summary>
        public struct TPCANMsg
        {
            /// <summary>
            /// 11/29-bit message identifier
            /// </summary>
            public uint ID;
            /// <summary>
            /// Type of the message
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            public TPCANMessageType MSGTYPE;
            /// <summary>
            /// Data Length Code of the message (0..8)
            /// </summary>
            public byte LEN;
            /// <summary>
            /// Data of the message (DATA[0]..DATA[7])
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] DATA;
        }

        /// <summary>
        /// Represents a timestamp of a received PCAN message.
        /// Total Microseconds = micros + 1000 * millis + 0x100000000 * 1000 * millis_overflow
        /// </summary>
        public struct TPCANTimestamp
        {
            /// <summary>
            /// Base-value: milliseconds: 0.. 2^32-1
            /// </summary>
            public uint millis;
            /// <summary>
            /// Roll-arounds of millis
            /// </summary>
            public ushort millis_overflow;
            /// <summary>
            /// Microseconds: 0..999
            /// </summary>
            public ushort micros;
        }

        /// <summary>
        /// Represents a PCAN message from a FD capable hardware
        /// </summary>
        public struct TPCANMsgFD
        {
            /// <summary>
            /// 11/29-bit message identifier
            /// </summary>
            public uint ID;
            /// <summary>
            /// Type of the message
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            public TPCANMessageType MSGTYPE;
            /// <summary>
            /// Data Length Code of the message (0..15)
            /// </summary>
            public byte DLC;
            /// <summary>
            /// Data of the message (DATA[0]..DATA[63])
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] DATA;
        }

        /// <summary>
        /// Describes an available PCAN channel
        /// </summary>
        public struct TPCANChannelInformation
        {
            /// <summary>
            /// PCAN channel handle
            /// </summary>
            [MarshalAs(UnmanagedType.U2)]
            public TPCANHandle channel_handle;
            /// <summary>
            /// Kind of PCAN device
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            public TPCANDevice device_type;
            /// <summary>
            /// CAN-Controller number
            /// </summary>
            public byte controller_number;
            /// <summary>
            /// Device capabilities flag (see FEATURE_*)
            /// </summary>
            public uint device_features;
            /// <summary>
            /// Device name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string device_name;
            /// <summary>
            /// Device number
            /// </summary>
            public uint device_id;
            /// <summary>
            /// Availability status of a PCAN-Channel
            /// </summary>
            public uint channel_condition;
        }
        #endregion
    }

    #region Enumerations
    /// <summary>
    /// Represents a PCAN status/error code
    /// </summary>
    [Flags]
    public enum TPCANStatus : uint
    {
        /// <summary>
        /// No error
        /// </summary>
        PCAN_ERROR_OK = 0x00000,
        /// <summary>
        /// Transmit buffer in CAN controller is full
        /// </summary>
        PCAN_ERROR_XMTFULL = 0x00001,
        /// <summary>
        /// CAN controller was read too late        
        /// </summary>
        PCAN_ERROR_OVERRUN = 0x00002,
        /// <summary>
        /// Bus error: an error counter reached the 'light' limit
        /// </summary>
        PCAN_ERROR_BUSLIGHT = 0x00004,
        /// <summary>
        /// Bus error: an error counter reached the 'heavy' limit
        /// </summary>
        PCAN_ERROR_BUSHEAVY = 0x00008,
        /// <summary>
        /// Bus error: an error counter reached the 'warning' limit
        /// </summary>
        PCAN_ERROR_BUSWARNING = PCAN_ERROR_BUSHEAVY,
        /// <summary>
        /// Bus error: the CAN controller is error passive
        /// </summary>
        PCAN_ERROR_BUSPASSIVE = 0x40000,
        /// <summary>
        /// Bus error: the CAN controller is in bus-off state
        /// </summary>
        PCAN_ERROR_BUSOFF = 0x00010,
        /// <summary>
        /// Mask for all bus errors
        /// </summary>
        PCAN_ERROR_ANYBUSERR = (PCAN_ERROR_BUSWARNING | PCAN_ERROR_BUSLIGHT | PCAN_ERROR_BUSHEAVY | PCAN_ERROR_BUSOFF | PCAN_ERROR_BUSPASSIVE),
        /// <summary>
        /// Receive queue is empty
        /// </summary>
        PCAN_ERROR_QRCVEMPTY = 0x00020,
        /// <summary>
        /// Receive queue was read too late
        /// </summary>
        PCAN_ERROR_QOVERRUN = 0x00040,
        /// <summary>
        /// Transmit queue is full
        /// </summary>
        PCAN_ERROR_QXMTFULL = 0x00080,
        /// <summary>
        /// Test of the CAN controller hardware registers failed (no hardware found)
        /// </summary>
        PCAN_ERROR_REGTEST = 0x00100,
        /// <summary>
        /// Driver not loaded
        /// </summary>
        PCAN_ERROR_NODRIVER = 0x00200,
        /// <summary>
        /// Hardware already in use by a Net
        /// </summary>
        PCAN_ERROR_HWINUSE = 0x00400,
        /// <summary>
        /// A Client is already connected to the Net
        /// </summary>
        PCAN_ERROR_NETINUSE = 0x00800,
        /// <summary>
        /// Hardware handle is invalid
        /// </summary>
        PCAN_ERROR_ILLHW = 0x01400,
        /// <summary>
        /// Net handle is invalid
        /// </summary>
        PCAN_ERROR_ILLNET = 0x01800,
        /// <summary>
        /// Client handle is invalid
        /// </summary>
        PCAN_ERROR_ILLCLIENT = 0x01C00,
        /// <summary>
        /// Mask for all handle errors
        /// </summary>
        PCAN_ERROR_ILLHANDLE = (PCAN_ERROR_ILLHW | PCAN_ERROR_ILLNET | PCAN_ERROR_ILLCLIENT),
        /// <summary>
        /// Resource (FIFO, Client, timeout) cannot be created
        /// </summary>
        PCAN_ERROR_RESOURCE = 0x02000,
        /// <summary>
        /// Invalid parameter
        /// </summary>
        PCAN_ERROR_ILLPARAMTYPE = 0x04000,
        /// <summary>
        /// Invalid parameter value
        /// </summary>
        PCAN_ERROR_ILLPARAMVAL = 0x08000,
        /// <summary>
        /// Unknown error
        /// </summary>
        PCAN_ERROR_UNKNOWN = 0x10000,
        /// <summary>
        /// Invalid data, function, or action.
        /// </summary>
        PCAN_ERROR_ILLDATA = 0x20000,
        /// <summary>
        /// Driver object state is wrong for the attempted operation
        /// </summary>
        PCAN_ERROR_ILLMODE = 0x80000,
        /// <summary>
        /// An operation was successfully carried out, however, irregularities were registered
        /// </summary>
        PCAN_ERROR_CAUTION = 0x2000000,
        /// <summary>
        /// Channel is not initialized
        /// <remarks>Value was changed from 0x40000 to 0x4000000</remarks>
        /// </summary>
        PCAN_ERROR_INITIALIZE = 0x4000000,
        /// <summary>
        /// Invalid operation
        /// <remarks>Value was changed from 0x80000 to 0x8000000</remarks>
        /// </summary>
        PCAN_ERROR_ILLOPERATION = 0x8000000,
    }

    /// <summary>
    /// Represents a PCAN device
    /// </summary>
    public enum TPCANDevice : byte
    {
        /// <summary>
        /// Undefined, unknown or not selected PCAN device value
        /// </summary>
        PCAN_NONE = 0,
        /// <summary>
        /// PCAN Non-PnP devices. NOT USED WITHIN PCAN-Basic API
        /// </summary>
        PCAN_PEAKCAN = 1,
        /// <summary>
        /// PCAN-ISA, PCAN-PC/104, and PCAN-PC/104-Plus
        /// </summary>
        PCAN_ISA = 2,
        /// <summary>
        /// PCAN-Dongle
        /// </summary>
        PCAN_DNG = 3,
        /// <summary>
        /// PCAN-PCI, PCAN-cPCI, PCAN-miniPCI, and PCAN-PCI Express
        /// </summary>
        PCAN_PCI = 4,
        /// <summary>
        /// PCAN-USB and PCAN-USB Pro
        /// </summary>
        PCAN_USB = 5,
        /// <summary>
        /// PCAN-PC Card
        /// </summary>
        PCAN_PCC = 6,
        /// <summary>
        /// PCAN Virtual hardware. NOT USED WITHIN PCAN-Basic API
        /// </summary>
        PCAN_VIRTUAL = 7,
        /// <summary>
        /// PCAN Gateway devices
        /// </summary>
        PCAN_LAN = 8
    }

    /// <summary>
    /// Represents a PCAN parameter to be read or set
    /// </summary>
    public enum TPCANParameter : byte
    {
        /// <summary>
        /// Device identifier parameter
        /// </summary>
        PCAN_DEVICE_ID = 1,
        /// <summary>
        /// DEPRECATED parameter. Use PCAN_DEVICE_ID instead
        /// </summary>
        [Obsolete]
        PCAN_DEVICE_NUMBER = PCAN_DEVICE_ID,
        /// <summary>
        /// 5-Volt power parameter
        /// </summary>
        PCAN_5VOLTS_POWER = 2,
        /// <summary>
        /// PCAN receive event handler parameter
        /// </summary>
        PCAN_RECEIVE_EVENT = 3,
        /// <summary>
        /// PCAN message filter parameter
        /// </summary>
        PCAN_MESSAGE_FILTER = 4,
        /// <summary>
        /// PCAN-Basic API version parameter
        /// </summary>
        PCAN_API_VERSION = 5,
        /// <summary>
        /// PCAN device channel version parameter
        /// </summary>
        PCAN_CHANNEL_VERSION = 6,
        /// <summary>
        /// PCAN Reset-On-Busoff parameter
        /// </summary>
        PCAN_BUSOFF_AUTORESET = 7,
        /// <summary>
        /// PCAN Listen-Only parameter
        /// </summary>
        PCAN_LISTEN_ONLY = 8,
        /// <summary>
        /// Directory path for log files
        /// </summary>
        PCAN_LOG_LOCATION = 9,
        /// <summary>
        /// Debug-Log activation status
        /// </summary>
        PCAN_LOG_STATUS = 10,
        /// <summary>
        /// Configuration of the debugged information (LOG_FUNCTION_***)
        /// </summary>
        PCAN_LOG_CONFIGURE = 11,
        /// <summary>
        /// Custom insertion of text into the log file
        /// </summary>
        PCAN_LOG_TEXT = 12,
        /// <summary>
        /// Availability status of a PCAN-Channel
        /// </summary>
        PCAN_CHANNEL_CONDITION = 13,
        /// <summary>
        /// PCAN hardware name parameter
        /// </summary>
        PCAN_HARDWARE_NAME = 14,
        /// <summary>
        /// Message reception status of a PCAN-Channel
        /// </summary>
        PCAN_RECEIVE_STATUS = 15,
        /// <summary>
        /// CAN-Controller number of a PCAN-Channel
        /// </summary>
        PCAN_CONTROLLER_NUMBER = 16,
        /// <summary>
        /// Directory path for PCAN trace files
        /// </summary>
        PCAN_TRACE_LOCATION = 17,
        /// <summary>
        /// CAN tracing activation status
        /// </summary>
        PCAN_TRACE_STATUS = 18,
        /// <summary>
        /// Configuration of the maximum file size of a CAN trace
        /// </summary>
        PCAN_TRACE_SIZE = 19,
        /// <summary>
        /// Configuration of the trace file storing mode (TRACE_FILE_***)
        /// </summary>
        PCAN_TRACE_CONFIGURE = 20,
        /// <summary>
        /// Physical identification of a USB based PCAN-Channel by blinking its associated LED
        /// </summary>
        PCAN_CHANNEL_IDENTIFYING = 21,
        /// <summary>
        /// Capabilities of a PCAN device (FEATURE_***)
        /// </summary>
        PCAN_CHANNEL_FEATURES = 22,
        /// <summary>
        /// Using of an existing bit rate (PCAN-View connected to a channel)
        /// </summary>
        PCAN_BITRATE_ADAPTING = 23,
        /// <summary>
        /// Configured bit rate as Btr0Btr1 value
        /// </summary>
        PCAN_BITRATE_INFO = 24,
        /// <summary>
        /// Configured bit rate as TPCANBitrateFD string
        /// </summary>
        PCAN_BITRATE_INFO_FD = 25,
        /// <summary>
        /// Configured nominal CAN Bus speed as Bits per seconds
        /// </summary>
        PCAN_BUSSPEED_NOMINAL = 26,
        /// <summary>
        /// Configured CAN data speed as Bits per seconds
        /// </summary>
        PCAN_BUSSPEED_DATA = 27,
        /// <summary>
        /// Remote address of a LAN channel as string in IPv4 format
        /// </summary>
        PCAN_IP_ADDRESS = 28,
        /// <summary>
        /// Status of the Virtual PCAN-Gateway Service 
        /// </summary>
        PCAN_LAN_SERVICE_STATUS = 29,
        /// <summary>
        /// Status messages reception status within a PCAN-Channel
        /// </summary>
        PCAN_ALLOW_STATUS_FRAMES = 30,
        /// <summary>
        /// RTR messages reception status within a PCAN-Channel
        /// </summary>
        PCAN_ALLOW_RTR_FRAMES = 31,
        /// <summary>
        /// Error messages reception status within a PCAN-Channel
        /// </summary>
        PCAN_ALLOW_ERROR_FRAMES = 32,
        /// <summary>
        /// Delay, in microseconds, between sending frames
        /// </summary>
        PCAN_INTERFRAME_DELAY = 33,
        /// <summary>
        /// Filter over code and mask patterns for 11-Bit messages
        /// </summary>
        PCAN_ACCEPTANCE_FILTER_11BIT = 34,
        /// <summary>
        /// Filter over code and mask patterns for 29-Bit messages
        /// </summary>
        PCAN_ACCEPTANCE_FILTER_29BIT = 35,
        /// <summary>
        /// Output mode of 32 digital I/O pin of a PCAN-USB Chip. 1: Output-Active 0 : Output Inactive
        /// </summary>
        PCAN_IO_DIGITAL_CONFIGURATION = 36,
        /// <summary>
        /// Value assigned to a 32 digital I/O pins of a PCAN-USB Chip
        /// </summary>
        PCAN_IO_DIGITAL_VALUE = 37,
        /// <summary>
        /// Value assigned to a 32 digital I/O pins of a PCAN-USB Chip - Multiple digital I/O pins to 1 = High
        /// </summary>
        PCAN_IO_DIGITAL_SET = 38,
        /// <summary>
        /// Clear multiple digital I/O pins to 0
        /// </summary>
        PCAN_IO_DIGITAL_CLEAR = 39,
        /// <summary>
        /// Get value of a single analog input pin
        /// </summary>
        PCAN_IO_ANALOG_VALUE = 40,
        /// <summary>
        /// Get the version of the firmware used by the device associated with a PCAN-Channel
        /// </summary>
        PCAN_FIRMWARE_VERSION = 41,
        /// <summary>
        /// Get the amount of PCAN channels attached to a system
        /// </summary>
        PCAN_ATTACHED_CHANNELS_COUNT = 42,
        /// <summary>
        /// Get information about PCAN channels attached to a system
        /// </summary>
        PCAN_ATTACHED_CHANNELS = 43,
    }

    /// <summary>
    /// Represents the type of a PCAN message
    /// </summary>
    [Flags]
    public enum TPCANMessageType : byte
    {
        /// <summary>
        /// The PCAN message is a CAN Standard Frame (11-bit identifier)
        /// </summary>
        PCAN_MESSAGE_STANDARD = 0x00,
        /// <summary>
        /// The PCAN message is a CAN Remote-Transfer-Request Frame
        /// </summary>
        PCAN_MESSAGE_RTR = 0x01,
        /// <summary>
        /// The PCAN message is a CAN Extended Frame (29-bit identifier)
        /// </summary>
        PCAN_MESSAGE_EXTENDED = 0x02,
        /// <summary>
        /// The PCAN message represents a FD frame in terms of CiA Specs
        /// </summary>
        PCAN_MESSAGE_FD = 0x04,
        /// <summary>
        /// The PCAN message represents a FD bit rate switch (CAN data at a higher bit rate)
        /// </summary>
        PCAN_MESSAGE_BRS = 0x08,
        /// <summary>
        /// The PCAN message represents a FD error state indicator(CAN FD transmitter was error active)
        /// </summary>
        PCAN_MESSAGE_ESI = 0x10,
        /// <summary>
        /// The PCAN message represents an error frame
        /// </summary>
        PCAN_MESSAGE_ERRFRAME = 0x40,
        /// <summary>
        /// The PCAN message represents a PCAN status message
        /// </summary>
        PCAN_MESSAGE_STATUS = 0x80,
    }

    /// <summary>
    /// Represents a PCAN filter mode
    /// </summary>
    public enum TPCANMode : byte
    {
        /// <summary>
        /// Mode is Standard (11-bit identifier)
        /// </summary>
        PCAN_MODE_STANDARD = TPCANMessageType.PCAN_MESSAGE_STANDARD,
        /// <summary>
        /// Mode is Extended (29-bit identifier)
        /// </summary>
        PCAN_MODE_EXTENDED = TPCANMessageType.PCAN_MESSAGE_EXTENDED,
    }

    /// <summary>
    /// Represents a PCAN Baud rate register value
    /// </summary>
    public enum TPCANBaudrate : ushort
    {
        /// <summary>
        /// 1 MBit/s
        /// </summary>
        PCAN_BAUD_1M = 0x0014,
        /// <summary>
        /// 800 KBit/s
        /// </summary>
        PCAN_BAUD_800K = 0x0016,
        /// <summary>
        /// 500 kBit/s
        /// </summary>
        PCAN_BAUD_500K = 0x001C,
        /// <summary>
        /// 250 kBit/s
        /// </summary>
        PCAN_BAUD_250K = 0x011C,
        /// <summary>
        /// 125 kBit/s
        /// </summary>
        PCAN_BAUD_125K = 0x031C,
        /// <summary>
        /// 100 kBit/s
        /// </summary>
        PCAN_BAUD_100K = 0x432F,
        /// <summary>
        /// 95,238 KBit/s
        /// </summary>
        PCAN_BAUD_95K = 0xC34E,
        /// <summary>
        /// 83,333 KBit/s
        /// </summary>
        PCAN_BAUD_83K = 0x852B,
        /// <summary>
        /// 50 kBit/s
        /// </summary>
        PCAN_BAUD_50K = 0x472F,
        /// <summary>
        /// 47,619 KBit/s
        /// </summary>
        PCAN_BAUD_47K = 0x1414,
        /// <summary>
        /// 33,333 KBit/s
        /// </summary>
        PCAN_BAUD_33K = 0x8B2F,
        /// <summary>
        /// 20 kBit/s
        /// </summary>
        PCAN_BAUD_20K = 0x532F,
        /// <summary>
        /// 10 kBit/s
        /// </summary>
        PCAN_BAUD_10K = 0x672F,
        /// <summary>
        /// 5 kBit/s
        /// </summary>
        PCAN_BAUD_5K = 0x7F7F,
    }

    /// <summary>
    /// Represents the type of PCAN (Non-PnP) hardware to be initialized
    /// </summary>
    public enum TPCANType : byte
    {
        /// <summary>
        /// PCAN-ISA 82C200
        /// </summary>
        PCAN_TYPE_ISA = 0x01,
        /// <summary>
        /// PCAN-ISA SJA1000
        /// </summary>
        PCAN_TYPE_ISA_SJA = 0x09,
        /// <summary>
        /// PHYTEC ISA 
        /// </summary>
        PCAN_TYPE_ISA_PHYTEC = 0x04,
        /// <summary>
        /// PCAN-Dongle 82C200
        /// </summary>
        PCAN_TYPE_DNG = 0x02,
        /// <summary>
        /// PCAN-Dongle EPP 82C200
        /// </summary>
        PCAN_TYPE_DNG_EPP = 0x03,
        /// <summary>
        /// PCAN-Dongle SJA1000
        /// </summary>
        PCAN_TYPE_DNG_SJA = 0x05,
        /// <summary>
        /// PCAN-Dongle EPP SJA1000
        /// </summary>
        PCAN_TYPE_DNG_SJA_EPP = 0x06,
    }
    #endregion
}
