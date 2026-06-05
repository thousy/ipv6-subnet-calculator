using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Windows.Forms;

internal static class PortableProgram
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new IPv6CalculatorForm());
    }
}

internal sealed class IPv6CalculatorForm : Form
{
    private readonly TextBox hostInput = new TextBox();
    private readonly TrackBar prefixTrack = new TrackBar();
    private readonly NumericUpDown prefixNumber = new NumericUpDown();
    private readonly TextBox[] hextets = new TextBox[8];
    private readonly Label maskBits = BlueLabel();
    private readonly Label hostCompact = BlueLabel();
    private readonly Label networkCompact = BlueLabel();
    private readonly Label rangeFullStart = BlueLabel();
    private readonly Label rangeFullEnd = BlueLabel();
    private readonly Label rangeShortStart = BlueLabel();
    private readonly Label rangeShortEnd = BlueLabel();
    private readonly Label hostBinary = RedLabel();
    private readonly Label maskBinary = RedLabel();
    private readonly Label networkBinary = RedLabel();
    private readonly Label networkFull = BlueLabel();
    private readonly Label rangeStart = BlueLabel();
    private readonly Label rangeEnd = BlueLabel();
    private readonly Label totalCount = BlueLabel();
    private readonly Label nextNetworkFull = BlueLabel();
    private readonly Label nextRangeStart = BlueLabel();
    private readonly Label nextRangeEnd = BlueLabel();
    private readonly Label nextTotalCount = BlueLabel();
    private readonly TextBox expandedInput = new TextBox();
    private readonly TextBox compressedInput = new TextBox();
    private readonly TextBox compressedBottom = new TextBox();
    private readonly TextBox expandedBottom = new TextBox();
    private readonly Label errorLabel = new Label();
    private bool updating;

    public IPv6CalculatorForm()
    {
        Text = "IPv6 子网计算器";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1080, 800);
        Size = new Size(1220, 860);
        Font = new Font("Microsoft YaHei UI", 9F);
        BackColor = Color.White;

        BuildUi();
        WireEvents();

        hostInput.Text = "2001:4860:4860::8888";
        prefixNumber.Value = 112;
        prefixTrack.Value = 112;
        Calculate();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(18),
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
        Controls.Add(root);

        root.Controls.Add(new Label
        {
            Text = "IPv6 子网计算工具  V1.2",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(Font.FontFamily, 13F, FontStyle.Bold)
        }, 0, 0);

        root.Controls.Add(new Label
        {
            Text = "主机IPv6子网计算工具",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Accent(),
            Font = new Font(Font.FontFamily, 10F, FontStyle.Bold)
        }, 0, 1);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 520));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(main, 0, 2);

        main.Controls.Add(BuildInputPanel(), 0, 0);
        main.Controls.Add(BuildResultPanel(), 1, 0);
        root.Controls.Add(BuildConvertPanel(), 0, 3);
    }

    private Control BuildInputPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 13, Padding = new Padding(0, 0, 18, 0) };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(GreenLabel("请输入主机IPv6"), 0, 0);
        panel.Controls.Add(InputWithCopy(hostInput), 0, 1);
        panel.Controls.Add(GreenLabel("请选择子网掩码位数："), 0, 2);

        var prefixPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        prefixPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        prefixPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        prefixTrack.Minimum = 0;
        prefixTrack.Maximum = 128;
        prefixTrack.TickFrequency = 8;
        prefixTrack.Dock = DockStyle.Fill;
        prefixNumber.Minimum = 0;
        prefixNumber.Maximum = 128;
        prefixNumber.Dock = DockStyle.Fill;
        prefixPanel.Controls.Add(prefixTrack, 0, 0);
        prefixPanel.Controls.Add(prefixNumber, 1, 0);
        panel.Controls.Add(prefixPanel, 0, 3);

        panel.Controls.Add(GreenLabel("IPv6地址范围计算"), 0, 4);
        var hexPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 8 };
        for (int i = 0; i < 8; i++)
        {
            hexPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            hextets[i] = new TextBox { Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center, MaxLength = 4 };
            hexPanel.Controls.Add(hextets[i], i, 0);
        }
        panel.Controls.Add(hexPanel, 0, 5);

        panel.Controls.Add(SummaryRow("掩码位数：", maskBits, null, " 位"), 0, 6);
        panel.Controls.Add(SummaryRow("主机地址：", hostCompact, hostCompact), 0, 7);
        panel.Controls.Add(SummaryRow("网络前缀：", networkCompact, networkCompact), 0, 8);
        panel.Controls.Add(RangeSummary("可用范围（完整地址）：", rangeFullStart, rangeFullEnd), 0, 9);
        panel.Controls.Add(RangeSummary("可用范围（压缩地址）：", rangeShortStart, rangeShortEnd), 0, 10);
        errorLabel.ForeColor = Color.Firebrick;
        errorLabel.Dock = DockStyle.Fill;
        panel.Controls.Add(errorLabel, 0, 11);

        return panel;
    }

    private Control BuildResultPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0)
        };

        panel.Controls.Add(new Label { Text = "结果", Width = 610, Height = 30, Font = new Font(Font.FontFamily, 10F, FontStyle.Bold) });
        panel.Controls.Add(Box(new Control[]
        {
            ResultRow("主机地址：", hostBinary, null, 610),
            ResultRow("网络掩码：", maskBinary, null, 610),
            ResultRow("子网地址：", networkBinary, null, 610)
        }, 610));

        panel.Controls.Add(Box(new Control[]
        {
            ResultRow("当前子网地址：", networkFull, networkFull, 610),
            Hint("当前子网地址起始到结束", 610),
            ResultRow("范围起始：", rangeStart, rangeStart, 610),
            ResultRow("范围结束：", rangeEnd, rangeEnd, 610),
            ResultRow("地址数量：", totalCount, null, 610)
        }, 610));

        panel.Controls.Add(Box(new Control[]
        {
            ResultRow("下个子网地址：", nextNetworkFull, nextNetworkFull, 610),
            Hint("下个子网当前子网地址起始到结束", 610),
            ResultRow("下个子网起始：", nextRangeStart, nextRangeStart, 610),
            ResultRow("下个子网结束：", nextRangeEnd, nextRangeEnd, 610),
            ResultRow("地址数量：", nextTotalCount, null, 610)
        }, 610));

        return panel;
    }

    private Control BuildConvertPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(0, 12, 0, 14) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        panel.Controls.Add(LabeledInput("IPv6扩展地址", expandedInput), 0, 0);
        panel.Controls.Add(Button("压缩IPv6 >", CompressClick), 1, 0);
        panel.Controls.Add(LabeledInput("IPv6压缩地址", compressedInput), 2, 0);
        panel.Controls.Add(LabeledInput("IPv6压缩地址", compressedBottom), 0, 1);
        panel.Controls.Add(Button("扩展IPv6 >", ExpandClick), 1, 1);
        panel.Controls.Add(LabeledInput("IPv6扩展地址", expandedBottom), 2, 1);

        return panel;
    }

    private void WireEvents()
    {
        hostInput.TextChanged += delegate { if (!updating) Calculate(); };
        prefixTrack.ValueChanged += delegate
        {
            if (updating) return;
            prefixNumber.Value = prefixTrack.Value;
            Calculate();
        };
        prefixNumber.ValueChanged += delegate
        {
            if (updating) return;
            prefixTrack.Value = (int)prefixNumber.Value;
            Calculate();
        };

        foreach (TextBox box in hextets)
        {
            box.TextChanged += delegate { if (!updating) UpdateHostFromHextets(); };
        }

        compressedInput.TextChanged += delegate { if (!updating) compressedBottom.Text = compressedInput.Text.ToLower(); };
        compressedBottom.TextChanged += delegate { if (!updating) compressedInput.Text = compressedBottom.Text.ToLower(); };
        expandedInput.TextChanged += delegate { if (!updating) expandedBottom.Text = expandedInput.Text.ToLower(); };
        expandedBottom.TextChanged += delegate { if (!updating) expandedInput.Text = expandedBottom.Text.ToLower(); };
    }

    private void Calculate()
    {
        try
        {
            updating = true;
            int prefix = (int)prefixNumber.Value;
            ushort[] hostParts = ParseIPv6(hostInput.Text);
            BigInteger hostValue = PartsToBig(hostParts);
            BigInteger maskValue = PrefixMask(prefix);
            BigInteger networkValue = hostValue & maskValue;
            BigInteger endValue = networkValue | (Max128() ^ maskValue);
            BigInteger? nextStartValue = endValue == Max128() ? (BigInteger?)null : endValue + BigInteger.One;
            BigInteger? nextEndValue = nextStartValue.HasValue ? nextStartValue.Value | (Max128() ^ maskValue) : (BigInteger?)null;

            ushort[] maskParts = BigToParts(maskValue);
            ushort[] networkParts = BigToParts(networkValue);
            ushort[] endParts = BigToParts(endValue);
            ushort[] nextStartParts = nextStartValue.HasValue ? BigToParts(nextStartValue.Value) : null;
            ushort[] nextEndParts = nextEndValue.HasValue ? BigToParts(nextEndValue.Value) : null;

            for (int i = 0; i < 8; i++) hextets[i].Text = hostParts[i].ToString("x4");

            maskBits.Text = prefix.ToString(CultureInfo.InvariantCulture);
            hostCompact.Text = Compress(hostParts);
            networkCompact.Text = Compress(networkParts);
            rangeFullStart.Text = Expanded(networkParts);
            rangeFullEnd.Text = Expanded(endParts);
            rangeShortStart.Text = Compress(networkParts);
            rangeShortEnd.Text = Compress(endParts);
            hostBinary.Text = Binary(hostParts);
            maskBinary.Text = Binary(maskParts);
            networkBinary.Text = Binary(networkParts);
            networkFull.Text = Expanded(networkParts) + "/" + prefix;
            rangeStart.Text = Expanded(networkParts);
            rangeEnd.Text = Expanded(endParts);
            totalCount.Text = CountText(prefix);

            if (nextStartParts == null || nextEndParts == null)
            {
                nextNetworkFull.Text = "无下个子网";
                nextRangeStart.Text = "无下个子网";
                nextRangeEnd.Text = "无下个子网";
                nextTotalCount.Text = "无下个子网";
            }
            else
            {
                nextNetworkFull.Text = Expanded(nextStartParts) + "/" + prefix;
                nextRangeStart.Text = Expanded(nextStartParts);
                nextRangeEnd.Text = Expanded(nextEndParts);
                nextTotalCount.Text = CountText(prefix);
            }

            expandedInput.Text = Expanded(hostParts);
            compressedInput.Text = Compress(hostParts);
            compressedBottom.Text = compressedInput.Text;
            expandedBottom.Text = expandedInput.Text;
            errorLabel.Text = "";
        }
        catch (Exception ex)
        {
            errorLabel.Text = ex.Message;
        }
        finally
        {
            updating = false;
        }
    }

    private void UpdateHostFromHextets()
    {
        try
        {
            string[] parts = hextets.Select(t => string.IsNullOrWhiteSpace(t.Text) ? "0" : t.Text.Trim()).ToArray();
            if (parts.Any(p => p.Length > 4 || !p.All(Uri.IsHexDigit)))
            {
                errorLabel.Text = "IPv6地址段只能输入0-ffff";
                return;
            }

            updating = true;
            hostInput.Text = string.Join(":", parts.Select(p => p.PadLeft(4, '0')));
            updating = false;
            Calculate();
        }
        finally
        {
            updating = false;
        }
    }

    private void CompressClick(object sender, EventArgs e)
    {
        try
        {
            compressedInput.Text = Compress(ParseIPv6(expandedInput.Text));
            compressedBottom.Text = compressedInput.Text;
            errorLabel.Text = "";
        }
        catch (Exception ex)
        {
            errorLabel.Text = ex.Message;
        }
    }

    private void ExpandClick(object sender, EventArgs e)
    {
        try
        {
            expandedBottom.Text = Expanded(ParseIPv6(compressedBottom.Text));
            expandedInput.Text = expandedBottom.Text;
            errorLabel.Text = "";
        }
        catch (Exception ex)
        {
            errorLabel.Text = ex.Message;
        }
    }

    private static ushort[] ParseIPv6(string value)
    {
        IPAddress address;
        if (!IPAddress.TryParse(value, out address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            throw new FormatException("请输入正确的IPv6地址");
        }

        byte[] bytes = address.GetAddressBytes();
        ushort[] parts = new ushort[8];
        for (int i = 0; i < 8; i++)
        {
            parts[i] = (ushort)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);
        }
        return parts;
    }

    private static BigInteger PartsToBig(ushort[] parts)
    {
        byte[] bytes = new byte[17];
        for (int i = 0; i < 8; i++)
        {
            bytes[15 - i * 2] = (byte)(parts[i] >> 8);
            bytes[14 - i * 2] = (byte)(parts[i] & 0xff);
        }
        return new BigInteger(bytes);
    }

    private static ushort[] BigToParts(BigInteger value)
    {
        byte[] little = value.ToByteArray();
        byte[] network = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            network[15 - i] = i < little.Length ? little[i] : (byte)0;
        }

        ushort[] parts = new ushort[8];
        for (int i = 0; i < 8; i++)
        {
            parts[i] = (ushort)((network[i * 2] << 8) | network[i * 2 + 1]);
        }
        return parts;
    }

    private static BigInteger Max128()
    {
        return (BigInteger.One << 128) - BigInteger.One;
    }

    private static BigInteger PrefixMask(int prefix)
    {
        if (prefix <= 0) return BigInteger.Zero;
        if (prefix >= 128) return Max128();
        return Max128() ^ ((BigInteger.One << (128 - prefix)) - BigInteger.One);
    }

    private static string Expanded(ushort[] parts)
    {
        return string.Join(":", parts.Select(p => p.ToString("x4")));
    }

    private static string Compress(ushort[] parts)
    {
        int bestStart = -1, bestLen = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != 0) continue;
            int j = i;
            while (j < parts.Length && parts[j] == 0) j++;
            int len = j - i;
            if (len > bestLen && len > 1)
            {
                bestStart = i;
                bestLen = len;
            }
            i = j;
        }

        if (bestStart < 0) return string.Join(":", parts.Select(p => p.ToString("x")));
        string before = string.Join(":", parts.Take(bestStart).Select(p => p.ToString("x")));
        string after = string.Join(":", parts.Skip(bestStart + bestLen).Select(p => p.ToString("x")));
        if (before.Length == 0 && after.Length == 0) return "::";
        if (before.Length == 0) return "::" + after;
        if (after.Length == 0) return before + "::";
        return before + "::" + after;
    }

    private static string Binary(ushort[] parts)
    {
        return string.Join(" ", parts.Select(p => Convert.ToString(p, 2).PadLeft(16, '0')));
    }

    private static string CountText(int prefix)
    {
        int hostBits = 128 - prefix;
        BigInteger count = BigInteger.One << hostBits;
        if (count <= new BigInteger(9999999999999999L)) return count.ToString("N0", CultureInfo.InvariantCulture).Replace(",", "");
        return "2^" + hostBits + " (" + count.ToString("N0", CultureInfo.InvariantCulture) + ")";
    }

    private static Color Accent()
    {
        return Color.FromArgb(0, 111, 187);
    }

    private static Label GreenLabel(string text)
    {
        return new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), TextAlign = ContentAlignment.MiddleLeft };
    }

    private static Label BlueLabel()
    {
        return new Label { AutoSize = false, Dock = DockStyle.Fill, ForeColor = Accent(), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Consolas", 9F, FontStyle.Bold) };
    }

    private static Label RedLabel()
    {
        return new Label { AutoSize = false, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(176, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Consolas", 8.5F, FontStyle.Bold) };
    }

    private static Control InputWithCopy(TextBox input)
    {
        input.Dock = DockStyle.Fill;
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
        panel.Controls.Add(input, 0, 0);
        panel.Controls.Add(CopyButton(input), 1, 0);
        return panel;
    }

    private static Control LabeledInput(string label, TextBox input)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(2) };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(GreenLabel(label), 0, 0);
        panel.Controls.Add(InputWithCopy(input), 0, 1);
        return panel;
    }

    private static Control SummaryRow(string caption, Label value, Label copySource, string suffix = "")
    {
        var row = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        row.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(0, 5, 0, 0) });
        value.AutoSize = true;
        value.Width = 310;
        row.Controls.Add(value);
        if (!string.IsNullOrEmpty(suffix)) row.Controls.Add(new Label { Text = suffix, AutoSize = true, ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(0, 5, 0, 0) });
        if (copySource != null) row.Controls.Add(CopyButton(copySource));
        return row;
    }

    private static Control RangeSummary(string caption, Label start, Label end)
    {
        var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        outer.Controls.Add(GreenLabel(caption), 0, 0);

        var rows = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 3 };
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22));
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        rows.Controls.Add(GreenLabel("从"), 0, 0);
        rows.Controls.Add(start, 1, 0);
        rows.Controls.Add(CopyButton(start), 2, 0);
        rows.Controls.Add(GreenLabel("到"), 0, 1);
        rows.Controls.Add(end, 1, 1);
        rows.Controls.Add(CopyButton(end), 2, 1);
        outer.Controls.Add(rows, 0, 1);
        return outer;
    }

    private static Control ResultRow(string caption, Label value, Label copySource, int width)
    {
        var row = new TableLayoutPanel { Width = width - 34, Height = 32, ColumnCount = 3 };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, copySource == null ? 1 : 30));
        row.Controls.Add(GreenLabel(caption), 0, 0);
        row.Controls.Add(value, 1, 0);
        if (copySource != null) row.Controls.Add(CopyButton(copySource), 2, 0);
        return row;
    }

    private static Control Hint(string text, int width)
    {
        return new Label { Text = "●  " + text, Width = width - 34, Height = 28, ForeColor = Color.Olive, TextAlign = ContentAlignment.MiddleLeft };
    }

    private static Control Box(Control[] controls, int width)
    {
        var box = new FlowLayoutPanel
        {
            Width = width,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(14, 10, 14, 10),
            Margin = new Padding(0, 0, 0, 12),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        foreach (Control control in controls) box.Controls.Add(control);
        return box;
    }

    private static Button Button(string text, EventHandler click)
    {
        var button = new Button { Text = text, Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 136, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        button.FlatAppearance.BorderSize = 0;
        button.Click += click;
        return button;
    }

    private static Button CopyButton(Control source)
    {
        var button = new Button { Text = "⧉", Width = 26, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Accent(), Margin = new Padding(0) };
        button.FlatAppearance.BorderColor = Color.FromArgb(217, 222, 231);
        button.Click += delegate
        {
            string text = source.Text;
            if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
        };
        return button;
    }
}
