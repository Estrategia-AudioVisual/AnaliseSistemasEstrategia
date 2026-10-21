using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace ResetExplode;

/// <summary>Dispositivo USB/serial registrado no Windows (conectado ou ausente).</summary>
public sealed class UsbDevice
{
    public string InstanceId { get; init; } = "";
    public string Name { get; init; } = "";
    public string? Port { get; init; }
    public bool Present { get; init; }
    public string? Maker { get; init; }   // Arduino, CH340, FTDI... (pelo VID)
}

/// <summary>
/// Lista os dispositivos USB e seriais registrados no Windows (inclusive os ausentes). Comparando duas listas,
/// os avisos de dispositivo sabem o que foi conectado ou removido.
/// </summary>
public static class UsbDevices
{
    /// <summary>Classes acompanhadas pelos avisos (USB, portas seriais e drivers USB genéricos).</summary>
    private static readonly HashSet<string> Classes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ports", "USB", "USBDevice", "libusbK devices", "libusb-win32 devices", "WinUSB", "Modem",
    };

    /// <summary>Fabricantes por Vendor ID USB (placas e conversores USB-serial comuns).</summary>
    private static readonly Dictionary<string, string> Vendors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["2341"] = "Arduino", ["2A03"] = "Arduino", ["1A86"] = "CH340/CH341", ["0403"] = "FTDI", ["10C4"] = "CP210x",
        ["303A"] = "Espressif (ESP32)", ["239A"] = "Adafruit", ["16C0"] = "Teensy", ["1B4F"] = "SparkFun",
        ["2E8A"] = "Raspberry Pi Pico", ["067B"] = "Prolific PL2303", ["0483"] = "STMicro", ["1366"] = "SEGGER",
    };

    public static List<UsbDevice> List()
    {
        var list = new List<UsbDevice>();
        IntPtr set = SetupDiGetClassDevsW(IntPtr.Zero, null, IntPtr.Zero, DIGCF_ALLCLASSES); // sem DIGCF_PRESENT: inclui ausentes
        if (set == INVALID_HANDLE) throw new Win32Exception();
        try
        {
            var data = new SP_DEVINFO_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>() };
            for (uint i = 0; SetupDiEnumDeviceInfo(set, i, ref data); i++)
            {
                string cls = RegString(set, ref data, SPDRP_CLASS) ?? "";
                string id = InstanceId(set, ref data);
                var vid = System.Text.RegularExpressions.Regex.Match(id, @"VID_([0-9A-F]{4})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                bool knownVid = vid.Success && Vendors.ContainsKey(vid.Groups[1].Value);
                // Placa conectada sem driver não tem classe ("Outros dispositivos"): entra pelo VID conhecido.
                if (!Classes.Contains(cls) && !knownVid) continue;
                bool present = CM_Get_DevNode_Status(out _, out _, data.DevInst, 0) == 0;
                // Hubs e controladores USB raiz não interessam aqui.
                if (cls.Equals("USB", StringComparison.OrdinalIgnoreCase) && (id.StartsWith("USB\\ROOT_HUB") || id.StartsWith("PCI\\"))) continue;

                string name = RegString(set, ref data, SPDRP_FRIENDLYNAME) ?? RegString(set, ref data, SPDRP_DEVICEDESC) ?? id;
                list.Add(new UsbDevice
                {
                    InstanceId = id, Name = name, Present = present, Port = PortName(set, ref data),
                    Maker = knownVid ? Vendors[vid.Groups[1].Value] : null,
                });
            }
        }
        finally { SetupDiDestroyDeviceInfoList(set); }
        return list.OrderBy(d => d.Present).ThenByDescending(d => d.Maker != null).ThenBy(d => d.Port == null).ThenBy(d => d.Name).ToList();
    }

    // ------------------------------------------------------------------ SetupAPI

    private static string InstanceId(IntPtr set, ref SP_DEVINFO_DATA data)
    {
        var sb = new StringBuilder(512);
        return SetupDiGetDeviceInstanceIdW(set, ref data, sb, sb.Capacity, out _) ? sb.ToString() : "";
    }

    private static string? RegString(IntPtr set, ref SP_DEVINFO_DATA data, uint prop)
    {
        var buf = new byte[1024];
        if (!SetupDiGetDeviceRegistryPropertyW(set, ref data, prop, out _, buf, (uint)buf.Length, out uint req)) return null;
        string s = Encoding.Unicode.GetString(buf, 0, (int)Math.Min(req, buf.Length)).TrimEnd('\0');
        return s.Length > 0 ? s : null;
    }

    /// <summary>"PortName" (COMx) fica na chave de parâmetros do dispositivo; existe também para dispositivos ausentes.</summary>
    private static string? PortName(IntPtr set, ref SP_DEVINFO_DATA data)
    {
        IntPtr h = SetupDiOpenDevRegKey(set, ref data, DICS_FLAG_GLOBAL, 0, DIREG_DEV, KEY_READ);
        if (h == INVALID_HANDLE || h == IntPtr.Zero) return null;
        using var key = RegistryKey.FromHandle(new SafeRegistryHandle(h, ownsHandle: true));
        return key.GetValue("PortName") as string;
    }

    private static readonly IntPtr INVALID_HANDLE = new(-1);
    private const int DIGCF_ALLCLASSES = 0x4;
    private const uint SPDRP_DEVICEDESC = 0x0, SPDRP_CLASS = 0x7, SPDRP_FRIENDLYNAME = 0xC;
    private const uint DICS_FLAG_GLOBAL = 0x1, DIREG_DEV = 0x1, KEY_READ = 0x20019;

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA { public uint cbSize; public Guid ClassGuid; public uint DevInst; public IntPtr Reserved; }

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevsW(IntPtr classGuid, string? enumerator, IntPtr hwnd, int flags);
    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr set, uint index, ref SP_DEVINFO_DATA data);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetupDiGetDeviceInstanceIdW(IntPtr set, ref SP_DEVINFO_DATA data, StringBuilder id, int size, out int required);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr set, ref SP_DEVINFO_DATA data, uint prop, out uint regType,
                                                                 byte[] buffer, uint size, out uint required);
    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiOpenDevRegKey(IntPtr set, ref SP_DEVINFO_DATA data, uint scope, uint hwProfile, uint keyType, uint access);
    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Get_DevNode_Status(out uint status, out uint problem, uint devInst, uint flags);
}
