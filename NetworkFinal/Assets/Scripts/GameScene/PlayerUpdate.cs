using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System.Threading;

public class PlayerUpdate : MonoBehaviour
{
    public GameObject scrollView;
    public GameObject itemPrefab;
    public GameObject contentObj;

    private static List<string> userList = new List<string>();
    private static List<GameObject> userListObj = new List<GameObject>();

    private USERINFO info;

    private bool isUpdated = false;

    private void Start()
    {
        info = GameObject.Find("USERINFO").GetComponent<USERINFO>();       
    }

    private void Update()
    {
        if (!isUpdated && GameManager.Instance.IsStarted)
        {
            StartCoroutine(WaitUpdateTime());
        }
    }

    private IEnumerator WaitUpdateTime()
    {
        isUpdated = true;
        UserUpdate();
        yield return new WaitForSeconds(0.1f);
        isUpdated = false;
    }

    private void UserUpdate()
    {
        foreach(var obj in userListObj)
        {
            Destroy(obj);
        }
        userListObj.Clear();
        userList.Clear();
        NetworkManager.Instance.SendData(NetworkManager.Header.Update, info.Uid, "Player");
        string[] data = null;
        foreach(NetworkManager.NetworkData networkData in NetworkManager.Instance.networkQueue)
        {
            if(networkData.head == NetworkManager.Header.Update)
            {
                string dataFromServer = Encoding.UTF8.GetString(networkData.data);
                data = dataFromServer.Split(',');
            }
        }

        foreach(var inData in data)
        {
            userList.Add(inData);
        }

        foreach(string s in userList)
        {
            GameObject item = Instantiate(itemPrefab);

            item.GetComponentInChildren<TMP_Text>().text = s;

            item.transform.SetParent(contentObj.transform, false);

            userListObj.Add(item);
        }
    }
}
