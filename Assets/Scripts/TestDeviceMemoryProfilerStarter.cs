using UnityEngine;

public class TestDeviceMemoryProfilerStarter : MonoBehaviour
{
    private void OnMouseDown()
    {
        Anexas.DeviceTools.DeviceMemoryProfiler.Instance.Show = !Anexas.DeviceTools.DeviceMemoryProfiler.Instance.Show;
    }

    void Update()
    {
        if (Input.GetKeyDown("`"))
        {
            Anexas.DeviceTools.DeviceMemoryProfiler.Instance.Show = !Anexas.DeviceTools.DeviceMemoryProfiler.Instance.Show;
        }
    }
}
