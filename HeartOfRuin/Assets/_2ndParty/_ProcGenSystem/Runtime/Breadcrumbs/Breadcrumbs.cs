using UnityEngine;

public class Breadcrumbs : MonoBehaviour
{
    public void Show() { this.gameObject.SetActive(true);  }
    public void Hide() { this.gameObject.SetActive(false); }
    public void Remove() { Destroy(this.gameObject); }
}
