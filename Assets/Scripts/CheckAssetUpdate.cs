using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CheckAssetUpdate : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> _cubeInstanceHandle;

    private IEnumerator Start()
    {
        Debug.Log("=== Start CheckAssetUpdate ===");
        //false 不自动释放handle
        var initHandle = Addressables.InitializeAsync(false);
        yield return initHandle;
        Debug.Log($"Initialize Status = {initHandle.Status}");

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        Debug.Log($"CheckForCatalogUpdates Status = {checkHandle.Status}");

        List<string> catalogs = null;
        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            catalogs = checkHandle.Result;
            Debug.Log($"Catalog update count = {(catalogs == null ? 0 : catalogs.Count)}");
        }

        Addressables.Release(checkHandle);

        if (catalogs != null && catalogs.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;
            Debug.Log($"UpdateCatalogs Status = {updateHandle.Status}");
            Addressables.Release(updateHandle);
        }

        var cubeHandle = Addressables.LoadAssetAsync<GameObject>("Cube");
        yield return cubeHandle;
        Debug.Log($"Load Cube Status = {cubeHandle.Status}");
        if (cubeHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var go = Instantiate(cubeHandle.Result);
            var renderer = go.GetComponent<Renderer>();
            // if (renderer != null)
            // {
            //     var mat = new Material(Shader.Find("Standard"));
            //     mat.color = Color.green;
            //     renderer.sharedMaterial = mat;
            // }

            Debug.Log("sharedMaterial = " + renderer.sharedMaterial);
            Debug.Log("material name = " + (renderer.sharedMaterial ? renderer.sharedMaterial.name : "NULL"));
            Debug.Log("shader = " + (renderer.sharedMaterial && renderer.sharedMaterial.shader != null
                ? renderer.sharedMaterial.shader.name
                : "NULL"));

            _cubeInstanceHandle = cubeHandle;
        }

        var dllHandle = Addressables.LoadAssetAsync<TextAsset>("HotUpdate.dll");
        yield return dllHandle;
        Debug.Log($"Load HotUpdate.dll Status = {dllHandle.Status}");

        if (dllHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"HotUpdate.dll bytes length = {dllHandle.Result.bytes.Length}");

            Assembly hotUpdate = Assembly.Load(dllHandle.Result.bytes);
            Type type = hotUpdate.GetType("Hello");
            MethodInfo method = type?.GetMethod("Run", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            Debug.Log($"Type Hello found = {type != null}");
            Debug.Log($"Method Run found = {method != null}");

            method?.Invoke(null, null);
        }

        Addressables.Release(dllHandle);
        Addressables.Release(initHandle);
    }

    private void OnDestroy()
    {
        if (_cubeInstanceHandle.IsValid())
        {
            Addressables.ReleaseInstance(_cubeInstanceHandle);
        }
    }
}