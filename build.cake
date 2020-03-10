#addin "nuget:?package=Cake.Incubator"

#load BuildScripts/Arguments.cake
#load BuildScripts/Projects.cake
#load BuildScripts/DirectoryStructure.cake
#load BuildScripts/Core.cake
#load BuildScripts/Components.cake
#load BuildScripts/Editors.cake
#load BuildScripts/Scrappers.cake
#load BuildScripts/Windows.cake

Task("Clean").Does(() =>
{
	DeleteFiles(GetFiles($"{buildDirectory}/*.*"));
});

Task("Restore-NuGet-Packages")    
    .Does(() => {
        NuGetRestore(solution);
});

Task("Build")
.IsDependentOn("Clean")
.IsDependentOn("Restore-NuGet-Packages")
.Does(() =>
{     
	DirectoryPath buildDir = MakeAbsolute(buildDirectory);   
    var settings = new MSBuildSettings().SetConfiguration(configuration);
    MSBuild(solution, settings);  
});


Task("PostBuild")
.IsDependentOn("Core.PostBuild")
.IsDependentOn("Components.PostBuild")
.IsDependentOn("Scrappers.PostBuild")
.IsDependentOn("Editors.PostBuild")
.IsDependentOn("Editors.Scripting.PostBuild")
.IsDependentOn("Windows.Native.PostBuild")
.Does(() =>
{	
	if(!DirectoryExists(applicationRepository))
	{
		CreateDirectory(applicationRepository);
		Information("Created directory ApplicationRepository");
	}

	if(!DirectoryExists(automationsDirectory))
	{
		CreateDirectory(automationsDirectory);
		Information("Created directory Automations");
	}
	
}).Finally(() =>
{
	Information("Completed PostBuild");
});

Task("Explore").Does(() =>
{
	var webDriverProject = ParseProject("./Pixel.Automation.Web.Selenium.Components//Pixel.Automation.Web.Selenium.Components.csproj");
	Information($"Assembly Name : {webDriverProject.AssemblyName}");
	Information($"Output Path : {webDriverProject.OutputPath}");
	
	foreach(var reference in webDriverProject.References)
	{
		Information($"Name : {reference.Name} | Include : {reference.Include} | Should Copy : {reference.Private}");
	}	
});


Task("Default")
.IsDependentOn("Build")
.IsDependentOn("PostBuild")
.Does(() =>
{
  Information("Starting build!"); 
});


RunTarget("Default");