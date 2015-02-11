using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class stateCheck : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(checkState());   
        }

        IEnumerator checkState()
        {
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return new WaitForSeconds(0.1f);//new WaitForSeconds(1f);

            // 2 frames after the flag is set it seems to be safe to initialise
            yield return null;
            yield return null;
            

            Core.Instance.editor();
        }
    }
}
