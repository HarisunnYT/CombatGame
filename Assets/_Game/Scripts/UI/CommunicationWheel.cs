using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationWheel : WorldSpaceUI
{
    [SerializeField]
    private CommunicationSlice[] slices;
    public CommunicationSlice[] Slices { get { return slices; } }
}
