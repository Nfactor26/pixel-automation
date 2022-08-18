﻿namespace Pixel.Scripting.Common.CSharp
{
    internal static class Configuration
    {
        public const string RoslynVersion = "4.2.0.0";
        public const string RoslynPublicKeyToken = "31bf3856ad364e35";

        public readonly static string RoslynFeatures = GetRoslynAssemblyFullName("Microsoft.CodeAnalysis.Features");
        public readonly static string RoslynCSharpFeatures = GetRoslynAssemblyFullName("Microsoft.CodeAnalysis.CSharp.Features");      
        
        public readonly static string RoslynWorkspaces = GetRoslynAssemblyFullName("Microsoft.CodeAnalysis.Workspaces");
        public readonly static string RoslynCSharpWorkspaces = GetRoslynAssemblyFullName("Microsoft.CodeAnalysis.CSharp.Workspaces");

        private static string GetRoslynAssemblyFullName(string name)
        {
            return $"{name}, Version={RoslynVersion}, Culture=neutral, PublicKeyToken={RoslynPublicKeyToken}";
        }
    }
}
