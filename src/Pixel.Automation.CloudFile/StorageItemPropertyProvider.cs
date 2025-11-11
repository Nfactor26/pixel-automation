using System.Runtime.InteropServices;
using Vanara.PInvoke;
using Windows.Storage;
using static Vanara.PInvoke.Ole32;
using static Vanara.PInvoke.PropSys;

namespace Pixel.Automations.CloudFile;

internal sealed class StorageProviderItemProperty
{
    public int Id { get; set; }
    public string Value { get; set; }
    public string IconResource { get; set; }
}

internal sealed class StorageItemPropertyProvider : IDisposable
{
    private IPropertyStore? propertyStore;
    private bool isDisposed = false;

    public StorageItemPropertyProvider()
    {
        propertyStore = CreatePropertyStore() ?? throw new InvalidOperationException("Failed to create IPropertyStore");
    }

    private static IPropertyStore? CreatePropertyStore()
    {
        Guid IID_IPropertyStore = typeof(IPropertyStore).GUID;
        PSCreateMemoryPropertyStore(IID_IPropertyStore, out object propStore).ThrowIfFailed();
        return propStore as IPropertyStore;
    }

    public async Task<Dictionary<string, StorageProviderItemProperty>> GetPropertiesAsync(IStorageItemProperties storageItem)
    {
        ThrowIfDisposed();
        Dictionary<string, StorageProviderItemProperty> properties = new();
        var customStates = (await storageItem.Properties.RetrievePropertiesAsync(["System.StorageProviderCustomStates"]))?.FirstOrDefault();
        if (customStates?.Value is byte[] customStatesValue)
        {
            LoadByteArrayIntoPersistStream(customStatesValue);
            using PROPVARIANT stateList = GetProperty("System.ItemCustomState.StateList");
            if (stateList.Value is string[] states && states.Any())
            {
                using PROPVARIANT valueList = GetProperty("System.ItemCustomState.Values");
                string[] values = valueList.Value as string[] ?? new string[states.Count()];
                using PROPVARIANT iconList = GetProperty("System.ItemCustomState.IconReferences");
                string[] icons = iconList.Value as string[] ?? new string[states.Count()];
                int index = 0;
                foreach (var propertyID in states)
                {
                    properties.Add(propertyID, new StorageProviderItemProperty()
                    {
                        Id = int.Parse(propertyID),
                        Value = (values)[index],
                        IconResource = (icons)[index]
                    });
                    index++;
                }
            }
        }
        return properties;
    }

    private void LoadByteArrayIntoPersistStream(byte[] data)
    {
        if (data is not null)
        {
            IntPtr pData = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, pData, data.Length);
            if (propertyStore is IPersistSerializedPropStorage persistStream)
            {
                persistStream.SetPropertyStorage(pData, (uint)data.Length);
            }
            else
            {
                throw new InvalidOperationException("PropertyStore does not support IPersistSerializedPropStorage");
            }
            Marshal.FreeHGlobal(pData);
        }
    }

    PROPVARIANT GetProperty(string propertyKey)
    {
        PROPVARIANT result = new();
        PropSys.PSGetPropertyKeyFromName(propertyKey, out var key);
        propertyStore?.GetValue(key, result);
        return result;
    }

    void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(StorageItemPropertyProvider));
        }
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            Marshal.ReleaseComObject(propertyStore);
            propertyStore = null;
            isDisposed = true;
        }
    }
}
