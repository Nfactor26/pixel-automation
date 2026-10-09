using Pixel.Automation.Core.Components.Prefabs;

namespace Pixel.Automation.Editor.Notifications.Notfications;

public record OpenPrefabRequestNotification(string PrefabId, PrefabHostEntity PrefabHost);
