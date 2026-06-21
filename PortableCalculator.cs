using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
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
        Text = "YouQian IPv6 子网计算工具";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1080, 800);
        Size = new Size(1220, 860);
        Font = new Font("Microsoft YaHei UI", 9F);
        BackColor = Color.White;

        try
        {
            if (System.IO.File.Exists("ipv6_pixel_grid_with_6_icon.ico"))
            {
                this.Icon = new Icon("ipv6_pixel_grid_with_6_icon.ico");
            }
        }
        catch { }

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
            Text = "YouQian IPv6 子网计算工具  V1.2.6",
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
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
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

        var exportBtn = new Button
        {
            Text = "导出当前子网可用地址（EXCEL表格）",
            Width = 580,
            Height = 32,
            BackColor = Color.FromArgb(0, 136, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 8, 0, 0),
            Font = new Font(Font.FontFamily, 9F, FontStyle.Bold)
        };
        exportBtn.FlatAppearance.BorderSize = 0;
        exportBtn.Click += ExportClick;

        panel.Controls.Add(Box(new Control[]
        {
            ResultRow("当前子网地址：", networkFull, networkFull, 610),
            Hint("当前子网地址起始到结束", 610),
            ResultRow("范围起始：", rangeStart, rangeStart, 610),
            ResultRow("范围结束：", rangeEnd, rangeEnd, 610),
            ResultRow("地址数量：", totalCount, null, 610),
            exportBtn
        }, 610));

        var exportNextBtn = new Button
        {
            Text = "导出下个子网可用地址（EXCEL表格）",
            Width = 580,
            Height = 32,
            BackColor = Color.FromArgb(0, 136, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 8, 0, 0),
            Font = new Font(Font.FontFamily, 9F, FontStyle.Bold)
        };
        exportNextBtn.FlatAppearance.BorderSize = 0;
        exportNextBtn.Click += ExportNextClick;

        panel.Controls.Add(Box(new Control[]
        {
            ResultRow("下个子网地址：", nextNetworkFull, nextNetworkFull, 610),
            Hint("下个子网当前子网地址起始到结束", 610),
            ResultRow("下个子网起始：", nextRangeStart, nextRangeStart, 610),
            ResultRow("下个子网结束：", nextRangeEnd, nextRangeEnd, 610),
            ResultRow("地址数量：", nextTotalCount, null, 610),
            exportNextBtn
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

    private void ExportClick(object sender, EventArgs e)
    {
        DoExport(false);
    }

    private void ExportNextClick(object sender, EventArgs e)
    {
        DoExport(true);
    }

    private void DoExport(bool isNext)
    {
        try
        {
            int prefix = (int)prefixNumber.Value;
            ushort[] hostParts;
            try
            {
                hostParts = ParseIPv6(hostInput.Text);
            }
            catch
            {
                MessageBox.Show("请先输入有效的主机IPv6地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BigInteger hostValue = PartsToBig(hostParts);
            BigInteger maskValue = PrefixMask(prefix);
            BigInteger networkValue = hostValue & maskValue;
            BigInteger endValue = networkValue | (Max128() ^ maskValue);

            if (isNext)
            {
                if (endValue == Max128())
                {
                    MessageBox.Show("已到达 IPv6 地址空间尽头，无下个子网，无法导出。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                networkValue = endValue + 1;
                endValue = networkValue | (Max128() ^ maskValue);
            }

            BigInteger totalCountVal = endValue - networkValue + 1;

            int exportLimit = 65536;
            if (totalCountVal > exportLimit)
            {
                string subName = isNext ? "下个子网" : "当前子网";
                var dr = MessageBox.Show(
                    string.Format("{0}包含的地址数量为 {1}。\n由于 Excel 行数限制以及性能原因，本次导出将仅生成前 {2} 个地址。\n是否继续导出？", subName, totalCountVal, exportLimit),
                    "导出确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (dr != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                exportLimit = (int)totalCountVal;
            }

            string compressSubnetStr = Compress(BigToParts(networkValue)).ToLower() + "_" + prefix;
            string fileName = compressSubnetStr.Replace(":", "_") + ".xlsx";

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
                sfd.FileName = fileName;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    byte[] templateBytes = GetTemplateBytes();
                    if (templateBytes == null) return;

                    File.WriteAllBytes(sfd.FileName, templateBytes);

                    string sheetXml = GenerateSheetXml(networkValue, prefix, exportLimit);

                    using (var archive = ZipFile.Open(sfd.FileName, ZipArchiveMode.Update))
                    {
                        var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
                        if (entry != null)
                        {
                            entry.Delete();
                        }
                        var newEntry = archive.CreateEntry("xl/worksheets/sheet1.xml");
                        using (var writer = new StreamWriter(newEntry.Open(), Encoding.UTF8))
                        {
                            writer.Write(sheetXml);
                        }
                    }

                    MessageBox.Show("导出成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private byte[] GetTemplateBytes()
    {
        try
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("moban.xlsx"))
            {
                if (stream != null)
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
        }
        catch { }

        if (File.Exists("moban.xlsx"))
        {
            return File.ReadAllBytes("moban.xlsx");
        }

        MessageBox.Show("未找到 Excel 模板资源或文件 moban.xlsx，请确保它存在于程序当前目录中。", "导出错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return null;
    }

    private string GenerateSheetXml(BigInteger networkValue, int prefix, int exportLimit)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" mc:Ignorable=\"x14ac xr xr2 xr3\" xmlns:x14ac=\"http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac\" xmlns:xr=\"http://schemas.microsoft.com/office/spreadsheetml/2014/revision\" xmlns:xr2=\"http://schemas.microsoft.com/office/spreadsheetml/2015/revision2\" xmlns:xr3=\"http://schemas.microsoft.com/office/spreadsheetml/2016/revision3\" xr:uid=\"{6B43EABC-031E-4A80-9AA0-A25C094EC4A3}\">");
        sb.AppendFormat("<dimension ref=\"A1:D{0}\"/>", 9 + exportLimit);
        sb.Append("<sheetViews><sheetView tabSelected=\"1\" workbookViewId=\"0\"><selection activeCell=\"D9\" sqref=\"D9\"/></sheetView></sheetViews>");
        sb.Append("<sheetFormatPr defaultColWidth=\"9\" defaultRowHeight=\"14.25\" x14ac:dyDescent=\"0.2\"/>");
        sb.Append("<cols>");
        sb.Append("<col min=\"1\" max=\"1\" width=\"18.75\" customWidth=\"1\"/>");
        sb.Append("<col min=\"2\" max=\"2\" width=\"11.25\" customWidth=\"1\"/>");
        sb.Append("<col min=\"3\" max=\"3\" width=\"48.25\" customWidth=\"1\"/>");
        sb.Append("<col min=\"4\" max=\"4\" width=\"50.625\" customWidth=\"1\"/>");
        sb.Append("</cols>");
        sb.Append("<sheetData>");

        string fullSubnetStr = Expanded(BigToParts(networkValue)).ToUpper() + "/" + prefix;
        string compressSubnetStr = Compress(BigToParts(networkValue)).ToLower() + "/" + prefix;

        // 计算可用范围
        BigInteger maskValue = PrefixMask(prefix);
        BigInteger endValue = networkValue | (Max128() ^ maskValue);
        string rangeStartFull = Expanded(BigToParts(networkValue)).ToUpper();
        string rangeEndFull = Expanded(BigToParts(endValue)).ToUpper();
        string rangeStartCompress = Compress(BigToParts(networkValue)).ToLower();
        string rangeEndCompress = Compress(BigToParts(endValue)).ToLower();

        // Row 1 (所属VLAN | VLAN 300)
        sb.Append("<row r=\"1\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A1\" s=\"9\" t=\"inlineStr\"><is><t>所属VLAN</t></is></c>");
        sb.Append("<c r=\"B1\" s=\"8\"/>");
        sb.Append("<c r=\"C1\" s=\"5\" t=\"inlineStr\"><is><t>VLAN 300</t></is></c>");
        sb.Append("<c r=\"D1\" s=\"3\"/>");
        sb.Append("</row>");

        // Row 2 (IPv4段)
        sb.Append("<row r=\"2\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A2\" s=\"8\" t=\"inlineStr\"><is><t>IPv4段</t></is></c>");
        sb.Append("<c r=\"B2\" s=\"8\"/>");
        sb.Append("<c r=\"C2\" s=\"5\" t=\"inlineStr\"><is><t>IPv4:192.168.1.0/24</t></is></c>");
        sb.Append("<c r=\"D2\" s=\"4\"/>");
        sb.Append("</row>");

        // Row 3 (IPv4地址池)
        sb.Append("<row r=\"3\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A3\" s=\"8\" t=\"inlineStr\"><is><t>IPv4地址池</t></is></c>");
        sb.Append("<c r=\"B3\" s=\"8\"/>");
        sb.Append("<c r=\"C3\" s=\"5\" t=\"inlineStr\"><is><t>192.168.1.1-192.168.1.254</t></is></c>");
        sb.Append("<c r=\"D3\" s=\"4\"/>");
        sb.Append("</row>");

        // Row 4 (IPv4网关)
        sb.Append("<row r=\"4\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A4\" s=\"8\" t=\"inlineStr\"><is><t>IPv4网关</t></is></c>");
        sb.Append("<c r=\"B4\" s=\"8\"/>");
        sb.Append("<c r=\"C4\" s=\"5\" t=\"inlineStr\"><is><t>192.168.1.254</t></is></c>");
        sb.Append("<c r=\"D4\" s=\"4\"/>");
        sb.Append("</row>");

        // Row 5 (IPv6段)
        sb.Append("<row r=\"5\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A5\" s=\"9\" t=\"inlineStr\"><is><t>IPv6段</t></is></c>");
        sb.Append("<c r=\"B5\" s=\"8\"/>");
        sb.AppendFormat("<c r=\"C5\" s=\"5\" t=\"inlineStr\"><is><t>{0}</t></is></c>", fullSubnetStr);
        sb.AppendFormat("<c r=\"D5\" s=\"5\" t=\"inlineStr\"><is><t>{0}</t></is></c>", compressSubnetStr);
        sb.Append("</row>");

        // Row 6 (新增：IPv6可用范围)
        sb.Append("<row r=\"6\" spans=\"1:4\" ht=\"33\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A6\" s=\"9\" t=\"inlineStr\"><is><t>IPv6可用范围</t></is></c>");
        sb.Append("<c r=\"B6\" s=\"8\"/>");
        sb.AppendFormat("<c r=\"C6\" s=\"5\" t=\"inlineStr\"><is><t xml:space=\"preserve\">从【{0}】&#10;到【{1}】</t></is></c>", rangeStartFull, rangeEndFull);
        sb.AppendFormat("<c r=\"D6\" s=\"5\" t=\"inlineStr\"><is><t xml:space=\"preserve\">从【{0}】&#10;到【{1}】</t></is></c>", rangeStartCompress, rangeEndCompress);
        sb.Append("</row>");

        // Row 7 (当前子网IPv6地址数量)
        sb.Append("<row r=\"7\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A7\" s=\"9\" t=\"inlineStr\"><is><t>当前子网IPv6地址数量</t></is></c>");
        sb.Append("<c r=\"B7\" s=\"8\"/>");
        sb.AppendFormat("<c r=\"C7\" s=\"5\" t=\"inlineStr\"><is><t>{0}</t></is></c>", CountText(prefix));
        sb.Append("<c r=\"D7\" s=\"5\"/>");
        sb.Append("</row>");

        // Row 8 (IPv6网关)
        sb.Append("<row r=\"8\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A8\" s=\"8\" t=\"inlineStr\"><is><t>IPv6网关</t></is></c>");
        sb.Append("<c r=\"B8\" s=\"8\"/>");
        sb.Append("<c r=\"C8\" s=\"5\"/>");
        sb.Append("<c r=\"D8\" s=\"4\"/>");
        sb.Append("</row>");

        // Row 9 (Header)
        sb.Append("<row r=\"9\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">");
        sb.Append("<c r=\"A9\" s=\"2\" t=\"inlineStr\"><is><t>IPV4地址</t></is></c>");
        sb.Append("<c r=\"B9\" s=\"6\" t=\"inlineStr\"><is><t>后一位</t></is></c>");
        sb.Append("<c r=\"C9\" s=\"6\" t=\"inlineStr\"><is><t>完整IPV6 (大写)</t></is></c>");
        sb.Append("<c r=\"D9\" s=\"7\" t=\"inlineStr\"><is><t>压缩版IPV6 (小写)</t></is></c>");
        sb.Append("</row>");

        BigInteger current = networkValue;
        for (int i = 0; i < exportLimit; i++)
        {
            int rowNum = 10 + i;
            ushort[] parts = BigToParts(current);
            string ipFull = Expanded(parts).ToUpper() + "/" + prefix;
            string ipCompress = Compress(parts).ToLower() + "/" + prefix;
            int lastByte = (int)(current & 0xFF);

            sb.AppendFormat("<row r=\"{0}\" spans=\"1:4\" ht=\"24\" customHeight=\"1\" x14ac:dyDescent=\"0.2\">", rowNum);
            sb.AppendFormat("<c r=\"A{0}\" s=\"1\" t=\"inlineStr\"><is><t>/</t></is></c>", rowNum);
            sb.AppendFormat("<c r=\"B{0}\" s=\"1\"><v>{1}</v></c>", rowNum, lastByte);
            sb.AppendFormat("<c r=\"C{0}\" s=\"5\" t=\"inlineStr\"><is><t>{1}</t></is></c>", rowNum, ipFull);
            sb.AppendFormat("<c r=\"D{0}\" s=\"1\" t=\"inlineStr\"><is><t>{1}</t></is></c>", rowNum, ipCompress);
            sb.Append("</row>");

            current += 1;
        }

        sb.Append("</sheetData>");
        sb.Append("<mergeCells count=\"8\">");
        sb.Append("<mergeCell ref=\"A1:B1\"/>");
        sb.Append("<mergeCell ref=\"A2:B2\"/>");
        sb.Append("<mergeCell ref=\"A3:B3\"/>");
        sb.Append("<mergeCell ref=\"A4:B4\"/>");
        sb.Append("<mergeCell ref=\"A5:B5\"/>");
        sb.Append("<mergeCell ref=\"A6:B6\"/>");
        sb.Append("<mergeCell ref=\"A7:B7\"/>");
        sb.Append("<mergeCell ref=\"A8:B8\"/>");
        sb.Append("</mergeCells>");
        sb.Append("<pageMargins left=\"0.75\" right=\"0.75\" top=\"1\" bottom=\"1\" header=\"0.5\" footer=\"0.5\"/>");
        sb.Append("</worksheet>");

        return sb.ToString();
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
        var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        outer.Controls.Add(GreenLabel(caption), 0, 0);

        var rows = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 3, Margin = new Padding(0) };
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
        var button = new Button
        {
            Text = "\uE8C8",
            Font = new Font("Segoe MDL2 Assets", 9F),
            Width = 26,
            Height = 24,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Accent(),
            Margin = new Padding(0)
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(217, 222, 231);

        var tt = new ToolTip();
        tt.SetToolTip(button, "复制");

        button.Click += delegate
        {
            string text = source.Text;
            if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
        };
        return button;
    }
}
