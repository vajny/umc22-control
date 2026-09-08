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

    [ComImport, Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioCaptureClientCom
    {
        [PreserveSig] int GetBuffer(out IntPtr data, out uint frames, out uint flags, out ulong pos, out ulong qpc);
        [PreserveSig] int ReleaseBuffer(uint frames);
        [PreserveSig] int GetNextPacketSize(out uint frames);
    }

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

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetVol(IntPtr self, out float level);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnSetVol(IntPtr self, float level, IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetMute(IntPtr self, out int mute);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnSetMute(IntPtr self, int mute, IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetPeak(IntPtr self, out float peak);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnInit(IntPtr self, int share, uint flags, long buf, long period, IntPtr fmt, IntPtr guid);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnGetService(IntPtr self, IntPtr iid, IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnStart(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnStop(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnCapBuffer(IntPtr self, out IntPtr data, out uint frames, out uint flags, IntPtr pos, IntPtr qpc);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnCapRelease(IntPtr self, uint frames);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate int FnCapPacket(IntPtr self, out uint frames);

    public static class WasapiCapture
    {
        const int eRender = 0;
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
        static readonly Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        static readonly Guid IID_IAudioMeterInformation = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");
        static readonly Guid IID_IAudioCaptureClient = new Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317");
        static readonly Guid KSDATAFORMAT_SUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00AA00389B71");
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

        public static Mixer Snapshot(string nameContains)
        {
            return RunStaT(delegate { return SnapshotCore(nameContains ?? "USB Audio CODEC"); });
        }

        public static string SetVolume(string deviceId, float scalar)
        {
            if (scalar < 0) scalar = 0;
            if (scalar > 1) scalar = 1;
            float s = scalar;
            return RunSta(delegate
            {
                return WithVolume(deviceId, delegate(IntPtr vol)
                {
                    return HrOrNull(Fn<FnSetVol>(vol, 7)(vol, s, IntPtr.Zero));
                });
            });
        }

        public static string SetMute(string deviceId, bool mute)
        {
            return RunSta(delegate
            {
                return WithVolume(deviceId, delegate(IntPtr vol)
                {
                    return HrOrNull(Fn<FnSetMute>(vol, 14)(vol, mute ? 1 : 0, IntPtr.Zero));
                });
            });
        }

        public static float Peak(string deviceId)
        {
            return RunStaT(delegate
            {
                float peak = 0;
                WithDevice(deviceId, delegate(IntPtr device)
                {
                    Guid iid = IID_IAudioMeterInformation;
                    IntPtr meter;
                    if (Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out meter) != 0 || meter == IntPtr.Zero)
                        return;
                    try { Fn<FnGetPeak>(meter, 3)(meter, out peak); }
                    finally { Release(meter); }
                });
                return peak;
            });
        }

        public static string MeterProbe(string deviceId)
        {
            return RunSta(delegate
            {
                CaptureTap t = CaptureTap.Open(deviceId);
                try
                {
                    if (t.Error != null) return t.Debug;
                    float max = 0;
                    for (int i = 0; i < 30; i++)
                    {
                        t.Pump();
                        if (t.Peak > max) max = t.Peak;
                        Thread.Sleep(50);
                    }
                    return t.Debug + " max=" + max.ToString("0.0000", CultureInfo.InvariantCulture);
                }
                finally { t.Dispose(); }
            });
        }

        public static string ApplyFormat(string deviceId, int rate, int channels, bool disableApo)
        {
            return RunSta(delegate
            {
                string apo = SetSysFx(deviceId, disableApo);
                string fmt = SetPcm16(deviceId, (uint)rate, (ushort)channels);
                if (apo != null) return apo;
                return fmt;
            });
        }

        static string RunSta(Func<string> fn)
        {
            return RunStaT(fn);
        }

        static T RunStaT<T>(Func<T> fn)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                return fn();
            T result = default(T);
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

        static int GetServicePtr(IntPtr client, int slot, Guid iid, out IntPtr ppv)
        {
            IntPtr iidMem = Marshal.AllocHGlobal(16);
            IntPtr outMem = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                Marshal.StructureToPtr(iid, iidMem, false);
                Marshal.WriteIntPtr(outMem, IntPtr.Zero);
                int hr = Fn<FnGetService>(client, slot)(client, iidMem, outMem);
                ppv = Marshal.ReadIntPtr(outMem);
                return hr;
            }
            finally
            {
                Marshal.FreeHGlobal(iidMem);
                Marshal.FreeHGlobal(outMem);
            }
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

        static string HrOrNull(int hr)
        {
            return hr == 0 ? null : Hr(hr);
        }

        delegate void DeviceOp(IntPtr device);
        delegate string VolumeOp(IntPtr vol);

        static void WithDevice(string deviceId, DeviceOp body)
        {
            IntPtr enumerator = CreateEnumerator();
            try
            {
                IntPtr idPtr = Marshal.StringToCoTaskMemUni(deviceId);
                IntPtr device;
                int hr;
                try { hr = Fn<FnGetDevice>(enumerator, 5)(enumerator, idPtr, out device); }
                finally { Marshal.FreeCoTaskMem(idPtr); }
                Marshal.ThrowExceptionForHR(hr);
                try { body(device); }
                finally { Release(device); }
            }
            finally { Release(enumerator); }
        }

        static string WithVolume(string deviceId, VolumeOp op)
        {
            string result = "no device";
            WithDevice(deviceId, delegate(IntPtr device)
            {
                Guid iid = IID_IAudioEndpointVolume;
                IntPtr vol;
                int hr = Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out vol);
                if (hr != 0 || vol == IntPtr.Zero) { result = Hr(hr); return; }
                try { result = op(vol); }
                finally { Release(vol); }
            });
            return result;
        }

        static Mixer SnapshotCore(string nameContains)
        {
            List<Dev> list = EnumCapture(nameContains);
            Mixer m = new Mixer();
            if (list.Count == 0)
            {
                m.Present = false;
                m.Error = "UMC22 (USB Audio CODEC) není v capture zařízeních.";
                return m;
            }
            Dev d = list[0];
            m.Present = true;
            m.Id = d.Id;
            m.Name = d.Name;
            m.Volume = d.Volume;
            m.Mute = d.Mute;
            m.Peak = d.Peak;
            m.EnhancementsOff = d.SysFxDisabled is bool && (bool)d.SysFxDisabled;
            m.Exclusive48k16Stereo = d.Exclusive48k16Stereo;
            m.Exclusive48k16Mono = d.Exclusive48k16Mono;
            if (d.Stored != null)
            {
                m.StoredRate = (int)d.Stored.Rate;
                m.StoredBits = d.Stored.Bits;
                m.StoredChannels = d.Stored.Channels;
            }
            if (d.Mix != null)
            {
                m.MixRate = (int)d.Mix.Rate;
                m.MixBits = d.Mix.Bits;
                m.MixChannels = d.Mix.Channels;
            }
            List<Dev> play = EnumEndpoints(eRender, nameContains, false);
            if (play.Count > 0)
            {
                m.PlayPresent = true;
                m.PlayId = play[0].Id;
                m.PlayName = play[0].Name;
                m.PlayVolume = play[0].Volume;
                m.PlayMute = play[0].Mute;
            }
            return m;
        }

        static void FillMixer(IntPtr device, Dev d)
        {
            Guid iid = IID_IAudioEndpointVolume;
            IntPtr vol;
            if (Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out vol) == 0 && vol != IntPtr.Zero)
            {
                try
                {
                    float level;
                    int mute;
                    if (Fn<FnGetVol>(vol, 9)(vol, out level) == 0) d.Volume = level;
                    if (Fn<FnGetMute>(vol, 15)(vol, out mute) == 0) d.Mute = mute != 0;
                }
                finally { Release(vol); }
            }
            iid = IID_IAudioMeterInformation;
            IntPtr meter;
            if (Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out meter) == 0 && meter != IntPtr.Zero)
            {
                try
                {
                    float peak;
                    if (Fn<FnGetPeak>(meter, 3)(meter, out peak) == 0) d.Peak = peak;
                }
                finally { Release(meter); }
            }
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
            return EnumEndpoints(eCapture, nameContains, true);
        }

        static List<Dev> EnumEndpoints(int flow, string nameContains, bool full)
        {
            List<Dev> list = new List<Dev>();
            IntPtr enumerator = CreateEnumerator();
            try
            {
                IntPtr col;
                Marshal.ThrowExceptionForHR(Fn<FnEnum>(enumerator, 3)(enumerator, flow, DEVICE_STATE_ACTIVE, out col));
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
                            if (full)
                            {
                                FillWasapi(device, d);
                                FillStoredFormat(device, d);
                                d.SysFxDisabled = ReadSysFxDisabled(device);
                            }
                            FillMixer(device, d);
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
            IntPtr p = AllocPcm16(48000, channels);
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
            return SetSysFx(deviceId, true);
        }

        static string SetSysFx(string deviceId, bool disabled)
        {
            string err = null;
            WithDevice(deviceId, delegate(IntPtr device)
            {
                IntPtr store;
                int hr = Fn<FnOpenStore>(device, 4)(device, STGM_READWRITE, out store);
                if (hr != 0) { err = Hr(hr); return; }
                try
                {
                    PROPVARIANT pv = new PROPVARIANT();
                    pv.vt = VT_UI4;
                    pv.ulVal = disabled ? 1u : 0u;
                    PROPERTYKEY key = PKEY_AudioEndpoint_Disable_SysFx;
                    hr = Fn<FnSetValue>(store, 6)(store, ref key, ref pv);
                    if (hr != 0) { err = Hr(hr); return; }
                    hr = Fn<FnCommit>(store, 7)(store);
                    err = HrOrNull(hr);
                }
                finally { Release(store); }
            });
            return err;
        }

        static string Set48k16(Dev d)
        {
            ushort ch = 2;
            if (d.Mix != null && d.Mix.Channels == 1) ch = 1;
            return SetPcm16(d.Id, 48000, ch);
        }

        static string SetPcm16(string deviceId, uint rate, ushort channels)
        {
            IntPtr fmt = AllocPcm16(rate, channels);
            try
            {
                IPolicyConfig cfg = (IPolicyConfig)new PolicyConfigClient();
                int hr = cfg.SetDeviceFormat(deviceId, fmt, fmt);
                return HrOrNull(hr);
            }
            catch (Exception ex)
            {
                return ex.GetType().Name + ": " + ex.Message;
            }
            finally { Marshal.FreeHGlobal(fmt); }
        }

        static IntPtr AllocPcm16(uint rate, ushort channels)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WAVEFORMATEXTENSIBLE)));
            WAVEFORMATEXTENSIBLE w = new WAVEFORMATEXTENSIBLE();
            w.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
            w.nChannels = channels;
            w.nSamplesPerSec = rate;
            w.wBitsPerSample = 16;
            w.nBlockAlign = (ushort)(channels * 2);
            w.nAvgBytesPerSec = rate * w.nBlockAlign;
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
            public float Volume, Peak;
            public bool Mute;
        }

        public class Mixer
        {
            public bool Present;
            public string Id, Name, Error;
            public int StoredRate, StoredBits, StoredChannels;
            public int MixRate, MixBits, MixChannels;
            public bool EnhancementsOff;
            public float Volume, Peak;
            public bool Mute;
            public bool Exclusive48k16Stereo, Exclusive48k16Mono;
            public bool PlayPresent;
            public string PlayId, PlayName;
            public float PlayVolume;
            public bool PlayMute;
        }

        public class CaptureTap : IDisposable
        {
            IntPtr _client;
            IAudioCaptureClientCom _cap;
            int _blockAlign;
            int _channels;
            int _bytesPerSample;
            bool _ieeeFloat;
            bool _started;
            float _peak;
            string _error;

            public float Peak { get { return _peak; } }
            public string Error { get { return _error; } }
            public bool Started { get { return _started; } }
            public string Debug
            {
                get
                {
                    return "started=" + _started
                        + " cap=" + (_cap != null)
                        + " client=" + _client.ToInt64().ToString("X")
                        + " float=" + _ieeeFloat
                        + " ch=" + _channels
                        + " align=" + _blockAlign
                        + " bps=" + _bytesPerSample
                        + " err=" + _error;
                }
            }

            public static CaptureTap Open(string deviceId)
            {
                CaptureTap t = new CaptureTap();
                t.Connect(deviceId);
                return t;
            }

            public void Pump()
            {
                if (_cap == null) return;
                float packetPeak = 0;
                for (;;)
                {
                    uint next;
                    int hr = _cap.GetNextPacketSize(out next);
                    if (hr != 0 || next == 0) break;
                    IntPtr data;
                    uint frames, flags;
                    ulong pos, qpc;
                    hr = _cap.GetBuffer(out data, out frames, out flags, out pos, out qpc);
                    if (hr != 0) break;
                    if ((flags & 1) == 0 && data != IntPtr.Zero && frames > 0)
                    {
                        float p = PeakOf(data, frames);
                        if (p > packetPeak) packetPeak = p;
                    }
                    _cap.ReleaseBuffer(frames);
                }
                if (packetPeak > _peak) _peak = packetPeak;
                else _peak *= 0.78f;
                if (_peak < 0.0005f) _peak = 0;
            }

            public void Dispose()
            {
                if (_started && _client != IntPtr.Zero)
                {
                    Fn<FnStop>(_client, 11)(_client);
                    _started = false;
                }
                if (_cap != null)
                {
                    Marshal.ReleaseComObject(_cap);
                    _cap = null;
                }
                if (_client != IntPtr.Zero) { Release(_client); _client = IntPtr.Zero; }
            }

            void Connect(string deviceId)
            {
                IntPtr enumerator = CreateEnumerator();
                try
                {
                    IntPtr idPtr = Marshal.StringToCoTaskMemUni(deviceId);
                    IntPtr device;
                    int hr;
                    try { hr = Fn<FnGetDevice>(enumerator, 5)(enumerator, idPtr, out device); }
                    finally { Marshal.FreeCoTaskMem(idPtr); }
                    if (hr != 0) { _error = CaptureError(hr); return; }
                    try
                    {
                        Guid iid = IID_IAudioClient;
                        hr = Fn<FnActivate>(device, 3)(device, ref iid, 23, IntPtr.Zero, out _client);
                        if (hr != 0 || _client == IntPtr.Zero) { _error = CaptureError(hr); return; }
                    }
                    finally { Release(device); }
                }
                finally { Release(enumerator); }

                IntPtr mix;
                int mixHr = Fn<FnMix>(_client, 8)(_client, out mix);
                if (mixHr != 0 || mix == IntPtr.Zero) { _error = CaptureError(mixHr); return; }
                try
                {
                    ReadMixLayout(mix);
                    mixHr = Fn<FnInit>(_client, 3)(_client, AUDCLNT_SHAREMODE_SHARED, 0, 2000000L, 0L, mix, IntPtr.Zero);
                }
                finally { Marshal.FreeCoTaskMem(mix); }
                if (mixHr != 0) { _error = CaptureError(mixHr); return; }

                IntPtr capPtr;
                int svcHr = GetServicePtr(_client, 14, IID_IAudioCaptureClient, out capPtr);
                if (svcHr != 0 || capPtr == IntPtr.Zero)
                {
                    _error = "GetService " + CaptureError(svcHr);
                    if (capPtr != IntPtr.Zero) Release(capPtr);
                    return;
                }

                try
                {
                    _cap = (IAudioCaptureClientCom)Marshal.GetTypedObjectForIUnknown(capPtr, typeof(IAudioCaptureClientCom));
                }
                catch (Exception ex)
                {
                    Release(capPtr);
                    _error = ex.GetType().Name + ": " + ex.Message;
                    return;
                }
                Release(capPtr);
                if (_cap == null) { _error = "RCW IAudioCaptureClient je null"; return; }

                mixHr = Fn<FnStart>(_client, 10)(_client);
                if (mixHr != 0) { _error = "Start " + CaptureError(mixHr); return; }
                _started = true;
            }

            void ReadMixLayout(IntPtr mix)
            {
                ushort tag = (ushort)Marshal.ReadInt16(mix, 0);
                _channels = Marshal.ReadInt16(mix, 2);
                ushort bits = (ushort)Marshal.ReadInt16(mix, 14);
                _blockAlign = Marshal.ReadInt16(mix, 12);
                _ieeeFloat = tag == 3;
                if (tag == WAVE_FORMAT_EXTENSIBLE)
                {
                    Guid sub = (Guid)Marshal.PtrToStructure(new IntPtr(mix.ToInt64() + 24), typeof(Guid));
                    _ieeeFloat = sub.Equals(KSDATAFORMAT_SUBTYPE_IEEE_FLOAT);
                }
                _bytesPerSample = _ieeeFloat ? 4 : (bits / 8);
                if (_blockAlign <= 0) _blockAlign = Math.Max(1, _channels * _bytesPerSample);
            }

            float PeakOf(IntPtr data, uint frames)
            {
                float peak = 0;
                int step = _bytesPerSample;
                int samples = (int)frames * _channels;
                for (int i = 0; i < samples; i++)
                {
                    IntPtr p = new IntPtr(data.ToInt64() + i * step);
                    float s;
                    if (_ieeeFloat) s = ReadFloat(p);
                    else if (_bytesPerSample >= 3) s = Marshal.ReadInt16(p) / 32768f;
                    else s = Marshal.ReadInt16(p) / 32768f;
                    if (s < 0) s = -s;
                    if (s > peak) peak = s;
                }
                if (peak > 1f) peak = 1f;
                return peak;
            }

            static float ReadFloat(IntPtr p)
            {
                byte[] b = new byte[4];
                Marshal.Copy(p, b, 0, 4);
                return BitConverter.ToSingle(b, 0);
            }

            static string CaptureError(int hr)
            {
                uint u = (uint)hr;
                if (u == 0x8889000A) return "vstup obsazený (exclusive / Sonar?)";
                if (u == 0x88890008) return "zařízení odpojeno";
                return "0x" + u.ToString("X8", CultureInfo.InvariantCulture);
            }
        }

        class Wave
        {
            public uint Rate;
            public ushort Bits, Channels, Tag;
        }
    }
}
