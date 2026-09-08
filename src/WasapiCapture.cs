using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Umc22
{
    [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    class PolicyConfigClient { }

    [ComImport, Guid("F8679F50-850A-41CF-9C72-430F290290C8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bDefault, out IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        [PreserveSig] int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pEndpointFormat, IntPtr mixFormat);
        [PreserveSig] int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bDefault, IntPtr a, IntPtr b);
        [PreserveSig] int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pmftPeriod);
        [PreserveSig] int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pMode);
        [PreserveSig] int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr mode);
        [PreserveSig] int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bFxStore, ref PROPERTYKEY pkey, out PROPVARIANT pv);
        [PreserveSig] int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bFxStore, ref PROPERTYKEY pkey, ref PROPVARIANT pv);
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int role);
        [PreserveSig] int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bVisible);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
        public PROPERTYKEY(Guid f, uint p) { fmtid = f; pid = p; }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public short boolVal;
        [FieldOffset(8)] public uint ulVal;
        [FieldOffset(8)] public IntPtr ptr;
        [FieldOffset(8)] public uint blobSize;
        [FieldOffset(16)] public IntPtr blobData;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WAVEFORMATEXTENSIBLE
    {
        public ushort wFormatTag, nChannels;
        public uint nSamplesPerSec, nAvgBytesPerSec;
        public ushort nBlockAlign, wBitsPerSample, cbSize, wValidBitsPerSample;
        public uint dwChannelMask;
        public Guid SubFormat;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate uint FnRelease(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnEnum(IntPtr self, int dataFlow, int mask, out IntPtr col);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetCount(IntPtr self, out uint count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnItem(IntPtr self, uint index, out IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetId(IntPtr self, out IntPtr name);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnActivate(IntPtr self, ref Guid iid, uint ctx, IntPtr p, out IntPtr pp);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnOpenStore(IntPtr self, int stgm, out IntPtr store);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetDevice(IntPtr self, IntPtr id, out IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetValue(IntPtr self, ref PROPERTYKEY key, out PROPVARIANT pv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnSetValue(IntPtr self, ref PROPERTYKEY key, ref PROPVARIANT pv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnCommit(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnMix(IntPtr self, out IntPtr fmt);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnPeriod(IntPtr self, out long def, out long min);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnIsFmt(IntPtr self, int share, IntPtr fmt, out IntPtr closest);

    public static class WasapiCapture
    {
        const int eCapture = 1;
        const int DEVICE_STATE_ACTIVE = 1;
        const int STGM_READ = 0;
        const int STGM_READWRITE = 2;
        const int CLSCTX_INPROC_SERVER = 1;
        const int AUDCLNT_SHAREMODE_SHARED = 0;
        const int AUDCLNT_SHAREMODE_EXCLUSIVE = 1;
        const ushort WAVE_FORMAT_EXTENSIBLE = 0xFFFE;
        const ushort VT_EMPTY = 0;
        const ushort VT_BOOL = 11;
        const ushort VT_UI4 = 19;
        const ushort VT_LPWSTR = 31;
        const ushort VT_BLOB = 65;

        static readonly Guid CLSID_MMDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        static readonly Guid IID_IMMDeviceEnumerator = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
        static readonly Guid IID_IAudioClient = new Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
        static readonly Guid KSDATAFORMAT_SUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00AA00389B71");
        static readonly PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
        static readonly PROPERTYKEY PKEY_AudioEngine_DeviceFormat = new PROPERTYKEY(new Guid("f19f064d-082c-4e27-bc73-6882a1bb8e4c"), 0);
        static readonly PROPERTYKEY PKEY_AudioEndpoint_Disable_SysFx = new PROPERTYKEY(new Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), 5);

        [DllImport("ole32.dll")]
        static extern int PropVariantClear(ref PROPVARIANT pvar);

        [DllImport("ole32.dll")]
        static extern int CoCreateInstance(ref Guid clsid, IntPtr outer, uint ctx, ref Guid iid, out IntPtr ppv);

        public static string Probe(string nameContains)
        {
            return RunSta(delegate { return ProbeCore(nameContains ?? "USB Audio CODEC"); });
        }

        public static string Apply(string nameContains)
        {
            return RunSta(delegate { return ApplyCore(nameContains ?? "USB Audio CODEC"); });
        }

        static string RunSta(Func<string> fn)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                return fn();
            string result = null;
            Exception error = null;
            Thread t = new Thread(delegate()
            {
                try { result = fn(); }
                catch (Exception ex) { error = ex; }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (error != null) throw error;
            return result;
        }

        static IntPtr Slot(IntPtr obj, int n)
        {
            return Marshal.ReadIntPtr(Marshal.ReadIntPtr(obj), n * IntPtr.Size);
        }

        static T Fn<T>(IntPtr obj, int n) where T : class
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(Slot(obj, n), typeof(T));
        }

        static void Release(IntPtr p)
        {
            if (p == IntPtr.Zero) return;
            Fn<FnRelease>(p, 2)(p);
        }

        static IntPtr CreateEnumerator()
        {
            Guid clsid = CLSID_MMDeviceEnumerator;
            Guid iid = IID_IMMDeviceEnumerator;
            IntPtr p;
            Marshal.ThrowExceptionForHR(CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iid, out p));
            return p;
        }

        static string ProbeCore(string nameContains)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"devices\":[");
            List<Dev> devices = EnumCapture(nameContains);
            for (int i = 0; i < devices.Count; i++)
            {
                if (i > 0) sb.Append(',');
                AppendDeviceJson(sb, devices[i], false);
            }
            sb.Append("]}");
            return sb.ToString();
        }

        static string ApplyCore(string nameContains)
        {
            List<Dev> matches = EnumCapture(nameContains);
            if (matches.Count == 0)
                return "{\"ok\":false,\"error\":\"no capture endpoint matching USB Audio CODEC\"}";
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"devices\":[");
            for (int i = 0; i < matches.Count; i++)
            {
                if (i > 0) sb.Append(',');
                Dev d = matches[i];
                d.DisableSysFxError = DisableSysFx(d.Id);
                d.SetFormatError = Set48k16(d);
                AppendDeviceJson(sb, d, true);
            }
            sb.Append("]}");
            return sb.ToString();
        }

        static List<Dev> EnumCapture(string nameContains)
        {
            List<Dev> list = new List<Dev>();
            IntPtr enumerator = CreateEnumerator();
            try
            {
                IntPtr col;
                Marshal.ThrowExceptionForHR(Fn<FnEnum>(enumerator, 3)(enumerator, eCapture, DEVICE_STATE_ACTIVE, out col));
                try
                {
                    uint count;
                    Marshal.ThrowExceptionForHR(Fn<FnGetCount>(col, 3)(col, out count));
                    for (uint i = 0; i < count; i++)
                    {
                        IntPtr device;
                        Marshal.ThrowExceptionForHR(Fn<FnItem>(col, 4)(col, i, out device));
                        try
                        {
                            string id = ReadId(device);
                            string name = ReadFriendlyName(device) ?? "";
                            if (name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                            Dev d = new Dev();
                            d.Id = id;
                            d.Name = name;
                            FillWasapi(device, d);
                            FillStoredFormat(device, d);
                            d.SysFxDisabled = ReadSysFxDisabled(device);
                            list.Add(d);
                        }
                        finally { Release(device); }
                    }
                }
                finally { Release(col); }
            }
            finally { Release(enumerator); }
            return list;
        }

        static string ReadId(IntPtr device)
        {
            IntPtr p;
            Marshal.ThrowExceptionForHR(Fn<FnGetId>(device, 5)(device, out p));
            string s = Marshal.PtrToStringUni(p);
            Marshal.FreeCoTaskMem(p);
            return s;
        }

        static void FillWasapi(IntPtr device, Dev d)
        {
            Guid iid = IID_IAudioClient;
            IntPtr client;
            int hr = Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out client);
            if (hr != 0 || client == IntPtr.Zero)
            {
                d.WasapiError = Hr(hr);
                return;
            }
            try
            {
                IntPtr pMix;
                hr = Fn<FnMix>(client, 8)(client, out pMix);
                if (hr == 0 && pMix != IntPtr.Zero)
                {
                    d.Mix = ReadWave(pMix);
                    Marshal.FreeCoTaskMem(pMix);
                }
                else d.WasapiError = Hr(hr);

                long defPeriod, minPeriod;
                if (Fn<FnPeriod>(client, 9)(client, out defPeriod, out minPeriod) == 0)
                {
                    d.DefaultPeriodMs = defPeriod / 10000.0;
                    d.MinPeriodMs = minPeriod / 10000.0;
                }

                d.Exclusive48k16Stereo = FormatSupported(client, AUDCLNT_SHAREMODE_EXCLUSIVE, 2);
                d.Exclusive48k16Mono = FormatSupported(client, AUDCLNT_SHAREMODE_EXCLUSIVE, 1);
                d.Shared48k16Stereo = FormatSupported(client, AUDCLNT_SHAREMODE_SHARED, 2);
            }
            finally { Release(client); }
        }

        static bool FormatSupported(IntPtr client, int shareMode, ushort channels)
        {
            IntPtr p = AllocPcm48k16(channels);
            try
            {
                IntPtr closest;
                int hr = Fn<FnIsFmt>(client, 7)(client, shareMode, p, out closest);
                if (closest != IntPtr.Zero) Marshal.FreeCoTaskMem(closest);
                return hr == 0;
            }
            finally { Marshal.FreeHGlobal(p); }
        }

        static void FillStoredFormat(IntPtr device, Dev d)
        {
            IntPtr store;
            if (Fn<FnOpenStore>(device, 4)(device, STGM_READ, out store) != 0) return;
            try
            {
                PROPVARIANT pv;
                PROPERTYKEY key = PKEY_AudioEngine_DeviceFormat;
                if (Fn<FnGetValue>(store, 5)(store, ref key, out pv) == 0 && pv.vt == VT_BLOB && pv.blobData != IntPtr.Zero)
                    d.Stored = ReadWave(pv.blobData);
                PropVariantClear(ref pv);
            }
            finally { Release(store); }
        }

        static object ReadSysFxDisabled(IntPtr device)
        {
            IntPtr store;
            if (Fn<FnOpenStore>(device, 4)(device, STGM_READ, out store) != 0) return null;
            try
            {
                PROPVARIANT pv;
                PROPERTYKEY key = PKEY_AudioEndpoint_Disable_SysFx;
                int hr = Fn<FnGetValue>(store, 5)(store, ref key, out pv);
                if (hr != 0 || pv.vt == VT_EMPTY) { PropVariantClear(ref pv); return null; }
                bool off = false;
                if (pv.vt == VT_BOOL) off = pv.boolVal != 0;
                else if (pv.vt == VT_UI4) off = pv.ulVal != 0;
                PropVariantClear(ref pv);
                return off;
            }
            finally { Release(store); }
        }

        static string DisableSysFx(string deviceId)
        {
            IntPtr enumerator = CreateEnumerator();
            try
            {
                IntPtr idPtr = Marshal.StringToCoTaskMemUni(deviceId);
                IntPtr device;
                int hr;
                try { hr = Fn<FnGetDevice>(enumerator, 5)(enumerator, idPtr, out device); }
                finally { Marshal.FreeCoTaskMem(idPtr); }
                if (hr != 0) return Hr(hr);
                try
                {
                    IntPtr store;
                    hr = Fn<FnOpenStore>(device, 4)(device, STGM_READWRITE, out store);
                    if (hr != 0) return Hr(hr);
                    try
                    {
                        PROPVARIANT pv = new PROPVARIANT();
                        pv.vt = VT_UI4;
                        pv.ulVal = 1;
                        PROPERTYKEY key = PKEY_AudioEndpoint_Disable_SysFx;
                        hr = Fn<FnSetValue>(store, 6)(store, ref key, ref pv);
                        if (hr != 0) return Hr(hr);
                        hr = Fn<FnCommit>(store, 7)(store);
                        return hr == 0 ? null : Hr(hr);
                    }
                    finally { Release(store); }
                }
                finally { Release(device); }
            }
            finally { Release(enumerator); }
        }

        static string Set48k16(Dev d)
        {
            ushort ch = 2;
            if (d.Mix != null && d.Mix.Channels == 1) ch = 1;
            IntPtr fmt = AllocPcm48k16(ch);
            try
            {
                IPolicyConfig cfg = (IPolicyConfig)new PolicyConfigClient();
                int hr = cfg.SetDeviceFormat(d.Id, fmt, fmt);
                return hr == 0 ? null : Hr(hr);
            }
            catch (Exception ex)
            {
                return ex.GetType().Name + ": " + ex.Message;
            }
            finally { Marshal.FreeHGlobal(fmt); }
        }

        static IntPtr AllocPcm48k16(ushort channels)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WAVEFORMATEXTENSIBLE)));
            WAVEFORMATEXTENSIBLE w = new WAVEFORMATEXTENSIBLE();
            w.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
            w.nChannels = channels;
            w.nSamplesPerSec = 48000;
            w.wBitsPerSample = 16;
            w.nBlockAlign = (ushort)(channels * 2);
            w.nAvgBytesPerSec = 48000u * w.nBlockAlign;
            w.cbSize = 22;
            w.wValidBitsPerSample = 16;
            w.dwChannelMask = channels == 1 ? 4u : 3u;
            w.SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
            Marshal.StructureToPtr(w, p, false);
            return p;
        }

        static Wave ReadWave(IntPtr p)
        {
            Wave w = new Wave();
            w.Tag = (ushort)Marshal.ReadInt16(p, 0);
            w.Channels = (ushort)Marshal.ReadInt16(p, 2);
            w.Rate = (uint)Marshal.ReadInt32(p, 4);
            w.Bits = (ushort)Marshal.ReadInt16(p, 14);
            return w;
        }

        static string ReadFriendlyName(IntPtr device)
        {
            IntPtr store;
            if (Fn<FnOpenStore>(device, 4)(device, STGM_READ, out store) != 0) return null;
            try
            {
                PROPVARIANT pv;
                PROPERTYKEY key = PKEY_Device_FriendlyName;
                if (Fn<FnGetValue>(store, 5)(store, ref key, out pv) != 0) return null;
                string s = null;
                if (pv.vt == VT_LPWSTR && pv.ptr != IntPtr.Zero) s = Marshal.PtrToStringUni(pv.ptr);
                PropVariantClear(ref pv);
                return s;
            }
            finally { Release(store); }
        }

        static string Hr(int hr) { return "0x" + ((uint)hr).ToString("X8", CultureInfo.InvariantCulture); }

        static string JsonStr(string s)
        {
            if (s == null) return "null";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        static void AppendWave(StringBuilder sb, string key, Wave w)
        {
            sb.Append("\"").Append(key).Append("\":");
            if (w == null) { sb.Append("null"); return; }
            sb.Append("{\"rate\":").Append(w.Rate)
              .Append(",\"bits\":").Append(w.Bits)
              .Append(",\"channels\":").Append(w.Channels)
              .Append(",\"tag\":").Append(w.Tag).Append("}");
        }

        static void AppendBoolObj(StringBuilder sb, string key, object v)
        {
            sb.Append("\"").Append(key).Append("\":");
            if (v == null) sb.Append("null");
            else sb.Append(((bool)v) ? "true" : "false");
        }

        static void AppendDeviceJson(StringBuilder sb, Dev d, bool apply)
        {
            sb.Append("{\"id\":").Append(JsonStr(d.Id))
              .Append(",\"name\":").Append(JsonStr(d.Name)).Append(",");
            AppendWave(sb, "mix", d.Mix);
            sb.Append(",");
            AppendWave(sb, "stored", d.Stored);
            sb.Append(",\"defaultPeriodMs\":").Append(d.DefaultPeriodMs.ToString("0.###", CultureInfo.InvariantCulture))
              .Append(",\"minPeriodMs\":").Append(d.MinPeriodMs.ToString("0.###", CultureInfo.InvariantCulture))
              .Append(",\"exclusive48k16Stereo\":").Append(d.Exclusive48k16Stereo ? "true" : "false")
              .Append(",\"exclusive48k16Mono\":").Append(d.Exclusive48k16Mono ? "true" : "false")
              .Append(",\"shared48k16Stereo\":").Append(d.Shared48k16Stereo ? "true" : "false")
              .Append(",");
            AppendBoolObj(sb, "sysFxDisabled", d.SysFxDisabled);
            if (d.WasapiError != null)
                sb.Append(",\"wasapiError\":").Append(JsonStr(d.WasapiError));
            if (apply)
            {
                sb.Append(",\"disableSysFxError\":").Append(JsonStr(d.DisableSysFxError))
                  .Append(",\"setFormatError\":").Append(JsonStr(d.SetFormatError));
            }
            sb.Append("}");
        }

        class Dev
        {
            public string Id, Name, WasapiError, DisableSysFxError, SetFormatError;
            public Wave Mix, Stored;
            public double DefaultPeriodMs, MinPeriodMs;
            public bool Exclusive48k16Stereo, Exclusive48k16Mono, Shared48k16Stereo;
            public object SysFxDisabled;
        }

        class Wave
        {
            public uint Rate;
            public ushort Bits, Channels, Tag;
        }
    }
}
