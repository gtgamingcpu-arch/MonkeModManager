using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using MonkeModManager.Internals;

namespace MonkeModManager;

public class BackupManager : Form
{
	private IContainer components;

	private Button button1;

	private Button button2;

	private ListView listView1;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private Button button3;

	private Button button4;

	private ColumnHeader columnHeader3;

	public BackupManager()
	{
		InitializeComponent();
	}

	private void button4_Click(object sender, EventArgs e)
	{
		Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups"));
	}

	private void button3_Click(object sender, EventArgs e)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		if (listView1.CheckedItems != null)
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups");
			if ((int)MessageBox.Show("Once a backup is deleted it's gone forever you can't recover it! This is a permanent action. Make sure this is the correct backup before deleting. Backup Name: " + listView1.CheckedItems[0].Text, "Are you sure?", (MessageBoxButtons)4, (MessageBoxIcon)48) == 6)
			{
				File.Delete(Path.Combine(path, listView1.CheckedItems[0].Text));
				Init();
			}
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		if (listView1.CheckedItems != null)
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups");
			if ((int)MessageBox.Show("Restoring a backup can break some of your currently installed mods so we recommend making a backup before restoring.", "Are you sure?", (MessageBoxButtons)4, (MessageBoxIcon)48) == 6)
			{
				UnzipFile(File.ReadAllBytes(Path.Combine(path, listView1.CheckedItems[0].Text)), Path.Combine(Form1.InstallDirectory, "BepInEx"));
				Form1.instance.GetInstalledMods();
				Init();
			}
		}
	}

	private void UnzipFile(byte[] data, string directory)
	{
		using MemoryStream stream = new MemoryStream(data);
		using Unzip unzip = new Unzip(stream);
		unzip.ExtractToDirectory(directory);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups");
		ZipFile.CreateFromDirectory(Path.Combine(Form1.InstallDirectory, "BepInEx"), Path.Combine(path, $"Backup-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.zip"));
		Init();
	}

	private void BackupManager_Load(object sender, EventArgs e)
	{
		Init();
	}

	private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if ((int)e.NewValue != 1)
		{
			return;
		}
		foreach (ListViewItem item in listView1.Items)
		{
			ListViewItem val = item;
			if (val.Index != e.Index)
			{
				val.Checked = false;
			}
		}
	}

	private void Init()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		listView1.Items.Clear();
		Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups"));
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonkeModManager", "Backups");
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path);
		foreach (string text in files)
		{
			if (Path.GetExtension(text).ToLower() == ".zip")
			{
				string fileName = Path.GetFileName(text);
				if (fileName != null)
				{
					ListViewItem val = new ListViewItem(fileName);
					val.SubItems.Add(File.GetCreationTime(text).ToString());
					val.SubItems.Add(GetFileSize(text));
					listView1.Items.Add(val);
				}
			}
		}
	}

	public string GetFileSize(string filePath)
	{
		double num = new FileInfo(filePath).Length;
		string[] array = new string[5] { "B", "KB", "MB", "GB", "BRUH" };
		int num2 = 0;
		while (num >= 1024.0 && num2 < array.Length)
		{
			num2++;
			num /= 1024.0;
		}
		return $"{num:F2} {array[num2]}";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Expected O, but got Unknown
		button1 = new Button();
		button2 = new Button();
		listView1 = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		button3 = new Button();
		button4 = new Button();
		columnHeader3 = new ColumnHeader();
		((Control)this).SuspendLayout();
		((Control)button1).Location = new Point(468, 292);
		((Control)button1).Name = "button1";
		((Control)button1).Size = new Size(138, 24);
		((Control)button1).TabIndex = 0;
		((Control)button1).Text = "Create New Backup";
		((ButtonBase)button1).UseVisualStyleBackColor = true;
		((Control)button1).Click += button1_Click;
		((Control)button2).Location = new Point(324, 292);
		((Control)button2).Name = "button2";
		((Control)button2).Size = new Size(138, 24);
		((Control)button2).TabIndex = 1;
		((Control)button2).Text = "Restore Backup";
		((ButtonBase)button2).UseVisualStyleBackColor = true;
		((Control)button2).Click += button2_Click;
		((Control)listView1).Anchor = (AnchorStyles)15;
		listView1.CheckBoxes = true;
		listView1.Columns.AddRange((ColumnHeader[])(object)new ColumnHeader[3] { columnHeader1, columnHeader2, columnHeader3 });
		listView1.FullRowSelect = true;
		listView1.HideSelection = false;
		((Control)listView1).Location = new Point(12, 12);
		listView1.MultiSelect = false;
		((Control)listView1).Name = "listView1";
		((Control)listView1).Size = new Size(594, 274);
		((Control)listView1).TabIndex = 2;
		listView1.UseCompatibleStateImageBehavior = false;
		listView1.View = (View)1;
		listView1.ItemCheck += new ItemCheckEventHandler(listView1_ItemCheck);
		columnHeader1.Text = "Name";
		columnHeader1.Width = 228;
		columnHeader2.Text = "Creation Date";
		columnHeader2.Width = 131;
		((Control)button3).Location = new Point(180, 292);
		((Control)button3).Name = "button3";
		((Control)button3).Size = new Size(138, 24);
		((Control)button3).TabIndex = 3;
		((Control)button3).Text = "Delete Backup";
		((ButtonBase)button3).UseVisualStyleBackColor = true;
		((Control)button3).Click += button3_Click;
		((Control)button4).Location = new Point(36, 292);
		((Control)button4).Name = "button4";
		((Control)button4).Size = new Size(138, 24);
		((Control)button4).TabIndex = 4;
		((Control)button4).Text = "Open Backup Location";
		((ButtonBase)button4).UseVisualStyleBackColor = true;
		((Control)button4).Click += button4_Click;
		columnHeader3.Text = "Size";
		columnHeader3.Width = 70;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(618, 328);
		((Control)this).Controls.Add((Control)(object)button4);
		((Control)this).Controls.Add((Control)(object)button3);
		((Control)this).Controls.Add((Control)(object)listView1);
		((Control)this).Controls.Add((Control)(object)button2);
		((Control)this).Controls.Add((Control)(object)button1);
		((Form)this).FormBorderStyle = (FormBorderStyle)5;
		((Control)this).Name = "BackupManager";
		((Control)this).Text = "Backup Manager";
		((Form)this).Load += BackupManager_Load;
		((Control)this).ResumeLayout(false);
	}
}
