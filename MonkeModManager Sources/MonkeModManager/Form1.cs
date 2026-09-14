using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonkeModManager.Internals;
using MonkeModManager.Internals.SimpleJSON;
using MonkeModManager.Properties;

namespace MonkeModManager;

public class Form1 : Form
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<KeyValuePair<string, JSONNode>, JSONNode> _003C_003E9__34_0;

		public static MethodInvoker _003C_003E9__46_0;

		internal JSONNode _003CLoadReleases_003Eb__34_0(KeyValuePair<string, JSONNode> x)
		{
			return x.Value["rank"];
		}

		internal void _003CCheckForUpdates_003Eb__46_0()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			MessageBox.Show("You have an old version of MMM.", "Update Available", (MessageBoxButtons)0, (MessageBoxIcon)64);
			Process.Start("https://github.com/NgbatzYT/MonkeModManager/releases/latest");
			Environment.Exit(0);
		}
	}

	public static Form1 instance;

	private string DefaultOculusInstallDirectory = "C:\\Program Files\\Oculus\\Software\\Software\\another-axiom-gorilla-tag";

	private string DefaultSteamInstallDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag";

	public static string InstallDirectory = "";

	private Dictionary<string, int> groups = new Dictionary<string, int>();

	private Dictionary<string, string> installed = new Dictionary<string, string>();

	private Dictionary<string, bool> installedr = new Dictionary<string, bool>();

	private List<ReleaseInfo> releases;

	private bool modsDisabled;

	private int CurrentVersion = 18;

	public const string VersionNumber = "2.7.2.2";

	private string currentMod;

	private CookieContainer PermCookie;

	private IContainer components;

	private Button button1;

	private ListView listViewMods;

	private ColumnHeader columnHeader3;

	private ColumnHeader columnHeader4;

	private TabPage Utilities;

	private Button buttonUninstallAll;

	private GroupBox groupBox1;

	private Label labelOpen;

	private Button buttonOpenGameFolder;

	private Button buttonOpenConfig;

	private Button buttonBepInEx;

	private Button buttonDiscordLink;

	private PictureBox pictureBox1;

	private Label labelVersion;

	private Button button2;

	private TabPage Updates;

	private ListView listView1;

	private ColumnHeader columnHeader1;

	private TabPage Installed;

	private TabControl tabControlMain;

	private Button modsButton;

	private TextBox textBoxDirectory;

	private Button buttonFolderBrowser;

	private Label label1;

	private Button buttonInstall;

	private Label labelStatus;

	private ColumnHeader columnHeaderName;

	private ColumnHeader columnHeaderAuthor;

	private ContextMenuStrip contextMenuStripMain;

	private ToolStripMenuItem viewInfoToolStripMenuItem;

	private Button buttonModInfo;

	private Button button4;

	private Button button3;

	private Button button5;

	private Button button6;

	private Button button7;

	private Button button8;

	public Form1()
	{
		InitializeComponent();
	}

	private void buttonFolderBrowser_Click(object sender, EventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).FileName = "Gorilla Tag.exe";
			((FileDialog)val).Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
			((FileDialog)val).FilterIndex = 1;
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				string fileName = ((FileDialog)val).FileName;
				if (Path.GetFileName(fileName).Equals("Gorilla Tag.exe"))
				{
					InstallDirectory = Path.GetDirectoryName(fileName);
					((Control)textBoxDirectory).Text = InstallDirectory;
					EditConfig(InstallDirectory);
				}
				else
				{
					MessageBox.Show("That's not Gorilla Tag, please try again.", "Error!", (MessageBoxButtons)0, (MessageBoxIcon)16);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void buttonInstall_Click(object sender, EventArgs e)
	{
		new Thread(Install).Start();
	}

	private async void Install()
	{
		try
		{
			ChangeInstallButtonState(enabled: false);
			UpdateStatus("Starting install sequence...");
			foreach (ReleaseInfo release in releases)
			{
				currentMod = release.Name;
				if (!release.Install)
				{
					continue;
				}
				UpdateStatus($"Downloading...{release.Name}");
				byte[] array = await DownloadFile(release.Link, release.Name);
				UpdateStatus($"Installing...{release.Name}");
				string fileName = Path.GetFileName(release.Link);
				if (Path.GetExtension(fileName).Equals(".dll"))
				{
					string text;
					if (release.InstallLocation == null)
					{
						text = Path.Combine(InstallDirectory, "BepInEx\\plugins", Regex.Replace(release.Name, "\\s+", string.Empty));
						if (!Directory.Exists(text))
						{
							Directory.CreateDirectory(text);
						}
					}
					else
					{
						text = Path.Combine(InstallDirectory, release.InstallLocation);
					}
					File.WriteAllBytes(Path.Combine(text, fileName), array);
					string path = Path.Combine(InstallDirectory, "BepInEx\\plugins", fileName);
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
				else
				{
					UnzipFile(array, (release.InstallLocation != null) ? Path.Combine(InstallDirectory, release.InstallLocation) : InstallDirectory);
				}
				UpdateStatus("Installed " + release.Name + "!");
			}
			UpdateStatus("Install complete!");
			ChangeInstallButtonState(enabled: true);
			GetInstalledMods();
			ConfigFix();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Hey, an error occurred. Please go to discord.gg/monkemod and tell 'Ngbatz' to fix this mod: '" + currentMod + "'. Error: " + ex.Message, "Error!", (MessageBoxButtons)0, (MessageBoxIcon)16);
		}
	}

	private void UnzipFile(byte[] data, string directory)
	{
		using MemoryStream stream = new MemoryStream(data);
		using Unzip unzip = new Unzip(stream);
		unzip.ExtractToDirectory(directory);
	}

	private void ChangeInstallButtonState(bool enabled)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		((Control)this).Invoke((Delegate)(MethodInvoker)delegate
		{
			((Control)buttonInstall).Enabled = enabled;
		});
	}

	private async Task<byte[]> DownloadFile(string url, string name)
	{
		TaskCompletionSource<byte[]> t = new TaskCompletionSource<byte[]>();
		WebClient webClient = new WebClient();
		webClient.Proxy = null;
		webClient.DownloadProgressChanged += delegate(object s, DownloadProgressChangedEventArgs e)
		{
			UpdateStatus($"Downloading... {name} {e.ProgressPercentage}%");
		};
		webClient.DownloadDataCompleted += delegate(object s, DownloadDataCompletedEventArgs e)
		{
			t.SetResult(e.Result);
		};
		webClient.DownloadDataAsync(new Uri(url));
		return t.Task.Result;
	}

	private void listViewMods_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		ReleaseInfo release = (ReleaseInfo)e.Item.Tag;
		if (release.Dependencies.Count > 0)
		{
			foreach (ListViewItem item in listViewMods.Items)
			{
				ListViewItem val = item;
				ReleaseInfo plugin = (ReleaseInfo)val.Tag;
				if (plugin.Name == release.Name || !release.Dependencies.Contains(plugin.Name))
				{
					continue;
				}
				if (e.Item.Checked)
				{
					val.Checked = true;
					val.ForeColor = Color.DimGray;
					continue;
				}
				release.Install = false;
				if (releases.Count((ReleaseInfo x) => plugin.Dependents.Contains(x.Name) && x.Install) <= 1)
				{
					val.Checked = false;
					val.ForeColor = Color.Black;
				}
			}
		}
		if (release.Dependents.Count > 0 && releases.Count((ReleaseInfo x) => release.Dependents.Contains(x.Name) && x.Install) > 0)
		{
			e.Item.Checked = true;
		}
		if (release.Name.Contains("BepInEx"))
		{
			e.Item.Checked = true;
		}
		release.Install = e.Item.Checked;
	}

	private void listViewMods_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		((Control)buttonModInfo).Enabled = listViewMods.SelectedItems.Count > 0;
	}

	private void listViewMods_DoubleClick(object sender, EventArgs e)
	{
		OpenLinkFromRelease();
	}

	private void viewInfoToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenLinkFromRelease();
	}

	private void buttonDiscordLink_Click(object sender, EventArgs e)
	{
		Process.Start("https://discord.gg/monkemod");
	}

	private void buttonOpenGameFolder_Click(object sender, EventArgs e)
	{
		if (Directory.Exists(InstallDirectory))
		{
			Process.Start(InstallDirectory);
		}
	}

	private void buttonOpenConfigFolder_Click(object sender, EventArgs e)
	{
		string text = Path.Combine(InstallDirectory, "BepInEx\\config");
		if (Directory.Exists(text))
		{
			Process.Start(text);
		}
	}

	private void buttonOpenBepInExFolder_Click(object sender, EventArgs e)
	{
		string text = Path.Combine(InstallDirectory, "BepInEx/plugins");
		if (Directory.Exists(text))
		{
			Process.Start(text);
		}
	}

	private void buttonUninstallAll_Click(object sender, EventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if ((int)MessageBox.Show("You are about to delete all your mods. This cannot be undone!\n\nAre you sure you wish to continue?", "Confirm Delete", (MessageBoxButtons)4) != 6)
		{
			return;
		}
		UpdateStatus("Removing all mods");
		string path = Path.Combine(InstallDirectory, "BepInEx\\plugins");
		try
		{
			string[] directories = Directory.GetDirectories(path);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.Delete(directories[i], recursive: true);
			}
			directories = Directory.GetFiles(path);
			for (int i = 0; i < directories.Length; i++)
			{
				File.Delete(directories[i]);
			}
		}
		catch (Exception arg)
		{
			MessageBox.Show($"Something went wrong! Error: {arg}", "Error", (MessageBoxButtons)0, (MessageBoxIcon)16);
			UpdateStatus("Failed to remove mods.");
			return;
		}
		UpdateStatus("All mods removed successfully!");
	}

	private void buttonModInfo_Click(object sender, EventArgs e)
	{
		OpenLinkFromRelease();
	}

	private void OpenLinkFromRelease()
	{
		if (listViewMods.SelectedItems.Count > 0)
		{
			ReleaseInfo releaseInfo = (ReleaseInfo)listViewMods.SelectedItems[0].Tag;
			UpdateStatus("Opening GitHub page for " + releaseInfo.Name);
			Process.Start($"https://github.com/{releaseInfo.GitPath}");
		}
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		CheckForUpdates();
		instance = this;
		InstallDirectory = Settings.Default.InstallDirectory;
		releases = new List<ReleaseInfo>();
		((Control)labelVersion).Text = "Monke Mod Manager v2.7.2.2";
		if (InstallDirectory != "" && File.Exists(Path.Combine(InstallDirectory, "Gorilla Tag.exe")))
		{
			((Control)textBoxDirectory).Text = InstallDirectory;
		}
		else if (File.Exists(Path.Combine(DefaultSteamInstallDirectory, "Gorilla Tag.exe")))
		{
			InstallDirectory = DefaultSteamInstallDirectory;
			((Control)textBoxDirectory).Text = InstallDirectory;
			EditConfig(InstallDirectory);
		}
		else if (File.Exists(Path.Combine(DefaultOculusInstallDirectory, "Gorilla Tag.exe")))
		{
			InstallDirectory = DefaultOculusInstallDirectory;
			((Control)textBoxDirectory).Text = InstallDirectory;
			EditConfig(InstallDirectory);
		}
		else
		{
			ShowErrorFindingDirectoryMessage();
		}
		ConfigFix();
		new Thread(LoadRequiredPlugins).Start();
	}

	private void UpdateStatus(string status)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		string formattedText = "Status: " + status;
		((Control)this).Invoke((Delegate)(MethodInvoker)delegate
		{
			((Control)labelStatus).Text = formattedText;
		});
	}

	private string DownloadSite(string URL)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (PermCookie == null)
			{
				PermCookie = new CookieContainer();
			}
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(URL);
			obj.Method = "GET";
			obj.KeepAlive = true;
			obj.CookieContainer = PermCookie;
			obj.ContentType = "application/x-www-form-urlencoded";
			obj.Referer = "";
			obj.UserAgent = "Monke-Mod-Manager";
			obj.Proxy = null;
			StreamReader streamReader = new StreamReader(((HttpWebResponse)obj.GetResponse()).GetResponseStream());
			string result = streamReader.ReadToEnd();
			streamReader.Close();
			return result;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message.Contains("403") ? "Failed to fetch info, GitHub has most likely rate limited you, please check back in 15 - 30 minutes" : "Failed to fetch info, please check your internet connection. If this persists contact \"ngbatz\" on discord via GTMG (https://discord.gg/monkemod)", ((Control)this).Text, (MessageBoxButtons)0, (MessageBoxIcon)16);
			MessageBox.Show("You will still be able to use MMM but you won't be able to install mods.", ((Control)this).Text, (MessageBoxButtons)0, (MessageBoxIcon)64);
			UpdateStatus("Failed to fetch info");
			return null;
		}
	}

	private void LoadReleases()
	{
		JSONNode jSONNode = JSON.Parse(DownloadSite("https://raw.githubusercontent.com/ngbatzyt/MonkeModInfo/master/groupinfo.json?nocache={DateTime.Now:ddMMyyyyHHmmss}"));
		JSONArray asArray = JSON.Parse(DownloadSite($"https://raw.githubusercontent.com/ngbatzyt/MonkeModInfo/master/modinfo.json?nocache={DateTime.Now:ddMMyyyyHHmmss}")).AsArray;
		JSONArray asArray2 = jSONNode.AsArray;
		for (int i = 0; i < asArray.Count; i++)
		{
			JSONNode jSONNode2 = asArray[i];
			ReleaseInfo item = new ReleaseInfo(jSONNode2["name"], jSONNode2["author"], jSONNode2["version"], jSONNode2["group"], jSONNode2["download_url"], jSONNode2["install_location"], jSONNode2["git_path"], jSONNode2["mod"], jSONNode2["dependencies"].AsArray);
			releases.Add(item);
		}
		asArray2.Linq.OrderBy((KeyValuePair<string, JSONNode> x) => x.Value["rank"]);
		for (int num = 0; num < asArray2.Count; num++)
		{
			JSONNode current = asArray2[num];
			if (releases.Any((ReleaseInfo x) => (JSONNode)x.Group == (object)current["name"]))
			{
				groups.Add(current["name"], groups.Count());
			}
		}
		groups.Add("Uncategorized", groups.Count());
		foreach (ReleaseInfo release in releases)
		{
			foreach (string dep in release.Dependencies)
			{
				releases.Where((ReleaseInfo x) => x.Name == dep).FirstOrDefault()?.Dependents.Add(release.Name);
			}
		}
	}

	public void ConfigFix()
	{
		string path = Path.Combine(InstallDirectory, "BepInEx\\config\\BepInEx.cfg");
		if (!File.Exists(path))
		{
			Directory.CreateDirectory(Path.Combine(InstallDirectory, "BepInEx", "config"));
			string text = DownloadSite("https://github.com/NgbatzYT/MonkeModInfo/raw/refs/heads/master/BepInEx.cfg");
			if (text != null)
			{
				File.WriteAllText(path, text);
			}
		}
		string text2 = File.ReadAllText(path);
		if (text2.Contains("HideManagerGameObject = false"))
		{
			string contents = text2.Replace("HideManagerGameObject = false", "HideManagerGameObject = true");
			File.WriteAllText(path, contents);
		}
	}

	private void LoadRequiredPlugins()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			UpdateStatus("Getting latest version info...");
			LoadReleases();
			((Control)this).Invoke((Delegate)(MethodInvoker)delegate
			{
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Expected O, but got Unknown
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a8: Expected O, but got Unknown
				int i;
				for (i = 0; i < groups.Count(); i++)
				{
					string key = groups.First((KeyValuePair<string, int> x) => x.Value == i).Key;
					int value = listViewMods.Groups.Add(new ListViewGroup(key, (HorizontalAlignment)0));
					groups[key] = value;
				}
				foreach (ReleaseInfo release in releases)
				{
					ListViewItem val = new ListViewItem
					{
						Text = release.Name
					};
					if (!string.IsNullOrEmpty(release.Version))
					{
						val.Text = release.Name + " - " + release.Version;
					}
					if (!string.IsNullOrEmpty(release.Tag))
					{
						val.Text = $"{release.Name} - ({release.Tag})";
					}
					val.SubItems.Add(release.Author);
					val.Tag = release;
					if (release.Install)
					{
						listViewMods.Items.Add(val);
					}
					CheckDefaultMod(release, val);
					if (release.Group == null || !groups.ContainsKey(release.Group))
					{
						val.Group = listViewMods.Groups[groups["Uncategorized"]];
					}
					else if (groups.ContainsKey(release.Group))
					{
						int num = groups[release.Group];
						val.Group = listViewMods.Groups[num];
					}
				}
				((Control)tabControlMain).Enabled = true;
				((Control)buttonInstall).Enabled = true;
			});
			UpdateStatus("Release info updated!");
			GetInstalledMods();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Error!");
		}
	}

	private void CheckDefaultMod(ReleaseInfo release, ListViewItem item)
	{
		if (release.Name.Contains("BepInEx"))
		{
			item.Checked = true;
			item.ForeColor = Color.DimGray;
		}
		else
		{
			release.Install = false;
		}
	}

	private void NotFoundHandler()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		while (!flag)
		{
			OpenFileDialog val = new OpenFileDialog();
			try
			{
				((FileDialog)val).FileName = "Gorilla Tag.exe";
				((FileDialog)val).Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
				((FileDialog)val).FilterIndex = 1;
				if ((int)((CommonDialog)val).ShowDialog() == 1)
				{
					string fileName = ((FileDialog)val).FileName;
					if (Path.GetFileName(fileName).Equals("Gorilla Tag.exe"))
					{
						InstallDirectory = Path.GetDirectoryName(fileName);
						((Control)textBoxDirectory).Text = InstallDirectory;
						flag = true;
						EditConfig(InstallDirectory);
					}
					else
					{
						MessageBox.Show("That's not Gorilla Tag, please try again.", "Error!", (MessageBoxButtons)0, (MessageBoxIcon)16);
					}
				}
				else
				{
					Environment.Exit(0);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void ShowErrorFindingDirectoryMessage()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MessageBox.Show("We couldn't find your Gorilla Tag installation, please press \"OK\" and point us to it", "Error", (MessageBoxButtons)0, (MessageBoxIcon)16);
		NotFoundHandler();
		((Form)this).TopMost = true;
	}

	private void EditConfig(string e)
	{
		Settings.Default.InstallDirectory = e;
		((SettingsBase)Settings.Default).Save();
		InstallDirectory = Settings.Default.InstallDirectory;
	}

	private void button2_Click(object sender, EventArgs e)
	{
		Process.Start("https://gorillatagmodding.ngbatzstudios.com/#/");
	}

	private void modsButton_Click(object sender, EventArgs e)
	{
		if (modsDisabled)
		{
			if (File.Exists(Path.Combine(InstallDirectory, "mods.disable")))
			{
				File.Move(Path.Combine(InstallDirectory, "mods.disable"), Path.Combine(InstallDirectory, "winhttp.dll"));
				((Control)modsButton).Text = "Disable Mods";
				((Control)modsButton).BackColor = Color.Transparent;
				modsDisabled = false;
				UpdateStatus("Enabled mods!");
			}
		}
		else if (File.Exists(Path.Combine(InstallDirectory, "winhttp.dll")))
		{
			File.Move(Path.Combine(InstallDirectory, "winhttp.dll"), Path.Combine(InstallDirectory, "mods.disable"));
			((Control)modsButton).Text = "Enable Mods";
			((Control)modsButton).BackColor = Color.IndianRed;
			modsDisabled = true;
			UpdateStatus("Disabled mods!");
		}
	}

	public void GetInstalledMods()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		listView1.Items.Clear();
		installedr.Clear();
		installed.Clear();
		string path = Path.Combine(InstallDirectory, "BepInEx\\plugins");
		if (!Directory.Exists(path))
		{
			return;
		}
		foreach (string item in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).Concat(Directory.GetFiles(path, "*.disable", SearchOption.AllDirectories)))
		{
			ListViewItem val = new ListViewItem();
			if (installed.ContainsKey(Path.GetFileNameWithoutExtension(item)))
			{
				int num = 0;
				foreach (KeyValuePair<string, string> item2 in installed)
				{
					if (item2.Key.Contains(Path.GetFileNameWithoutExtension(item)))
					{
						num++;
					}
				}
				num++;
				if (Path.GetExtension(item) == ".disable")
				{
					val.Text = Path.GetFileNameWithoutExtension(item) + $" {num}" + " - Disabled";
				}
				else
				{
					val.Text = Path.GetFileNameWithoutExtension(item) + " " + num;
				}
			}
			else if (Path.GetExtension(item) == ".disable")
			{
				val.Text = Path.GetFileNameWithoutExtension(item) + " - Disabled";
			}
			else
			{
				val.Text = Path.GetFileNameWithoutExtension(item);
			}
			installed.Add(val.Text, item);
			listView1.Items.Add(val);
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		foreach (KeyValuePair<string, string> item in installed)
		{
			if (installedr.ContainsKey(item.Key) && installedr[item.Key] && File.Exists(item.Value))
			{
				File.Delete(item.Value);
				UpdateStatus("Uninstalled " + item.Key + "...");
			}
		}
		GetInstalledMods();
	}

	private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		foreach (ListViewItem item in listView1.Items)
		{
			ListViewItem val = item;
			if (val == null || string.IsNullOrEmpty(val.Text))
			{
				break;
			}
			installedr.Remove(val.Text);
			installedr.Add(val.Text, val.Checked);
		}
	}

	private void CheckForUpdates()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		try
		{
			UpdateStatus("Checking for updates...");
			if (Convert.ToInt16(DownloadSite("https://raw.githubusercontent.com/NgbatzYT/MonkeModManager/master/update")) <= CurrentVersion)
			{
				return;
			}
			object obj = _003C_003Ec._003C_003E9__46_0;
			if (obj == null)
			{
				MethodInvoker val = delegate
				{
					//IL_000d: Unknown result type (might be due to invalid IL or missing references)
					MessageBox.Show("You have an old version of MMM.", "Update Available", (MessageBoxButtons)0, (MessageBoxIcon)64);
					Process.Start("https://github.com/NgbatzYT/MonkeModManager/releases/latest");
					Environment.Exit(0);
				};
				_003C_003Ec._003C_003E9__46_0 = val;
				obj = (object)val;
			}
			((Control)this).Invoke((Delegate)obj);
		}
		catch
		{
			MessageBox.Show("An error occured while checking for updates, MMM will now close.", "Update Unknown", (MessageBoxButtons)0, (MessageBoxIcon)64);
			Environment.Exit(0);
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		foreach (KeyValuePair<string, string> item in installed)
		{
			if (installedr.ContainsKey(item.Key) && installedr[item.Key] && File.Exists(item.Value))
			{
				string destFileName = Path.ChangeExtension(item.Value, ".disable");
				File.Move(item.Value, destFileName);
				UpdateStatus("Disabled " + item.Key + "...");
			}
		}
		GetInstalledMods();
	}

	private void button4_Click(object sender, EventArgs e)
	{
		foreach (KeyValuePair<string, string> item in installed)
		{
			if (installedr.ContainsKey(item.Key) && installedr[item.Key] && File.Exists(item.Value))
			{
				string destFileName = Path.ChangeExtension(item.Value, ".dll");
				File.Move(item.Value, destFileName);
				UpdateStatus("Enabled " + item.Key + "...");
			}
		}
		GetInstalledMods();
	}

	private void button6_Click(object sender, EventArgs e)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		((Form)new BackupManager()).ShowDialog();
	}

	private void button5_Click(object sender, EventArgs e)
	{
		GetInstalledMods();
	}

	private void button7_Click(object sender, EventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MessageBox.Show("Make sure you trust who made the mod you are installing, if you don't trust them do NOT install it.", "Warning", (MessageBoxButtons)0, (MessageBoxIcon)48);
			OpenFileDialog val = new OpenFileDialog();
			try
			{
				val.Multiselect = false;
				((FileDialog)val).Filter = "Dll files (.dll)|*.dll";
				((FileDialog)val).FilterIndex = 1;
				if ((int)((CommonDialog)val).ShowDialog() == 1)
				{
					string fileName = ((FileDialog)val).FileName;
					if (Path.GetExtension(fileName).Equals(".dll", StringComparison.OrdinalIgnoreCase))
					{
						UpdateStatus("Installing External Mod...");
						File.Copy(Path.GetFullPath(fileName), Path.Combine(InstallDirectory, "BepInEx/plugins", Path.GetFileName(fileName)));
						UpdateStatus("Installed External Mod.");
					}
					else
					{
						MessageBox.Show("Error.", "Error!", (MessageBoxButtons)0, (MessageBoxIcon)16);
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)0, (MessageBoxIcon)16);
		}
	}

	private void button8_Click(object sender, EventArgs e)
	{
		string text = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "AppData\\LocalLow\\Another Axiom\\Gorilla Tag");
		if (Directory.Exists(text))
		{
			Process.Start(text);
		}
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
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_0990: Unknown result type (might be due to invalid IL or missing references)
		//IL_099a: Expected O, but got Unknown
		//IL_0e0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_113f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1149: Expected O, but got Unknown
		//IL_11a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_12cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d6: Expected O, but got Unknown
		//IL_12e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ed: Expected O, but got Unknown
		//IL_1539: Unknown result type (might be due to invalid IL or missing references)
		//IL_1543: Expected O, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
		textBoxDirectory = new TextBox();
		buttonFolderBrowser = new Button();
		label1 = new Label();
		buttonInstall = new Button();
		labelStatus = new Label();
		contextMenuStripMain = new ContextMenuStrip(components);
		viewInfoToolStripMenuItem = new ToolStripMenuItem();
		buttonModInfo = new Button();
		modsButton = new Button();
		columnHeaderName = new ColumnHeader();
		columnHeaderAuthor = new ColumnHeader();
		Utilities = new TabPage();
		button7 = new Button();
		button6 = new Button();
		button2 = new Button();
		labelVersion = new Label();
		pictureBox1 = new PictureBox();
		buttonDiscordLink = new Button();
		groupBox1 = new GroupBox();
		buttonBepInEx = new Button();
		buttonOpenConfig = new Button();
		buttonOpenGameFolder = new Button();
		labelOpen = new Label();
		buttonUninstallAll = new Button();
		Updates = new TabPage();
		button5 = new Button();
		button4 = new Button();
		button3 = new Button();
		button1 = new Button();
		listView1 = new ListView();
		columnHeader1 = new ColumnHeader();
		Installed = new TabPage();
		listViewMods = new ListView();
		columnHeader3 = new ColumnHeader();
		columnHeader4 = new ColumnHeader();
		tabControlMain = new TabControl();
		button8 = new Button();
		((Control)contextMenuStripMain).SuspendLayout();
		((Control)Utilities).SuspendLayout();
		((ISupportInitialize)pictureBox1).BeginInit();
		((Control)groupBox1).SuspendLayout();
		((Control)Updates).SuspendLayout();
		((Control)Installed).SuspendLayout();
		((Control)tabControlMain).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)textBoxDirectory).Anchor = (AnchorStyles)13;
		((Control)textBoxDirectory).Enabled = false;
		((Control)textBoxDirectory).Location = new Point(10, 25);
		((Control)textBoxDirectory).Name = "textBoxDirectory";
		((Control)textBoxDirectory).Size = new Size(508, 20);
		((Control)textBoxDirectory).TabIndex = 0;
		((Control)buttonFolderBrowser).Anchor = (AnchorStyles)9;
		((Control)buttonFolderBrowser).Location = new Point(524, 25);
		((Control)buttonFolderBrowser).Name = "buttonFolderBrowser";
		((Control)buttonFolderBrowser).Size = new Size(26, 23);
		((Control)buttonFolderBrowser).TabIndex = 1;
		((Control)buttonFolderBrowser).Text = "..";
		((ButtonBase)buttonFolderBrowser).UseVisualStyleBackColor = true;
		((Control)buttonFolderBrowser).Click += buttonFolderBrowser_Click;
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(9, 9);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(118, 13);
		((Control)label1).TabIndex = 2;
		((Control)label1).Text = "Gorilla Tag Folder Path:";
		((Control)buttonInstall).Anchor = (AnchorStyles)10;
		((Control)buttonInstall).Enabled = false;
		((Control)buttonInstall).Location = new Point(440, 341);
		((Control)buttonInstall).Name = "buttonInstall";
		((Control)buttonInstall).Size = new Size(112, 23);
		((Control)buttonInstall).TabIndex = 4;
		((Control)buttonInstall).Text = "Install / Update";
		((ButtonBase)buttonInstall).UseVisualStyleBackColor = true;
		((Control)buttonInstall).Click += buttonInstall_Click;
		((Control)labelStatus).Anchor = (AnchorStyles)6;
		((Control)labelStatus).AutoSize = true;
		((Control)labelStatus).Location = new Point(7, 346);
		((Control)labelStatus).Name = "labelStatus";
		((Control)labelStatus).Size = new Size(61, 13);
		((Control)labelStatus).TabIndex = 5;
		((Control)labelStatus).Text = "Status: Null";
		((ToolStrip)contextMenuStripMain).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[1] { (ToolStripItem)viewInfoToolStripMenuItem });
		((Control)contextMenuStripMain).Name = "contextMenuStripMain";
		((Control)contextMenuStripMain).Size = new Size(124, 26);
		((ToolStripItem)viewInfoToolStripMenuItem).Name = "viewInfoToolStripMenuItem";
		((ToolStripItem)viewInfoToolStripMenuItem).Size = new Size(123, 22);
		((ToolStripItem)viewInfoToolStripMenuItem).Text = "View Info";
		((ToolStripItem)viewInfoToolStripMenuItem).Click += viewInfoToolStripMenuItem_Click;
		((Control)buttonModInfo).Anchor = (AnchorStyles)10;
		((Control)buttonModInfo).Enabled = false;
		((Control)buttonModInfo).Location = new Point(322, 341);
		((Control)buttonModInfo).Name = "buttonModInfo";
		((Control)buttonModInfo).Size = new Size(112, 23);
		((Control)buttonModInfo).TabIndex = 9;
		((Control)buttonModInfo).Text = "View Mod Info";
		((ButtonBase)buttonModInfo).UseVisualStyleBackColor = true;
		((Control)buttonModInfo).Click += buttonModInfo_Click;
		((Control)modsButton).Anchor = (AnchorStyles)10;
		((Control)modsButton).Location = new Point(204, 341);
		((Control)modsButton).Name = "modsButton";
		((Control)modsButton).Size = new Size(112, 23);
		((Control)modsButton).TabIndex = 10;
		((Control)modsButton).Text = "Disable Mods";
		((ButtonBase)modsButton).UseVisualStyleBackColor = true;
		((Control)modsButton).Click += modsButton_Click;
		columnHeaderName.DisplayIndex = 0;
		columnHeaderName.Text = "Name";
		columnHeaderName.Width = 321;
		columnHeaderAuthor.DisplayIndex = 1;
		columnHeaderAuthor.Text = "Author";
		((Control)Utilities).Controls.Add((Control)(object)button8);
		((Control)Utilities).Controls.Add((Control)(object)button7);
		((Control)Utilities).Controls.Add((Control)(object)button6);
		((Control)Utilities).Controls.Add((Control)(object)button2);
		((Control)Utilities).Controls.Add((Control)(object)labelVersion);
		((Control)Utilities).Controls.Add((Control)(object)pictureBox1);
		((Control)Utilities).Controls.Add((Control)(object)buttonDiscordLink);
		((Control)Utilities).Controls.Add((Control)(object)groupBox1);
		((Control)Utilities).Controls.Add((Control)(object)buttonUninstallAll);
		Utilities.Location = new Point(4, 22);
		((Control)Utilities).Name = "Utilities";
		((Control)Utilities).Size = new Size(536, 256);
		Utilities.TabIndex = 1;
		((Control)Utilities).Text = "Utilities";
		Utilities.UseVisualStyleBackColor = true;
		((Control)button7).Location = new Point(14, 101);
		((Control)button7).Name = "button7";
		((Control)button7).Size = new Size(134, 23);
		((Control)button7).TabIndex = 21;
		((Control)button7).Text = "Install External Mod";
		((ButtonBase)button7).UseVisualStyleBackColor = true;
		((Control)button7).Click += button7_Click;
		((Control)button6).Location = new Point(14, 72);
		((Control)button6).Name = "button6";
		((Control)button6).Size = new Size(134, 23);
		((Control)button6).TabIndex = 20;
		((Control)button6).Text = "Backup Manager";
		((ButtonBase)button6).UseVisualStyleBackColor = true;
		((Control)button6).Click += button6_Click;
		((Control)button2).Location = new Point(379, 181);
		((Control)button2).Name = "button2";
		((Control)button2).Size = new Size(134, 23);
		((Control)button2).TabIndex = 19;
		((Control)button2).Text = "Check out the guides!";
		((ButtonBase)button2).UseVisualStyleBackColor = true;
		((Control)button2).Click += button2_Click;
		((Control)labelVersion).Anchor = (AnchorStyles)3;
		((Control)labelVersion).AutoSize = true;
		((Control)labelVersion).Location = new Point(188, 209);
		((Control)labelVersion).Name = "labelVersion";
		((Control)labelVersion).Size = new Size(109, 13);
		((Control)labelVersion).TabIndex = 11;
		((Control)labelVersion).Text = "Monke Mod Manager";
		labelVersion.TextAlign = (ContentAlignment)512;
		labelVersion.UseMnemonic = false;
		pictureBox1.Image = (Image)componentResourceManager.GetObject("pictureBox1.Image");
		((Control)pictureBox1).Location = new Point(170, 43);
		((Control)pictureBox1).Name = "pictureBox1";
		((Control)pictureBox1).Size = new Size(186, 163);
		pictureBox1.SizeMode = (PictureBoxSizeMode)1;
		pictureBox1.TabIndex = 10;
		pictureBox1.TabStop = false;
		((Control)buttonDiscordLink).Location = new Point(379, 210);
		((Control)buttonDiscordLink).Name = "buttonDiscordLink";
		((Control)buttonDiscordLink).Size = new Size(134, 23);
		((Control)buttonDiscordLink).TabIndex = 8;
		((Control)buttonDiscordLink).Text = "Join the Discord!";
		((ButtonBase)buttonDiscordLink).UseVisualStyleBackColor = true;
		((Control)buttonDiscordLink).Click += buttonDiscordLink_Click;
		((Control)groupBox1).Controls.Add((Control)(object)buttonBepInEx);
		((Control)groupBox1).Controls.Add((Control)(object)buttonOpenConfig);
		((Control)groupBox1).Controls.Add((Control)(object)buttonOpenGameFolder);
		((Control)groupBox1).Controls.Add((Control)(object)labelOpen);
		((Control)groupBox1).Location = new Point(373, 16);
		((Control)groupBox1).Name = "groupBox1";
		((Control)groupBox1).Size = new Size(146, 159);
		((Control)groupBox1).TabIndex = 7;
		groupBox1.TabStop = false;
		((Control)buttonBepInEx).Location = new Point(6, 96);
		((Control)buttonBepInEx).Name = "buttonBepInEx";
		((Control)buttonBepInEx).Size = new Size(134, 23);
		((Control)buttonBepInEx).TabIndex = 5;
		((Control)buttonBepInEx).Text = "Mods/Plugins Folder";
		((ButtonBase)buttonBepInEx).UseVisualStyleBackColor = true;
		((Control)buttonBepInEx).Click += buttonOpenBepInExFolder_Click;
		((Control)buttonOpenConfig).Location = new Point(6, 67);
		((Control)buttonOpenConfig).Name = "buttonOpenConfig";
		((Control)buttonOpenConfig).Size = new Size(134, 23);
		((Control)buttonOpenConfig).TabIndex = 5;
		((Control)buttonOpenConfig).Text = "Config Folder";
		((ButtonBase)buttonOpenConfig).UseVisualStyleBackColor = true;
		((Control)buttonOpenConfig).Click += buttonOpenConfigFolder_Click;
		((Control)buttonOpenGameFolder).Location = new Point(6, 38);
		((Control)buttonOpenGameFolder).Name = "buttonOpenGameFolder";
		((Control)buttonOpenGameFolder).Size = new Size(134, 23);
		((Control)buttonOpenGameFolder).TabIndex = 5;
		((Control)buttonOpenGameFolder).Text = "Game Folder";
		((ButtonBase)buttonOpenGameFolder).UseVisualStyleBackColor = true;
		((Control)buttonOpenGameFolder).Click += buttonOpenGameFolder_Click;
		((Control)labelOpen).AutoSize = true;
		((Control)labelOpen).Location = new Point(23, 15);
		((Control)labelOpen).Name = "labelOpen";
		((Control)labelOpen).Size = new Size(88, 13);
		((Control)labelOpen).TabIndex = 6;
		((Control)labelOpen).Text = "Important Folders";
		((Control)buttonUninstallAll).Location = new Point(14, 43);
		((Control)buttonUninstallAll).Name = "buttonUninstallAll";
		((Control)buttonUninstallAll).Size = new Size(134, 23);
		((Control)buttonUninstallAll).TabIndex = 0;
		((Control)buttonUninstallAll).Text = "Uninstall All Mods";
		((ButtonBase)buttonUninstallAll).UseVisualStyleBackColor = true;
		((Control)buttonUninstallAll).Click += buttonUninstallAll_Click;
		((Control)Updates).Controls.Add((Control)(object)button5);
		((Control)Updates).Controls.Add((Control)(object)button4);
		((Control)Updates).Controls.Add((Control)(object)button3);
		((Control)Updates).Controls.Add((Control)(object)button1);
		((Control)Updates).Controls.Add((Control)(object)listView1);
		Updates.Location = new Point(4, 22);
		((Control)Updates).Name = "Updates";
		((Control)Updates).Padding = new Padding(3);
		((Control)Updates).Size = new Size(536, 256);
		Updates.TabIndex = 8;
		((Control)Updates).Text = "Installed";
		Updates.UseVisualStyleBackColor = true;
		((Control)button5).Anchor = (AnchorStyles)10;
		((Control)button5).Location = new Point(402, 227);
		((Control)button5).Name = "button5";
		((Control)button5).Size = new Size(126, 23);
		((Control)button5).TabIndex = 14;
		((Control)button5).Text = "Refresh";
		((ButtonBase)button5).UseVisualStyleBackColor = true;
		((Control)button5).Click += button5_Click;
		((Control)button4).Anchor = (AnchorStyles)10;
		((Control)button4).Location = new Point(138, 227);
		((Control)button4).Name = "button4";
		((Control)button4).Size = new Size(126, 23);
		((Control)button4).TabIndex = 13;
		((Control)button4).Text = "Enable Selected";
		((ButtonBase)button4).UseVisualStyleBackColor = true;
		((Control)button4).Click += button4_Click;
		((Control)button3).Anchor = (AnchorStyles)10;
		((Control)button3).Location = new Point(6, 227);
		((Control)button3).Name = "button3";
		((Control)button3).Size = new Size(126, 23);
		((Control)button3).TabIndex = 12;
		((Control)button3).Text = "Disable Selected";
		((ButtonBase)button3).UseVisualStyleBackColor = true;
		((Control)button3).Click += button3_Click;
		((Control)button1).Anchor = (AnchorStyles)10;
		((Control)button1).Location = new Point(270, 227);
		((Control)button1).Name = "button1";
		((Control)button1).Size = new Size(126, 23);
		((Control)button1).TabIndex = 11;
		((Control)button1).Text = "Uninstall Selected";
		((ButtonBase)button1).UseVisualStyleBackColor = true;
		((Control)button1).Click += button1_Click;
		((Control)listView1).Anchor = (AnchorStyles)15;
		listView1.CheckBoxes = true;
		listView1.Columns.AddRange((ColumnHeader[])(object)new ColumnHeader[1] { columnHeader1 });
		listView1.FullRowSelect = true;
		listView1.HideSelection = false;
		((Control)listView1).Location = new Point(6, 6);
		((Control)listView1).Name = "listView1";
		((Control)listView1).Size = new Size(524, 215);
		((Control)listView1).TabIndex = 1;
		listView1.UseCompatibleStateImageBehavior = false;
		listView1.View = (View)1;
		listView1.ItemChecked += new ItemCheckedEventHandler(listView1_ItemChecked);
		columnHeader1.Text = "Name";
		columnHeader1.Width = 358;
		((Control)Installed).Controls.Add((Control)(object)listViewMods);
		Installed.Location = new Point(4, 22);
		((Control)Installed).Name = "Installed";
		((Control)Installed).Padding = new Padding(3);
		((Control)Installed).Size = new Size(536, 256);
		Installed.TabIndex = 0;
		((Control)Installed).Text = "Plugins";
		Installed.UseVisualStyleBackColor = true;
		((Control)listViewMods).Anchor = (AnchorStyles)15;
		listViewMods.CheckBoxes = true;
		listViewMods.Columns.AddRange((ColumnHeader[])(object)new ColumnHeader[2] { columnHeader3, columnHeader4 });
		((Control)listViewMods).ContextMenuStrip = contextMenuStripMain;
		listViewMods.FullRowSelect = true;
		listViewMods.HideSelection = false;
		((Control)listViewMods).Location = new Point(6, 6);
		((Control)listViewMods).Name = "listViewMods";
		((Control)listViewMods).Size = new Size(524, 244);
		((Control)listViewMods).TabIndex = 2;
		listViewMods.UseCompatibleStateImageBehavior = false;
		listViewMods.View = (View)1;
		listViewMods.ItemChecked += new ItemCheckedEventHandler(listViewMods_ItemChecked);
		listViewMods.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(listViewMods_ItemSelectionChanged);
		((Control)listViewMods).DoubleClick += listViewMods_DoubleClick;
		columnHeader3.Text = "Name";
		columnHeader3.Width = 321;
		columnHeader4.Text = "Author";
		columnHeader4.Width = 136;
		((Control)tabControlMain).Anchor = (AnchorStyles)15;
		((Control)tabControlMain).Controls.Add((Control)(object)Installed);
		((Control)tabControlMain).Controls.Add((Control)(object)Updates);
		((Control)tabControlMain).Controls.Add((Control)(object)Utilities);
		((Control)tabControlMain).Location = new Point(10, 53);
		((Control)tabControlMain).Name = "tabControlMain";
		tabControlMain.SelectedIndex = 0;
		((Control)tabControlMain).Size = new Size(544, 282);
		((Control)tabControlMain).TabIndex = 8;
		((Control)button8).Location = new Point(379, 141);
		((Control)button8).Name = "button8";
		((Control)button8).Size = new Size(134, 23);
		((Control)button8).TabIndex = 7;
		((Control)button8).Text = "Log Folder";
		((ButtonBase)button8).UseVisualStyleBackColor = true;
		((Control)button8).Click += button8_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = SystemColors.Control;
		((Form)this).ClientSize = new Size(566, 376);
		((Control)this).Controls.Add((Control)(object)modsButton);
		((Control)this).Controls.Add((Control)(object)buttonModInfo);
		((Control)this).Controls.Add((Control)(object)labelStatus);
		((Control)this).Controls.Add((Control)(object)buttonInstall);
		((Control)this).Controls.Add((Control)(object)label1);
		((Control)this).Controls.Add((Control)(object)buttonFolderBrowser);
		((Control)this).Controls.Add((Control)(object)textBoxDirectory);
		((Control)this).Controls.Add((Control)(object)tabControlMain);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).Location = new Point(15, 15);
		((Control)this).Name = "Form1";
		((Form)this).StartPosition = (FormStartPosition)1;
		((Control)this).Text = "Monke Mod Manager";
		((Form)this).Load += Form1_Load;
		((Control)contextMenuStripMain).ResumeLayout(false);
		((Control)Utilities).ResumeLayout(false);
		((Control)Utilities).PerformLayout();
		((ISupportInitialize)pictureBox1).EndInit();
		((Control)groupBox1).ResumeLayout(false);
		((Control)groupBox1).PerformLayout();
		((Control)Updates).ResumeLayout(false);
		((Control)Installed).ResumeLayout(false);
		((Control)tabControlMain).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
