# Backsense BS-9200X Test Tool — User Guide



## 1 Software Package & Getting Started

### 1.1 Check CAN Device Driver

> After connecting the CAN device, check if the CAN device driver is installed on the computer and verify the corresponding CAN device.

![image-20260714171301275](assets/image-20260714171301275.png)

### 1.2 CAN Device Driver Installation

> After confirming the CAN driver is installed, open the corresponding CAN device using the CAN manufacturer's official software. If opened successfully and messages with IDs `0x390`, `0x2XX`, or `0x3XX` are received, it indicates the CAN device configuration and connection are working properly.
>
> Examples:

- Demo 1 --- ZLG CAN

![image-20260714171541577](assets/image-20260714171541577.png)

- Demo 2 --- PEAK CAN

![image-20260714171458727](assets/image-20260714171458727.png)

### 1.3 Getting the Software Package

> After confirming CAN device communication is working properly, obtain the software installer ***Rxbit.BrigadeApp-1.5-x86.msi*** and double-click to install. After installation completes:

![image-20260714170800866](assets/image-20260714170800866.png)

> The ***Rxbit.BrigadeApp*** shortcut can be found in the Start menu or on the desktop.

![image-20260716093540752](assets/image-20260716093540752.png)

> To uninstall or repair the software, open **Settings → Apps → Installed apps**, find **Rxbit.BrigadeApp**, and choose Uninstall (or run the MSI again and select Repair/Remove).

![image-20260716093330393](assets/image-20260716093330393.png)

> For the new View menu entries in V1.5 (Screen Recording, USB Camera Recording, Video Playback) and their operating steps, see the Chinese user manual `使用说明文档.md`, Chapter 8.



## 2  Started Application

### 2.1 Start CAN Devices

> Double-click to launch Rxbit.BrigadeApp. The CAN device configuration interface will open. Configure the corresponding parameters according to the CAN device: manufacturer, device type, device index, channel index, baud rate, CAN protocol, and enable settings. After configuration is complete, click the ***Open Device*** button. Once opened successfully, the radar SensorID will be automatically detected. Only after detecting a SensorID can you navigate to the main application interface.
>
> Function description:
>
> - Open Device: Opens only the CAN device
> - Close Device: Closes the CAN device

![image-20260716085853498](assets/image-20260716085853498.png)

**Parameter Description:**

| Configuration Item | Description | Options |
|-------------------|-------------|---------|
| Manufacturer | CAN device manufacturer | GCAN, ZLGCAN, PEAK, Chuangxin Technology, etc. |
| Device Type | Device model | e.g., GCAN USBCAN FD |
| Device Index | Device index | 0, 1, 2... |
| Channel | Channel number | 0, 1 |
| CAN Protocol | Communication protocol | CAN, CANFD |
| Baud Rate | Baud rate | 500k, 1M, etc. |
| Data Baud Rate | Data baud rate (CANFD) | 2M, 5M, etc. |
| Resistance | Termination resistor | Enabled, Disabled |



### 2.2 Start-up / Empty Screen

>  **Launch the application**
>
> The tool always opens to this empty screen. The aqua-rings branding fills the view and no data windows can be opened yet — the user must first choose a Sensor ID. The status bar (bottom-left) reads **"No sensor selected"**.

![image-20260716085931249](assets/image-20260716085931249.png)

**UI Regions:**

| Region | Description |
|--------|-------------|
| Top menu bar | `File` · ``Device · `View` · `Tools` · `About` |
| Select Sensor dropdown | Empty by default (`None` selected), arrow on the right |
| Central canvas | Aqua concentric rings branding / radar placeholder |
| Hint box (center) | **"No sensor selected — choose a Sensor ID to begin receiving data"** |
| Bottom status bar (left) | Sensor connection state |
| Bottom status bar (right) | Cycle indicator |

---

### 2.3   Select a Sensor

> **View→ click the ▾ arrow**

![image-20260714163819019](assets/image-20260714163819019.png)



The sensor list opens only from the dropdown arrow (**not** by clicking the box). It lists **"None"** plus every available Sensor ID, grouped into two ranges:

| Group | Range | Protocol |
|-------|-------|----------|
| **CAN Type 1 · 3xx** | `0x310`, `0x320`, `0x330`, `0x340` … | Gen1 sensors |
| **CAN Type 2 · 2xx** | `0x210`, `0x220`, `0x230` … | Gen2 sensors |

Choosing any ID (other than None) brings the tool online — the status bar turns green and monitoring windows become available.

**Navigation hint:** Changing the sensor while data windows are open immediately re-points every window at the newly-selected CAN ID.

---

## 3 · Main Navigation

> The top menu bar is divided into 5 main modules: 
>
> `Files` for file operations, 
>
> `Configuration` for parameter configuration, 
>
> `View` for view panels, 
>
> `Tools` for utilities, and 
>
> `Help` for assistance information. 
>
> The following sections describe the functionality of each menu item.

![image-20260714164106880](assets/image-20260714164106880.png)



### 3.1 Files Menu

| Primary Menu             | Secondary Menu Item       | Core Function Description                                   |
| ------------------------ | ------------------------- | ----------------------------------------------------------- |
| **Files** File Operations | Save Config               | Save the current radar sensor configuration file, including CAN parameters, calibration parameters, and UI layout. Automatically loaded on next startup. |
|                          | Open Software WorkDir     | Open the software working root directory for centralized management of logs, caches, configurations, conversion files, and all generated files. |
|                          | Open FlashDriver          | Open the firmware driver directory, which stores BIN/HEX firmware files for radar MCU upgrades used for sensor program flashing. |
|                          | Open Recording Path       | Open the message recording file save directory, which stores CAN ASC messages and radar raw detection data recorded by the software. |
|                          | Open Logs Path            | Open the program runtime log directory, which records software exceptions, CAN communication errors, and operation records for troubleshooting. |
|                          | Open Setting Path         | Open the software global settings directory, which stores global parameters such as UI language, window layout, and CAN hardware port presets. |
|                          | Exit                      | Exit the software and automatically save unsaved configurations and UI layout. |

![image-20260714164609057](assets/image-20260714164609057.png)

### 3.2 Configuration Menu

| Primary Menu              | Secondary Menu Item       | Core Function Description                                   |
| ------------------------- | ------------------------- | ----------------------------------------------------------- |
| **Configuration** Settings | CAN Device Configuration  | CAN bus hardware configuration: select CAN adapter model, baud rate, channel, filter ID, transmit/receive timeout. Establish CAN communication link with radar. |
|                           | UDS Configuration         | UDS diagnostic protocol configuration: set diagnostic ID, service timeout, security access key, programming session parameters. Used for firmware upgrade, DTC reading, and calibration writing. |
|                           | Sensor Configuration      | Radar sensor parameter configuration: 77GHz radar detection threshold, RCS filtering, target confidence, motion classification, occlusion detection, and other algorithm parameter calibration. |

![image-20260714164620003](assets/image-20260714164620003.png)

### 3.3 View Menu

| Primary Menu              | Secondary Menu Item       | Core Function Description                                   |
| ------------------------- | ------------------------- | ----------------------------------------------------------- |
| **View** View Panels      | Birds Eye View           | Bird's-eye radar plan view: 2D visualization interface that real-time draws detected target coordinates, distance, angle, and speed. Provides intuitive display of road target distribution. |
|                           | Detection & Sensor Info   | Target and sensor information panel: table that real-time refreshes all detected target messages while displaying radar hardware status (voltage, temperature, RF faults). |
|                           | Network Summary           | CAN bus network overview: statistics on CAN message transmit/receive rate, message count, bus load, packet loss rate, and bus errors. Monitor communication stability. |
|                           | CAN Live Data             | CAN real-time message monitor: raw hexadecimal message scrolling list that captures all ID data on the bus in real-time. Supports filtering, pause, and ASC file recording. |

![image-20260714164630956](assets/image-20260714164630956.png)



**View menu items:**

| Item | Opens… |
|------|--------|
| ▢ Birds Eye View | Radar plan view (Chapter 3 · 04) |
| ▢ Detection & Sensor Info | Parameter table, one column per detected object (Ch.3 · 05/06) |
| ▢ Network Summary | All 16 sensor base IDs, X rows + Y rows (Ch.3 · 07) |
| ▢ CAN Live Data | Timestamped raw CAN frame log (Ch.3 · 08) |

> 💡 The same four windows can be opened simultaneously for cross-referencing. Selecting any object in one window highlights it in all others at once (see § 3).

---



### 3.4 Tools Menu
| Primary Menu              | Secondary Menu Item       | Core Function Description                                   |
| ------------------------- | ------------------------- | ----------------------------------------------------------- |
| **Tools** Utilities       | File Tools               | Basic file utilities: folder selection, batch file deletion, path validation, log cleanup, and other general file operation functions. |
|                           | ASC to Brigade           | ASC message to radar parsing tool: parse .asc recorded messages exported from CANoe/Peak into software-readable format. Play back historical radar detection data. |
|                           | Convertor Tools          | Format conversion toolbox: BIN↔HEX firmware conversion, byte/string/hex conversion, CRC checksum calculation, message byte order conversion. |

![image-20260714164643924](assets/image-20260714164643924.png)

### 3.5 Help Menu

| Primary Menu              | Secondary Menu Item       | Core Function Description                                   |
| ------------------------- | ------------------------- | ----------------------------------------------------------- |
| **Help** Help Information | About                    | Software about window: displays software version, build time, hardware compatibility models, development copyright, and technical support channels. |
|                           | Docs                     | Open product documentation: includes CAN communication protocol, UDS diagnostic manual, radar parameter description, and tool usage tutorials. |

![image-20260714164710918](assets/image-20260714164710918.png)

## 4 · Monitoring View

> This menu interface is primarily used for receiving and viewing radar track target information, message information, and message recording.

---

### 4.1 Birds Eye View

> This page is primarily used for displaying bird's-eye radar track target motion status information and viewing track details.
>
> Operation steps:
>
> - Click **View → Birds Eye View** to enter the bird's-eye view page
> - Click the Start Recvi button to receive radar track targets



- 3XX

![image-20260716144025220](assets/image-20260716144025220.png)

- 2XX

  ![image-20260714165341360](assets/image-20260714165341360.png)

A plan view of all detections across the **±9 m (lateral) × 65 m (range)** detection area, with the **sensor at the origin (red dot)**.

**Visual legend:**

| Element | Color / Symbol | Meaning |
|---------|---------------|---------|
| Sensor | Red dot (center bottom) | Radar position / origin |
| Objects (unselected) | Grey squares | Detected targets |
| Object (selected) | Cyan (aqua) square | Currently selected object |
| Grid labels | `±8m` (left/right) and `0..65m` (top) | Coordinate axis |
| Object ID | White label above square | e.g. **01 / 02 / 03 / 04** |
| Window title | `Birds Eye View` (top-left X closes) | Floating panel |

**Key behavior:**
Objects are grey until **selected**. Clicking an object here — **or its row / column in any other window** — highlights it **teal everywhere at once** (cross-window highlighting).

---

### 4.2   Detection & Sensor Info (Type 1)

> Reproduces the legacy tool's **parameter-by-object** table for a **Gen1 / Type 1 (3xx range)** sensor. Each detected object is a **column**; parameters run down the left.
>
> Click    **View → Detection & Sensor Info ·**

![image-20260716144113232](assets/image-20260716144113232.png)



**Row layout / parameter list (Gen1 / 3xx):**

| Group | Parameter | Unit |
|-------|-----------|------|
| **Spatial** | Sensor Parameter (header) · Object Distance · Object Angle · X Coordinate · Y Coordinate · Relative X Speed | m · Deg · m · m · KPH |
| **Signal** | Object Signal Level · Object Appearance Status · Object Trigger Event | dB · - · - |
| **Flags** | Detection Flag · Identification Flag | Valid/Invalid · e.g. *77GHz Radar* |
| **Sensor-level faults** | Voltage Fault · MMIC Fault · High Temperature Fault · CAN Fault | OK / NG |

The currently-selected object's entire **column is highlighted in cyan (aqua)**, matching the Birds Eye View and Network Summary.

---

### 4.3  Detection & Sensor Info (Type 2)

> With a **Gen2 / Type 2 (2xx range)** sensor selected the table automatically expands to the next-generation feature set the sensor adds.
>
> Click    **View → Detection & Sensor Info · 2xx sensor**



![image-20260714165458210](assets/image-20260714165458210.png)



**Extra parameters (Gen2 / 2xx) vs Gen1:**

| Group | Added parameters | Unit / values |
|-------|------------------|---------------|
| **Radar signature** | Object RCS · Object Magnitude · Object SNR | m² · n/a · dB |
| **Motion** | Acceleration X · Acceleration Y · Sport Attribute | m/s² · m/s² · Static / Crosswalk / Radial+ / Radial- |
| **Probability / track** | Relative Speed Y · Existence Probability · Alive Count · Classification | KPH · % · frames · Pedestrian / Truck / Bicycle / Vehicle |
| **Frame-level** | Detected Objects · Frame ID · Ground Speed · Sensor Error 5 · Sensor Error 6 · Voltage Fault | count · uint · km/h · OK/NG · OK/NG · OK/NG |

Column highlighting again uses cyan for the currently-selected object, synchronized with the other three live windows.

---

### 4.4   Network Summary

> A **network-wide view of all sixteen possible sensor base IDs** (both **3xx** and **2xx** ranges can appear simultaneously in the same grid).
>
>  Click **View → Network Summary**

- 3XX

![image-20260714165039625](assets/image-20260714165039625.png)

- 2XX

  ![image-20260714165544251](assets/image-20260714165544251.png)

**Layout:**

- Each base CAN ID occupies **two rows**: one **`X`** row + one **`Y`** row.
- Columns are **`Obj_1 … Obj_8`** (one per detected object slot).
- Cells contain the live coordinate value in metres, or **"—"** when no data is present for that slot / sensor.

**Highlighting rules:**

| Highlight | Meaning |
|-----------|---------|
| **Cyan column** | Currently-selected object column (synchronized with Birds Eye & Detection windows) |
| **Live rows** (populated values) | The **currently-connected sensor** rows are highlighted (brighter background) and populated live |
| "—" cells | Sensors that are not currently online / have no data show a dash |

> 💡 This is the only window that shows data from sensors other than the one selected in the main toolbar dropdown (useful for checking a whole vehicle radar network at a glance).

---

### 4.5  CAN Live Data

> A live, **timestamped log of every CAN frame in the session**. White background is used for readability.
>
> click  **View → CAN Live Data**

- 3XX

![image-20260714165105847](assets/image-20260714165105847.png)

- 2XX

  ![image-20260714165655097](assets/image-20260714165655097.png)

**Columns:**

| Column | Contents |
|--------|----------|
| **Time** | `hh:mm:ss.mmm` timestamp of frame reception / transmission |
| **Dir** | `RX` (received from bus) or `TX` (sent by the tool) |
| **CAN ID** | 11/29-bit CAN identifier, hex (e.g. `0x310` · `0x311` … `0x318`) |
| **DLC** | Data length code (bytes in the frame, typically `8`) |
| **Data (B0..B7)** | 8 data bytes, hex, space separated |
| **Message** | Plain-language decoded meaning: `Object 1 — Car`, `Object 2 — Truck`, `Object 3 — Pedestrian`, `Config Frame ID = 0x310`, etc. |

**Toolbar (top of window):**

| Button | Function |
|--------|----------|
| **Pause / Resume** | Freeze the log on-screen while reception continues in the background |
| **Clear** | Wipe the visible buffer (counter keeps incrementing internally) |
| **Export CSV** | Save the current visible log to a comma-separated file |
| **Frame counter (right)** | e.g. **`312 msgs`** — total frames captured since Clear was last pressed |



## 5 Sensor Configuration

### 5.1 Sensor Configuration

![image-20260714164806679](assets/image-20260714164806679.png)

### 5.2 Upgrade Firmware

![image-20260714164827055](assets/image-20260714164827055.png)

## 6 Configuration Menu

### 6.1  UDS Configuration

> 当我们雷达UDS地址变更时，可以通过这个配置界面完成UDS地址配置 和报文录制地址更改

![image-20260716144943248](assets/image-20260716144943248.png)

### 6.2 CAN Device

> 此界面是Brigade APP软件的程序启动入口，同时也可以通过

![image-20260715090332920](assets/image-20260715090332920.png)

## 7 About View

> 本界面功能主要用于查看软件功能更新信息 和版本信息 和发布时间

![image-20260716085749271](assets/image-20260716085749271.png)

