using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioClip _Clip;

    private AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //print(audio.isPlaying);
    }

    void OnEnable()
    {
        print("onenable");
        Test1();
    }

    void OnDisable()
    {
        print("ondisable");
        Test2();
    }

    [Button]
    private void Test1()
    {
        gameObject.SetActive(true);
    }

    [Button]
    private void Test2()
    {
        gameObject.SetActive(false);
    }
}