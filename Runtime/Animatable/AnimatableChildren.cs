using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace elZach.Common
{
    public class AnimatableChildren : AnimatableMultiple
    {
#pragma warning disable CS4014
        public override IEnumerable<Transform> Targets => transform.GetChildren();
#pragma warning restore CS4014
        
    }
}