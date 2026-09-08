using System;
using System.Drawing;
using System.Windows.Forms;

namespace Umc22;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

sealed class MainForm : Form
{
    const string Match = "USB Audio CODEC";

    readonly Label _device = new();
    readonly Label _format = new();
    readonly TrackBar _volume = new();
    readonly Label _volumeVal = new();
    readonly CheckBox _mute = new();
    readonly TrackBar _playVol = new();
    readonly Label _playVolVal = new();
    readonly CheckBox _playMute = new();
    readonly Label _peakLbl = new();
    readonly ProgressBar _peak = new();
    readonly ComboBox _rate = new();
    readonly ComboBox _channels = new();
    readonly CheckBox _apo = new();
    readonly Button _apply = new();
    readonly Button _refresh = new();
    readonly Label _status = new();
    readonly System.Windows.Forms.Timer _timer = new();

    string _id;
    string _playId;
    bool _ui;
    WasapiCapture.CaptureTap _tap;

    public MainForm()
    {
        Text = "UMC22";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(420, 452);
        Font = new Font("Segoe UI", 9.75f);

        _device.AutoSize = false;
        _device.Location = new Point(16, 16);
        _device.Size = new Size(388, 40);
        _device.Text = "Hledám UMC22…";

        _format.AutoSize = false;
        _format.Location = new Point(16, 56);
        _format.Size = new Size(388, 22);
        _format.ForeColor = SystemColors.GrayText;

        var volLbl = new Label { Text = "Mikrofon", Location = new Point(16, 84), AutoSize = true };
        _volume.Location = new Point(12, 102);
        _volume.Size = new Size(320, 45);
        _volume.Minimum = 0;
        _volume.Maximum = 100;
        _volume.TickFrequency = 10;
        _volume.Scroll += (_, _) =>
        {
            if (_ui || _id == null) return;
            _volumeVal.Text = _volume.Value + " %";
            var err = WasapiCapture.SetVolume(_id, _volume.Value / 100f);
            if (err != null) _status.Text = err;
        };
        _volumeVal.Location = new Point(340, 108);
        _volumeVal.AutoSize = true;
        _volumeVal.Text = "—";

        var playLbl = new Label { Text = "Výstup", Location = new Point(16, 148), AutoSize = true };
        _playVol.Location = new Point(12, 166);
        _playVol.Size = new Size(320, 45);
        _playVol.Minimum = 0;
        _playVol.Maximum = 100;
        _playVol.TickFrequency = 10;
        _playVol.Scroll += (_, _) =>
        {
            if (_ui || _playId == null) return;
            _playVolVal.Text = _playVol.Value + " %";
            var err = WasapiCapture.SetVolume(_playId, _playVol.Value / 100f);
            if (err != null) _status.Text = err;
        };
        _playVolVal.Location = new Point(340, 172);
        _playVolVal.AutoSize = true;
        _playVolVal.Text = "—";

        _mute.Text = "Ztlumit mikrofon";
        _mute.Location = new Point(16, 210);
        _mute.AutoSize = true;
        _mute.CheckedChanged += (_, _) =>
        {
            if (_ui || _id == null) return;
            var err = WasapiCapture.SetMute(_id, _mute.Checked);
            if (err != null) _status.Text = err;
        };

        _playMute.Text = "Ztlumit výstup";
        _playMute.Location = new Point(200, 210);
        _playMute.AutoSize = true;
        _playMute.CheckedChanged += (_, _) =>
        {
            if (_ui || _playId == null) return;
            var err = WasapiCapture.SetMute(_playId, _playMute.Checked);
            if (err != null) _status.Text = err;
        };

        _peakLbl.Text = "Vstup";
        _peakLbl.Location = new Point(16, 242);
        _peakLbl.AutoSize = true;
        _peak.Location = new Point(16, 264);
        _peak.Size = new Size(388, 18);
        _peak.Maximum = 1000;

        var rateLbl = new Label { Text = "Sample rate", Location = new Point(16, 296), AutoSize = true };
        _rate.DropDownStyle = ComboBoxStyle.DropDownList;
        _rate.Location = new Point(16, 316);
        _rate.Width = 140;
        _rate.Items.AddRange(new object[] { "44100", "48000" });

        var chLbl = new Label { Text = "Kanály", Location = new Point(172, 296), AutoSize = true };
        _channels.DropDownStyle = ComboBoxStyle.DropDownList;
        _channels.Location = new Point(172, 316);
        _channels.Width = 80;
        _channels.Items.AddRange(new object[] { "1", "2" });

        _apo.Text = "Vypnout Windows vylepšení";
        _apo.Location = new Point(16, 352);
        _apo.AutoSize = true;
        _apo.Checked = true;

        _apply.Text = "Použít formát";
        _apply.Location = new Point(16, 384);
        _apply.Size = new Size(140, 32);
        _apply.Click += (_, _) => ApplyFormat();

        _refresh.Text = "Obnovit";
        _refresh.Location = new Point(164, 384);
        _refresh.Size = new Size(100, 32);
        _refresh.Click += (_, _) => LoadSnapshot();

        _status.AutoSize = false;
        _status.Location = new Point(272, 388);
        _status.Size = new Size(132, 28);
        _status.ForeColor = SystemColors.GrayText;
        _status.Text = "";

        Controls.AddRange(new Control[]
        {
            _device, _format, volLbl, _volume, _volumeVal, playLbl, _playVol, _playVolVal,
            _mute, _playMute, _peakLbl, _peak, rateLbl, _rate, chLbl, _channels, _apo,
            _apply, _refresh, _status
        });

        _timer.Interval = 50;
        _timer.Tick += (_, _) => TickPeak();
        Load += (_, _) =>
        {
            LoadSnapshot();
            _timer.Start();
        };
        FormClosed += (_, _) =>
        {
            _timer.Stop();
            CloseTap();
        };
    }

    void LoadSnapshot()
    {
        try
        {
            var s = WasapiCapture.Snapshot(Match);
            _ui = true;
            if (!s.Present)
            {
                _id = null;
                _playId = null;
                CloseTap();
                SetPlayEnabled(false);
                _device.Text = s.Error ?? "UMC22 není připojená.";
                _format.Text = "";
                _status.Text = "offline";
                _peakLbl.Text = "Vstup";
                _peak.Value = 0;
                return;
            }
            _id = s.Id;
            OpenTap();
            _device.Text = s.Name + "\nWindows usbaudio · 16-bit PCM2902";
            _format.Text = s.StoredRate > 0
                ? $"Uložený formát: {s.StoredRate} Hz / {s.StoredBits}-bit / {s.StoredChannels} ch"
                : "Uložený formát: neznámý";
            _volume.Value = Math.Clamp((int)Math.Round(s.Volume * 100), 0, 100);
            _volumeVal.Text = _volume.Value + " %";
            _mute.Checked = s.Mute;
            if (s.PlayPresent)
            {
                _playId = s.PlayId;
                SetPlayEnabled(true);
                _playVol.Value = Math.Clamp((int)Math.Round(s.PlayVolume * 100), 0, 100);
                _playVolVal.Text = _playVol.Value + " %";
                _playMute.Checked = s.PlayMute;
            }
            else
            {
                _playId = null;
                SetPlayEnabled(false);
                _playVol.Value = 0;
                _playVolVal.Text = "—";
                _playMute.Checked = false;
            }
            if (s.StoredRate == 44100 || s.StoredRate == 48000)
                _rate.SelectedItem = s.StoredRate.ToString();
            else
                _rate.SelectedItem = "48000";
            _channels.SelectedItem = s.StoredChannels == 1 ? "1" : "2";
            _apo.Checked = s.EnhancementsOff || s.StoredRate == 0;
            _status.Text = "ok";
        }
        catch (Exception ex)
        {
            _id = null;
            _playId = null;
            CloseTap();
            SetPlayEnabled(false);
            _device.Text = ex.Message;
            _status.Text = "chyba";
        }
        finally { _ui = false; }
    }

    void SetPlayEnabled(bool on)
    {
        _playVol.Enabled = on;
        _playMute.Enabled = on;
        if (!on) _playVolVal.Text = "—";
    }

    void ApplyFormat()
    {
        if (_id == null)
        {
            LoadSnapshot();
            if (_id == null) { _status.Text = "žádná karta"; return; }
        }
        int rate = int.Parse((string)_rate.SelectedItem);
        int ch = int.Parse((string)_channels.SelectedItem);
        var err = WasapiCapture.ApplyFormat(_id, rate, ch, _apo.Checked);
        _status.Text = err == null ? "uloženo" : err;
        LoadSnapshot();
    }

    void CloseTap()
    {
        if (_tap == null) return;
        _tap.Dispose();
        _tap = null;
    }

    void OpenTap()
    {
        CloseTap();
        if (_id == null) return;
        _tap = WasapiCapture.CaptureTap.Open(_id);
        if (_tap.Error != null)
            _peakLbl.Text = "Vstup — " + _tap.Error;
        else
            _peakLbl.Text = "Vstup";
    }

    void TickPeak()
    {
        if (_tap == null || _tap.Error != null)
        {
            _peak.Value = 0;
            return;
        }
        try
        {
            _tap.Pump();
            int pct = Math.Clamp((int)Math.Round(_tap.Peak * 100), 0, 100);
            _peak.Value = pct * 10;
            _peakLbl.Text = "Vstup  " + pct + " %";
        }
        catch (Exception ex)
        {
            _peak.Value = 0;
            _peakLbl.Text = "Vstup — " + ex.Message;
        }
    }
}
