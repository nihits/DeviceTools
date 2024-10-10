using UnityEngine;

public class TestHierarchyStarter : MonoBehaviour
{
    private void OnMouseDown()
    {
        Anexas.DeviceTools.DeviceHierarchy.Instance.Show = !Anexas.DeviceTools.DeviceHierarchy.Instance.Show;
    }

    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            Anexas.DeviceTools.DeviceHierarchy.Instance.Show = !Anexas.DeviceTools.DeviceHierarchy.Instance.Show;
        }
    }
}
