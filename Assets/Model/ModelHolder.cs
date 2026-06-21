using UnityEngine;

public class ModelHolder : MonoBehaviour
{
    public Crazy8sModel model;

    void Awake()
    {
        model = new Crazy8sModel();
        model.Reset();
    }
}
