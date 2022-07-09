using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public class AnimatableTargets : AnimatableMultiple
    {
        public List<Transform> targets = new List<Transform>();
        public override IEnumerable<Transform> Targets => targets;
    }
}