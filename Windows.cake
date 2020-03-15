#load Projects.cake
#load DirectoryStructure.cake

Task("Windows.Clean").Does(()=>
{
	if(!DirectoryExists(windowsDirectory))
	{
		CreateDirectory(windowsDirectory);
		Information("Created directory Windows");
	}
	CleanDirectory(windowsDirectory);
});

Task("Windows.Build").Does(()=>
{
	
});

Task("Copy.Vanara.Dependencies").Does(() =>
{
	var windowsNativeOutputDirectory = Directory("./Pixel.Automation.Native.Windows") + projectOutputDirectory;	
	CopyFiles(GetFiles($"{windowsNativeOutputDirectory}/Vanara.*"), windowsNativeOutputDirectory);
});



 Task("Windows.PostBuild")
.IsDependentOn("Copy.Vanara.Dependencies")
.Does(()=>
{
	var windowsNativeProjects = ProjectStructure.GetWindowsNativeProjects().ToList();
	foreach(var project in scriptEditorProjects)
	{				
		var projectName = project.GetFilenameWithoutExtension();
		var projectDirectory = Directory($"./{projectName.ToString()}") + projectOutputDirectory;
		CopyFiles(GetFiles($"{projectDirectory}/{projectName.ToString()}.*"), editorsDirectory);
	}	
	
});

