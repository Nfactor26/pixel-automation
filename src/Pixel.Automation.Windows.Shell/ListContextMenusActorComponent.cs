using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Vanara.PInvoke;
using static Vanara.PInvoke.Shell32;
using static Vanara.PInvoke.User32;

namespace Pixel.Automation.Windows.Shell;

[DataContract]
[Serializable]
[ToolBoxItem("List Context Menus", "Windows Shell", iconSource: null, description: "List the context menus for a file or folder", tags: ["Context", "Menu"])]
public class ListcontextMenusActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<ListcontextMenusActorComponent>();
    const uint idFirst = 1, idLast = 0x7FFF;

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };
    
    public ListcontextMenusActorComponent() : base("List Context Menus", "ListContextMenus")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
      
        EnumerateAndPrintContextMenuItems(path);    
    }
   
    void EnumerateAndPrintContextMenuItems(string path)
    {
        using var pidl = ILCreateFromPath(path);
        if (pidl.IsNull || pidl.IsInvalid)
        {
            throw new InvalidOperationException($"Failed to get PIDL for path: {path}");
        }

        var hr = SHBindToParent(pidl, typeof(IShellFolder).GUID, out object parentObject, out nint childPidl);
        hr.ThrowIfFailed();

        var parentFolder = (IShellFolder)parentObject;
        parentFolder.GetUIObjectOf(HWND.NULL, 1U, new nint[] { childPidl }, typeof(IContextMenu).GUID, IntPtr.Zero, out object icmObj).ThrowIfFailed();

        var icm = (IContextMenu)icmObj;
        var icm3 = icm as Shell32.IContextMenu3;
        var icm2 = icm3 is null ? icm as Shell32.IContextMenu2 : null;

        using var hMenu = CreatePopupMenu();

        icm.QueryContextMenu(hMenu, 0, idFirst, idLast, CMF.CMF_NORMAL | CMF.CMF_EXTENDEDVERBS).ThrowIfFailed();

        logger.Information($"Context menu items for {path}:");

        EnumerateMenuRecursive(icm, icm2, icm3, hMenu, idFirst, string.Empty);

    }

    void EnumerateMenuRecursive(Shell32.IContextMenu icm, Shell32.IContextMenu2? icm2, Shell32.IContextMenu3? icm3, HMENU hMenu, uint idFirst, string prefix)
    {
        //var msg = new MSG { message = (uint)WindowMessage.WM_INITMENUPOPUP, wParam = (nint)hMenu };
        //icm3?.HandleMenuMsg2(msg.message, msg.wParam, msg.lParam, out _);
        //icm2?.HandleMenuMsg(msg.message, msg.wParam, msg.lParam);

        int itemCount = GetMenuItemCount(hMenu);
        for (int i = 0; i < itemCount; i++)
        {
            var sub = GetSubMenu(hMenu, i);
            if (!sub.IsNull)
            {
                icm3?.HandleMenuMsg2((uint)WindowMessage.WM_INITMENUPOPUP, sub.DangerousGetHandle(), (IntPtr)i, out _);
                icm2?.HandleMenuMsg((uint)WindowMessage.WM_INITMENUPOPUP, sub.DangerousGetHandle(), (IntPtr)i);

                var title = GetMenuItemText(hMenu, i);
                logger.Information($"{prefix}{(string.IsNullOrEmpty(title) ? "[submenu]" : title)}");
                EnumerateMenuRecursive(icm, icm2, icm3, sub, idFirst, prefix + "  ");
                continue;
            }

            uint itemID = GetMenuItemID(hMenu, i);
            if (itemID == 0xFFFFFFFF)
            {
                continue; // Separator or invalid
            }

            string text = GetMenuItemText(hMenu, i);

            logger.Information($"{prefix}{text} [id={itemID}, cmd={itemID - idFirst}]");
        }
    }

    string GetMenuItemText(HMENU hMenu, int index)
    {
        var sb = new System.Text.StringBuilder(512);
        var len = GetMenuStringW(hMenu.DangerousGetHandle(), (uint)index, sb, sb.Capacity, MenuFlags.MF_BYPOSITION);
        return len > 0 ? sb.ToString() : string.Empty;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true, EntryPoint = "GetMenuStringW")]
    static extern int GetMenuStringW(nint hMenu, uint uIDItem, System.Text.StringBuilder lpString, int nMaxCount, MenuFlags uFlag);
}
