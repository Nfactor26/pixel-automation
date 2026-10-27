using Pixel.Automation.Core.Components.Prefabs;

namespace Pixel.Automation.Editor.Notifications;

public record SavePrefabRequestNotification(string PrefabId, PrefabHostEntity PrefabHost);
