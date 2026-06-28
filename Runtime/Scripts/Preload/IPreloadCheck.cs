using UnityEngine;

namespace MolcaSDK.Preload
{
    public interface IPreloadCheck
    {
        Awaitable RunCheck();
    }
}
