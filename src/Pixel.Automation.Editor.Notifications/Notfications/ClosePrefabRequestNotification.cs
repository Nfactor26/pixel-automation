using Pixel.Automation.Core.Components.Prefabs;

namespace Pixel.Automation.Editor.Notifications;

public record ClosePrefabRequestNotification(string PrefabId, PrefabHostEntity PrefabHost, bool SaveChanges);
