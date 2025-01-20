using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Animator _anim;
    private Animator _parentAnim;

    private void Start()
    {
        _parentAnim = transform.GetComponentInParent<Animator>();
        _anim = transform.GetComponent<Animator>();
    }

    private void Update()
    {
        _anim.SetBool("running", _parentAnim.GetBool("running"));
    }
}
