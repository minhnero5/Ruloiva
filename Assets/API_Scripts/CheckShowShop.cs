using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class CheckShowShop : MonoBehaviour
{
    [SerializeField] private GameObject shopBtn;

    void Start()
    {
        shopBtn.SetActive(IAPManager.Instance.canShowShop);
    }

    IEnumerator WaitProductFetch()
    {
        while (!IAPManager.Instance.isProductsFetched)
        {
            yield return null;
        }
        shopBtn.SetActive(IAPManager.Instance.canShowShop);
    }

}
