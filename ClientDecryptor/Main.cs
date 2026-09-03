using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClientDecryptor;

public class Main : Form
{
	private string m_ClientFileLocation = AppDomain.CurrentDomain.BaseDirectory;

	private string m_ClientFileName = "client.exe";

	private static byte[] bytes;

	public static long FileSize;

	private IContainer components = null;

	private Label label1;

	private Label LAB_Status;

	private Label LAB_StatusIS;

	private Button button1;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem creditsToolStripMenuItem;

	private ToolStripMenuItem licenseToolStripMenuItem;

	private Label label4;

	private Label label6;
    private Panel panel1;
    private StatusStrip statusStrip1;
    private PictureBox pictureBox1;

	public Main()
	{
		InitializeComponent();
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = ".exe Files (*.exe)|*.exe|all files (*.*)|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_ClientFileLocation = Path.GetDirectoryName(openFileDialog.FileName);
			m_ClientFileName = Path.GetFileName(openFileDialog.FileName);
			if (!File.Exists(openFileDialog.FileName))
			{
				LAB_StatusIS.Text = "Client Not Found !!!";
				return;
			}
			LAB_StatusIS.Text = "Found...";
			ReadClientFile();
		}
	}

	private void ReadClientFile()
	{
		LAB_StatusIS.Text = "Reading Client...";
		try
		{
			using FileStream fileStream = File.OpenRead(Path.Combine(m_ClientFileLocation, m_ClientFileName));
			FileSize = fileStream.Length;
			bytes = new byte[fileStream.Length];
			fileStream.ReadExactly(bytes, 0, bytes.Length);
			fileStream.Close();
			LAB_StatusIS.Text = "Removing Encryption...";
			RemoveEncryption(bytes, FileSize);
			LAB_StatusIS.Text = "Patching Multi Client Stuff...";
			MultiClientPatch();
			LAB_StatusIS.Text = "Writing new client file...";
			FileStream fileStream2 = File.Open(Path.Combine(m_ClientFileLocation, "decrypted_" + m_ClientFileName), FileMode.OpenOrCreate);
			fileStream2.Write(bytes, 0, bytes.Length);
			fileStream2.Close();
			LAB_StatusIS.Text = "Decrypted Client.exe Created";
		}
		catch (IOException)
		{
			LAB_StatusIS.Text = "File I/O ERROR !!!";
		}
	}

	private static bool FindSignatureOffset(byte[] signature, byte[] buffer, out int offset)
	{
		bool flag = false;
		offset = 0;
		for (int i = 0; i < buffer.Length - signature.Length; i++)
		{
			for (int j = 0; j < signature.Length; j++)
			{
				if (buffer[i + j] == signature[j])
				{
					flag = true;
					continue;
				}
				flag = false;
				break;
			}
			if (flag)
			{
				offset = i;
				break;
			}
		}
		return flag;
	}

	private static bool ErrorCheckPatch(byte[] fileBuffer)
	{
		byte[] signature = new byte[5] { 133, 192, 117, 47, 191 };
		byte[] signature2 = new byte[6] { 133, 192, 95, 94, 117, 47 };
		if (FindSignatureOffset(signature, fileBuffer, out var offset))
		{
			fileBuffer[offset] = 102;
			fileBuffer[offset + 1] = 51;
			fileBuffer[offset + 2] = 192;
			fileBuffer[offset + 3] = 144;
			return true;
		}
		if (FindSignatureOffset(signature2, fileBuffer, out offset))
		{
			fileBuffer[offset + 4] = 144;
			fileBuffer[offset + 5] = 144;
			return true;
		}
		return false;
	}

	private static bool SingleCheckPatch(byte[] fileBuffer)
	{
		byte[] signature = new byte[8] { 199, 68, 36, 16, 17, 1, 0, 0 };
		byte[] signature2 = new byte[7] { 131, 196, 4, 51, 219, 83, 80 };
		if (FindSignatureOffset(signature, fileBuffer, out var offset))
		{
			if (fileBuffer[offset + 23] == 116)
			{
				fileBuffer[offset + 23] = 235;
				return true;
			}
			return false;
		}
		if (FindSignatureOffset(signature2, fileBuffer, out offset))
		{
			if (fileBuffer[offset + 15] == 116)
			{
				fileBuffer[offset + 15] = 235;
				return true;
			}
			return false;
		}
		return false;
	}

	private static bool TripleCheckPatch(byte[] fileBuffer)
	{
		byte[] signature = new byte[7] { 255, 214, 106, 1, 255, 215, 104 };
		byte[] signature2 = new byte[6] { 59, 195, 137, 68, 36, 8 };
		if (FindSignatureOffset(signature, fileBuffer, out var offset))
		{
			if (fileBuffer[offset - 45] == 117 && fileBuffer[offset - 14] == 117 && fileBuffer[offset + 24] == 116)
			{
				fileBuffer[offset - 45] = 235;
				fileBuffer[offset - 14] = 235;
				fileBuffer[offset + 24] = 235;
				return true;
			}
			return false;
		}
		if (FindSignatureOffset(signature2, fileBuffer, out offset))
		{
			if (fileBuffer[offset + 6] == 117 && fileBuffer[offset + 45] == 117 && fileBuffer[offset + 95] == 116)
			{
				fileBuffer[offset + 6] = 235;
				fileBuffer[offset + 45] = 235;
				fileBuffer[offset + 95] = 235;
				return true;
			}
			return false;
		}
		return false;
	}

	private void MultiClientPatch()
	{
		if (TripleCheckPatch(bytes) && SingleCheckPatch(bytes) && ErrorCheckPatch(bytes))
		{
			LAB_StatusIS.Text = "Multi Client Patching...Done";
		}
	}

	private void RemoveEncryption(byte[] InClient, long InClientLength)
	{
		byte[] signature = new byte[8] { 129, 249, 0, 0, 1, 0, 15, 143 };
		byte[] signature2 = new byte[8] { 0, 0, 0, 0, 117, 18, 139, 84 };
		byte[] signature3 = new byte[2] { 15, 133 };
		byte[] signature4 = new byte[2] { 15, 132 };
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		byte[] signature5 = new byte[5] { 44, 82, 0, 0, 118 };
		byte[] signature6 = new byte[4] { 59, 195, 15, 132 };
		int num6 = -1;
		int num7 = -1;
		byte[] signature7 = new byte[14]
		{
			139, 139, 204, 204, 204, 204, 129, 249, 0, 1,
			0, 0, 116, 16
		};
		byte[] signature8 = new byte[9] { 116, 15, 131, 185, 180, 0, 0, 0, 0 };
		byte[] signature9 = new byte[7] { 15, 132, 204, 0, 0, 0, 85 };
		int num8 = -1;
		int num9 = -1;
		int num10 = -1;
		byte[] signature10 = new byte[8] { 74, 131, 202, 240, 66, 138, 148, 50 };
		byte[] signature11 = new byte[10] { 133, 204, 116, 204, 51, 204, 133, 204, 126, 204 };
		byte[] signature12 = new byte[11]
		{
			0, 0, 116, 55, 131, 190, 180, 0, 0, 0,
			0
		};
		int num11 = -1;
		int num12 = -1;
		int num13 = -1;
		byte[] array = new byte[49]
		{
			169, 32, 50, 48, 48, 54, 32, 69, 108, 101,
			99, 116, 114, 111, 110, 105, 99, 32, 65, 114,
			116, 115, 32, 73, 110, 99, 46, 32, 32, 65,
			108, 108, 32, 82, 105, 103, 104, 116, 115, 32,
			82, 101, 115, 101, 114, 118, 101, 100, 46
		};
		byte[] array2 = new byte[49]
		{
			169, 32, 50, 48, 48, 53, 32, 69, 108, 101,
			99, 116, 114, 111, 110, 105, 99, 32, 65, 114,
			116, 115, 32, 73, 110, 99, 46, 32, 32, 65,
			108, 108, 32, 82, 105, 103, 104, 116, 115, 32,
			82, 101, 115, 101, 114, 118, 101, 100, 46
		};
		byte[] array3 = new byte[49]
		{
			169, 32, 50, 48, 48, 57, 32, 69, 108, 101,
			99, 116, 114, 111, 110, 105, 99, 32, 65, 114,
			116, 115, 32, 73, 110, 99, 46, 32, 32, 65,
			108, 108, 32, 82, 105, 103, 104, 116, 115, 32,
			82, 101, 115, 101, 114, 118, 101, 100, 46
		};
		num = ScanClient(256, signature, InClient, InClientLength, 8, 0);
		if (num == -1)
		{
			num2 = ScanClient(256, signature2, InClient, InClientLength, 8, 0);
		}
		if (num != -1 && num2 != -1)
		{
			LAB_StatusIS.Text = "Can't find a login signature in this file ???";
		}
		else if (num != -1)
		{
			num3 = ScanClient(256, signature3, InClient, InClientLength, 2, num);
			num4 = ScanClient(256, signature4, InClient, InClientLength, 2, num);
			if (num4 > num3)
			{
				bytes[num3 + 1] = 132;
				LAB_StatusIS.Text = $"Patching with JE (0x0F 0x84) - (15 132) @{num3:X} - ({num3.ToString()})";
			}
			else if (num3 > num4)
			{
				bytes[num3 + 1] = 133;
				LAB_StatusIS.Text = $"Patching with JNZ (0x0F 0x85) - (15 133) @{num4:X} - ({num4.ToString()})";
			}
		}
		else if (num2 != -1)
		{
			bytes[num2 + 4] = 235;
			LAB_StatusIS.Text = $"Patching with (0xEB) - (235) @{num2 + 4:X} - ({(num2 + 4).ToString()})";
			num5 = 1;
		}
		num6 = ScanClient(256, signature5, InClient, InClientLength, 5, 0);
		if (num6 != -1)
		{
			num7 = ScanClient(256, signature6, InClient, InClientLength, 4, num6 - 32);
		}
		if (num6 == -1 || num7 == -1)
		{
			LAB_StatusIS.Text = "Can't find the blowfish signature";
		}
		else
		{
			bytes[num7 + 1] = 192;
			LAB_StatusIS.Text = $"Patching with CMP (0xC0) - (192) @{num7:X} - ({num7.ToString()})";
		}
		num8 = ScanClient(204, signature7, InClient, InClientLength, 14, 0);
		if (num8 != -1)
		{
			num10 = ScanClient(204, signature9, InClient, InClientLength, 7, num8 - 32);
		}
		num9 = ScanClient(204, signature8, InClient, InClientLength, 9, 0);
		if (num8 == -1 && num10 == -1 && num9 == -1)
		{
			LAB_StatusIS.Text = "Can't find old OR new Twofish signatures";
		}
		else if (num8 != -1 && num10 != -1)
		{
			bytes[num10 + 1] = 133;
			LAB_StatusIS.Text = $"Patching (old TF) with JNZ (0x85) - (133) @{num10 + 1:X} - ({(num10 + 1).ToString()})";
		}
		else if (num9 != -1)
		{
			bytes[num9] = 235;
			LAB_StatusIS.Text = $"Patching (new TF) with (0xEB) - (235) @{num9:X} - ({num9.ToString()})";
		}
		num11 = ScanClient(256, signature10, InClient, InClientLength, 8, 0);
		num13 = ScanClient(256, signature12, InClient, InClientLength, 11, 0);
		if (num11 != -1)
		{
			num12 = ScanClient(204, signature11, InClient, InClientLength, 10, num11 - 256);
		}
		if (num11 == -1 && num12 == -1 && num13 == -1)
		{
			LAB_StatusIS.Text = "Can't find any MD5 Decrypt signatures ???";
		}
		else
		{
			switch (num5)
			{
			case -1:
				bytes[num12] = 59;
				LAB_StatusIS.Text = $"Patching old MD5 with CMP (0x3B) - (59) @{num12:X} - ({num12.ToString()})";
				break;
			case 1:
				bytes[num13 + 2] = 235;
				LAB_StatusIS.Text = $"Patching (new MD5 (D2+2)) with (0xEB) - (235) @{num13:X} - ({num13.ToString()})";
				break;
			}
		}
		LAB_StatusIS.Text = "Client Decryption Done...";
	}

	private int ScanClient(int FlexByte, byte[] signature, byte[] client, long client_length, int sig_length, int startat)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool myBool = GetMyBool(FlexByte, 100);
		byte myByte = GetMyByte(FlexByte);
		if (startat == -1)
		{
			startat = 0;
		}
		for (num2 = startat; num2 < client_length - sig_length; num2++)
		{
			for (num3 = 0; num3 < sig_length && (!myBool || signature[num3] == myByte || signature[num3] == client[num2 + num3]) && (myBool || signature[num3] == client[num2 + num3]); num3++)
			{
				if (num3 == sig_length - 1)
				{
					num++;
					if (num >= 1)
					{
						return num2;
					}
				}
			}
		}
		return -1;
	}

	private bool GetMyBool(int a, int b)
	{
		int value = a & b;
		return Convert.ToBoolean(value);
	}

	private byte GetMyByte(int flex)
	{
		int value = flex & 0xFF;
		return Convert.ToByte(value);
	}

	private void label3_Click(object sender, EventArgs e)
	{
	}

	private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		base.DialogResult = MessageBox.Show("FingersMcSteal \nPraxis & Sythen \nThe RunUO Community", "Credits", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

    private void InitializeComponent()
    {
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Main));
        label1 = new Label();
        LAB_Status = new Label();
        LAB_StatusIS = new Label();
        button1 = new Button();
        menuStrip1 = new MenuStrip();
        creditsToolStripMenuItem = new ToolStripMenuItem();
        licenseToolStripMenuItem = new ToolStripMenuItem();
        label4 = new Label();
        label6 = new Label();
        pictureBox1 = new PictureBox();
        panel1 = new Panel();
        statusStrip1 = new StatusStrip();
        menuStrip1.SuspendLayout();
        ((ISupportInitialize)pictureBox1).BeginInit();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.BackColor = Color.Black;
        label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label1.ForeColor = Color.FromArgb(192, 192, 255);
        label1.Location = new Point(0, 149);
        label1.Margin = new Padding(4, 0, 4, 0);
        label1.Name = "label1";
        label1.Size = new Size(464, 15);
        label1.TabIndex = 0;
        label1.Text = "UO Decryptor By FingersMcSteal 2011  |  Edited By: ghostbyte420 And Praxiis (2014)";
        // 
        // LAB_Status
        // 
        LAB_Status.AutoSize = true;
        LAB_Status.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        LAB_Status.Location = new Point(213, 127);
        LAB_Status.Margin = new Padding(4, 0, 4, 0);
        LAB_Status.Name = "LAB_Status";
        LAB_Status.Size = new Size(60, 13);
        LAB_Status.TabIndex = 3;
        LAB_Status.Text = "STATUS:";
        // 
        // LAB_StatusIS
        // 
        LAB_StatusIS.AutoSize = true;
        LAB_StatusIS.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        LAB_StatusIS.Location = new Point(278, 125);
        LAB_StatusIS.Margin = new Padding(4, 0, 4, 0);
        LAB_StatusIS.Name = "LAB_StatusIS";
        LAB_StatusIS.Size = new Size(93, 15);
        LAB_StatusIS.TabIndex = 5;
        LAB_StatusIS.Text = "App Loaded OK";
        // 
        // button1
        // 
        button1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
        button1.ForeColor = Color.DarkRed;
        button1.Location = new Point(213, 71);
        button1.Margin = new Padding(4, 3, 4, 3);
        button1.Name = "button1";
        button1.Size = new Size(239, 41);
        button1.TabIndex = 6;
        button1.Text = "Search For Your Client...";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click_1;
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { creditsToolStripMenuItem, licenseToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Padding = new Padding(7, 2, 0, 2);
        menuStrip1.RenderMode = ToolStripRenderMode.Professional;
        menuStrip1.Size = new Size(463, 24);
        menuStrip1.TabIndex = 7;
        menuStrip1.Text = "menuStrip1";
        // 
        // creditsToolStripMenuItem
        // 
        creditsToolStripMenuItem.Margin = new Padding(490, 0, 0, 0);
        creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
        creditsToolStripMenuItem.Size = new Size(56, 20);
        creditsToolStripMenuItem.Text = "Credits";
        creditsToolStripMenuItem.Click += creditsToolStripMenuItem_Click;
        // 
        // licenseToolStripMenuItem
        // 
        licenseToolStripMenuItem.Name = "licenseToolStripMenuItem";
        licenseToolStripMenuItem.Size = new Size(12, 20);
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label4.ForeColor = Color.Maroon;
        label4.Location = new Point(213, 27);
        label4.Margin = new Padding(4, 0, 4, 0);
        label4.Name = "label4";
        label4.Size = new Size(92, 16);
        label4.TabIndex = 8;
        label4.Text = "Description:";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label6.Location = new Point(213, 44);
        label6.Margin = new Padding(4, 0, 4, 0);
        label6.Name = "label6";
        label6.Size = new Size(241, 15);
        label6.TabIndex = 9;
        label6.Text = "Look For 'decrypted_client' In Game Folder";
        // 
        // pictureBox1
        // 
        pictureBox1.Dock = DockStyle.Fill;
        pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
        pictureBox1.Location = new Point(0, 0);
        pictureBox1.Margin = new Padding(4, 3, 4, 3);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(200, 122);
        pictureBox1.TabIndex = 10;
        pictureBox1.TabStop = false;
        // 
        // panel1
        // 
        panel1.Controls.Add(pictureBox1);
        panel1.Dock = DockStyle.Left;
        panel1.Location = new Point(0, 24);
        panel1.Name = "panel1";
        panel1.Size = new Size(200, 122);
        panel1.TabIndex = 11;
        // 
        // statusStrip1
        // 
        statusStrip1.Location = new Point(0, 146);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(463, 22);
        statusStrip1.SizingGrip = false;
        statusStrip1.TabIndex = 12;
        statusStrip1.Text = "statusStrip1";
        // 
        // Main
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(463, 168);
        Controls.Add(label1);
        Controls.Add(LAB_Status);
        Controls.Add(panel1);
        Controls.Add(label6);
        Controls.Add(label4);
        Controls.Add(button1);
        Controls.Add(LAB_StatusIS);
        Controls.Add(menuStrip1);
        Controls.Add(statusStrip1);
        ForeColor = Color.Black;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MainMenuStrip = menuStrip1;
        Margin = new Padding(4, 3, 4, 3);
        MaximizeBox = false;
        Name = "Main";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Client Decryptor";
        TopMost = true;
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ((ISupportInitialize)pictureBox1).EndInit();
        panel1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
