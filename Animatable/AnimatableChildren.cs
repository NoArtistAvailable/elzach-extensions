using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public class AnimatableChildren : AnimatableMultiple
    { 
        public override IEnumerable<Transform> Targets => transform.GetChildren();
    }
}