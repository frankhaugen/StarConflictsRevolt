#!/usr/bin/env dotnet
#:property TargetFramework=net10.0
#:property PublishAot=false

using System.Xml.Linq;

var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
var slnxPath = ResolveSlnxPath(args, cwd);
if (slnxPath is null)
	return; // No .slnx found — exit gracefully

var rootDir = new DirectoryInfo(Path.GetDirectoryName(slnxPath)!);

var doc = LoadOrCreateSolution(slnxPath);
var solution = doc.Root ?? throw new InvalidOperationException("Missing <Solution> root.");

// 1. Deployment folder
var deploymentDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "Deployment"));
var desiredDeployment = deploymentDir.Exists
	? deploymentDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
		.Select(f => ToRelativePath(f.FullName, rootDir))
		.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
		.ToArray()
	: [];
var deploymentChanged = UpdateFolder(solution, "/Deployment/", desiredDeployment, "Deployment");

// 2. Docs — one Folder per subdirectory, declared sequentially (parent before child)
var docsDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "docs"));
var docsChanged = UpdateDocsFolders(solution, docsDir, rootDir);

// 3. Solution Items - root files, skip *.slnx, *.user, pull_request_template.md
var rootFiles = rootDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
	.Where(f => !f.Name.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)
			 && !f.Name.EndsWith(".user", StringComparison.OrdinalIgnoreCase)
			 && !string.Equals(f.Name, "pull_request_template.md", StringComparison.OrdinalIgnoreCase))
	.Select(f => ToRelativePath(f.FullName, rootDir))
	.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
	.ToArray();
var solutionItemsChanged = UpdateFolder(solution, "/Solution Items/", rootFiles, "Solution Items");

// Reorder: Projects, Deployment, Solution Items, Docs (improves git diffing — stable sections first, volatile docs last)
var reordered = ReorderSolution(solution);
if (!docsChanged && !deploymentChanged && !solutionItemsChanged && !reordered)
	return;

// Write back without reformatting everything
var newText = doc.ToString(SaveOptions.DisableFormatting);
var oldText = File.Exists(slnxPath) ? File.ReadAllText(slnxPath) : "";

if (!string.Equals(oldText, newText, StringComparison.Ordinal))
	File.WriteAllText(slnxPath, newText);

Console.WriteLine($"[generate-slnx] Updated {slnxPath}");

static bool UpdateDocsFolders(XElement solution, DirectoryInfo docsDir, DirectoryInfo rootDir)
{
	if (!docsDir.Exists)
	{
		var existing = solution.Elements("Folder")
			.Where(f => IsDocsFolder((string?)f.Attribute("Name")))
			.ToList();
		if (existing.Count == 0) return false;
		foreach (var f in existing) f.Remove();
		return true;
	}

	var filePaths = docsDir.EnumerateFiles("*.md", SearchOption.AllDirectories)
		.Select(f => ToRelativePath(f.FullName, rootDir))
		.ToList();

	var dirToFiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
	foreach (var path in filePaths)
	{
		var dir = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "docs";
		if (!dirToFiles.TryGetValue(dir, out var list))
			dirToFiles[dir] = list = [];
		list.Add(path);
	}

	// Build folder names in sequential order: parent before child (sort by path)
	var dirs = dirToFiles.Keys.OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToList();
	var desired = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
	foreach (var dir in dirs)
	{
		var slnxName = dir == "docs" ? "/Docs/" : "/Docs/" + dir.Replace("docs/", "", StringComparison.OrdinalIgnoreCase) + "/";
		desired[slnxName] = dirToFiles[dir].OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	var existingFolders = solution.Elements("Folder")
		.Where(f => IsDocsFolder((string?)f.Attribute("Name")))
		.ToDictionary(f => (string)f.Attribute("Name")!, f => f, StringComparer.OrdinalIgnoreCase);

	var changed = false;
	var toRemove = existingFolders.Keys.Except(desired.Keys, StringComparer.OrdinalIgnoreCase).ToList();
	foreach (var name in toRemove)
	{
		existingFolders[name].Remove();
		changed = true;
	}

	XElement? lastDocsFolder = null;
	foreach (var (slnxName, paths) in desired.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
	{
		var folder = solution.Elements("Folder")
			.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), slnxName, StringComparison.OrdinalIgnoreCase));

		if (folder is null)
		{
			folder = new XElement("Folder", new XAttribute("Name", slnxName));
			var insertAfter = lastDocsFolder ?? solution.Elements("Folder")
				.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), "/Solution Items/", StringComparison.OrdinalIgnoreCase));
			if (insertAfter is not null)
				insertAfter.AddAfterSelf(Environment.NewLine + "  ", folder);
			else
				solution.Add(Environment.NewLine + "  ", folder, Environment.NewLine);
			changed = true;
		}
		lastDocsFolder = folder;

		var existingPaths = folder.Elements("File")
			.Select(e => (string?)e.Attribute("Path"))
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Select(p => p!)
			.ToArray();

		if (!existingPaths.SequenceEqual(paths, StringComparer.OrdinalIgnoreCase))
		{
			folder.Nodes().Remove();
			folder.Add(Environment.NewLine);
			foreach (var p in paths)
				folder.Add("    ", new XElement("File", new XAttribute("Path", p)), Environment.NewLine);
			folder.Add("  ");
			changed = true;
		}
	}

	return changed;
}

static bool IsDocsFolder(string? name)
{
	if (string.IsNullOrEmpty(name)) return false;
	return string.Equals(name, "/Docs/", StringComparison.OrdinalIgnoreCase)
		|| string.Equals(name, "Docs", StringComparison.OrdinalIgnoreCase)
		|| (name.StartsWith("/Docs/", StringComparison.OrdinalIgnoreCase) && name.Length > 6);
}

static bool UpdateFolder(XElement solution, string folderName, string[] desiredPaths, string altName)
{
	var folder = solution.Elements("Folder")
		.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), folderName, StringComparison.OrdinalIgnoreCase)
						  || string.Equals((string?)f.Attribute("Name"), altName, StringComparison.OrdinalIgnoreCase));

	if (folder is null)
	{
		folder = new XElement("Folder", new XAttribute("Name", folderName));
		var solutionItems = solution.Elements("Folder")
			.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), "/Solution Items/", StringComparison.OrdinalIgnoreCase));
		if (solutionItems is not null) solutionItems.AddBeforeSelf(Environment.NewLine + "  ", folder);
		else solution.Add(Environment.NewLine + "  ", folder, Environment.NewLine);
	}

	var existingPaths = folder.Elements("File")
		.Select(e => (string?)e.Attribute("Path"))
		.Where(p => !string.IsNullOrWhiteSpace(p))
		.Select(p => p!)
		.ToArray();

	if (existingPaths.SequenceEqual(desiredPaths, StringComparer.OrdinalIgnoreCase))
		return false;

	folder.Nodes().Remove();
	folder.Add(Environment.NewLine);
	foreach (var p in desiredPaths)
		folder.Add("    ", new XElement("File", new XAttribute("Path", p)), Environment.NewLine);
	folder.Add("  ");
	return true;
}

static bool IsManagedFolder(XElement folder)
{
	var name = (string?)folder.Attribute("Name");
	if (string.IsNullOrEmpty(name)) return false;
	var cmp = StringComparison.OrdinalIgnoreCase;
	return string.Equals(name, "/Deployment/", cmp) || string.Equals(name, "Deployment", cmp)
		|| string.Equals(name, "/Solution Items/", cmp) || string.Equals(name, "Solution Items", cmp)
		|| IsDocsFolder(name);
}

static bool ReorderSolution(XElement solution)
{
	var projects = solution.Elements("Project").ToList();
	var allFolders = solution.Elements("Folder").ToList();
	var unmanagedFolders = allFolders.Where(f => !IsManagedFolder(f)).ToList();
	var deployment = allFolders.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), "/Deployment/", StringComparison.OrdinalIgnoreCase));
	var solutionItems = allFolders.FirstOrDefault(f => string.Equals((string?)f.Attribute("Name"), "/Solution Items/", StringComparison.OrdinalIgnoreCase));
	var docsFolders = allFolders.Where(f => IsDocsFolder((string?)f.Attribute("Name")))
		.OrderBy(f => (string?)f.Attribute("Name"), StringComparer.OrdinalIgnoreCase)
		.ToList();

	// Order: Projects, unmanaged folders (unchanged), Deployment, Solution Items, Docs (sequential, last — changes often)
	var elements = projects.Cast<XElement>()
		.Concat(unmanagedFolders)
		.Concat(new[] { deployment, solutionItems }.Where(n => n is not null).Cast<XElement>())
		.Concat(docsFolders)
		.ToList();

	var currentOrder = solution.Elements().Select(e => e.Name.LocalName + (string?)e.Attribute("Path") + (string?)e.Attribute("Name")).ToList();
	var desiredOrder = elements.Select(e => e.Name.LocalName + (string?)e.Attribute("Path") + (string?)e.Attribute("Name")).ToList();
	if (currentOrder.SequenceEqual(desiredOrder))
		return false;

	solution.RemoveNodes();
	solution.Add(Environment.NewLine);
	foreach (var el in elements)
		solution.Add("  ", el, Environment.NewLine);
	return true;
}

static XDocument LoadOrCreateSolution(string path)
{
	if (!File.Exists(path))
		return new XDocument(new XElement("Solution", Environment.NewLine));

	using var fs = File.OpenRead(path);
	return XDocument.Load(fs, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
}

static string? ResolveSlnxPath(string[] args, DirectoryInfo rootDir)
{
	if (args.Length > 0)
	{
		var p = args[0];
		if (Path.IsPathRooted(p) ? File.Exists(p) : File.Exists(Path.Combine(rootDir.FullName, p)))
			return Path.IsPathRooted(p) ? p : Path.GetFullPath(Path.Combine(rootDir.FullName, p));
	}
	var slnxFiles = rootDir.EnumerateFiles("*.slnx", SearchOption.TopDirectoryOnly)
		.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
		.ToList();
	return slnxFiles.Count > 0 ? slnxFiles[0].FullName : null;
}

static string ToRelativePath(string fullPath, DirectoryInfo root)
{
	var rootPath = root.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
		.Replace('\\', '/');

	var path = fullPath.Replace('\\', '/');

	if (path.StartsWith(rootPath + "/", StringComparison.OrdinalIgnoreCase))
		path = path[(rootPath.Length + 1)..];

	return path;
}
