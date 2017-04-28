using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum ControlPointType
{
    Start,
    End
};

public struct ControlPoint
{
    public Vector3 Control0;
    public Vector3 Control1;
    public Vector3 Control2;
    public Vector3 Control3;

    public Vector3 this[int index]
    {
        get
        {
            Assert.IsTrue(index >= 0 && index < 4,
                        "index " + index + " is out of range, should be 0..3");
            return  index == 0 ? Control0
                :   index == 1 ? Control1
                :   index == 2 ? Control2
                :   Control3;
        }
        set
        {
            Assert.IsTrue(index >= 0 && index < 4,
                        "index " + index + " is out of range, should be 0..3");
            switch (index)
            {
                case 0: Control0 = value; break;
                case 1: Control1 = value; break;
                case 2: Control2 = value; break;
                case 3: Control3 = value; break;
            }
        }
    }
}